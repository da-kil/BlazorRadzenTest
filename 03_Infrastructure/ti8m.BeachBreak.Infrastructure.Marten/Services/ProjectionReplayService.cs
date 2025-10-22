using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Application.Command.Services;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Core.Infrastructure.Configuration;
using ti8m.BeachBreak.Domain.ProjectionReplayAggregate;
using ti8m.BeachBreak.Infrastructure.Marten.Replay;

namespace ti8m.BeachBreak.Infrastructure.Marten.Services;

/// <summary>
/// Orchestrates projection replay operations with background execution
/// </summary>
public class ProjectionReplayService : IProjectionReplayService
{
    private readonly IProjectionRegistry registry;
    private readonly IProjectionRebuilder rebuilder;
    private readonly IProjectionReplayAggregateRepository repository;
    private readonly ProjectionReplaySettings settings;
    private readonly ILogger<ProjectionReplayService> logger;

    public ProjectionReplayService(
        IProjectionRegistry registry,
        IProjectionRebuilder rebuilder,
        IProjectionReplayAggregateRepository repository,
        IOptions<ProjectionReplaySettings> settings,
        ILogger<ProjectionReplayService> logger)
    {
        this.registry = registry;
        this.rebuilder = rebuilder;
        this.repository = repository;
        this.settings = settings.Value;
        this.logger = logger;
    }

    public async Task<Result<Guid>> StartReplayAsync(
        string projectionName,
        Guid initiatedBy,
        string reason,
        CancellationToken ct = default)
    {
        try
        {
            // 1. Check if replay is enabled
            if (!settings.Enabled)
            {
                logger.LogWarning("Projection replay is disabled in configuration");
                return Result<Guid>.Fail("Projection replay is currently disabled. Enable it in configuration (ProjectionReplay:Enabled) to use this feature.", 403);
            }

            // 2. Validate projection exists
            var projection = registry.GetProjection(projectionName);
            if (projection == null)
            {
                logger.LogWarning("Projection '{ProjectionName}' not found", projectionName);
                return Result<Guid>.Fail($"Projection '{projectionName}' not found", 400);
            }

            // 3. Validate projection is rebuildable
            if (!projection.IsRebuildable)
            {
                logger.LogWarning("Projection '{ProjectionName}' cannot be rebuilt", projectionName);
                return Result<Guid>.Fail($"Projection '{projectionName}' cannot be rebuilt", 400);
            }

            // 4. Validate projection readiness
            var (isValid, errorMessage) = await rebuilder.ValidateProjectionAsync(projectionName, ct);
            if (!isValid)
            {
                logger.LogWarning("Projection '{ProjectionName}' validation failed: {Error}",
                    projectionName, errorMessage);
                return Result<Guid>.Fail($"Validation failed: {errorMessage}", 400);
            }

            // 5. Create ProjectionReplay aggregate (event sourced)
            var replayId = Guid.NewGuid();
            var replayAggregate = ProjectionReplay.Start(replayId, projectionName, initiatedBy, reason);

            // 6. Store aggregate (emits ProjectionReplayStarted event)
            await repository.StoreAsync(replayAggregate, ct);

            logger.LogInformation(
                "Replay {ReplayId} started for projection '{ProjectionName}' by {InitiatedBy}",
                replayId, projectionName, initiatedBy);

            // 7. Start background job (fire and forget)
            _ = Task.Run(() => ExecuteReplayAsync(replayId, projectionName, ct), CancellationToken.None);

            return Result<Guid>.Success(replayId, 202); // 202 Accepted - processing in background
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start replay for projection '{ProjectionName}'", projectionName);
            return Result<Guid>.Fail($"Failed to start replay: {ex.Message}", 500);
        }
    }

    public async Task<Result> CancelReplayAsync(Guid replayId, Guid cancelledBy, CancellationToken ct = default)
    {
        try
        {
            // Load the aggregate
            var replay = await repository.LoadAsync<ProjectionReplay>(replayId, cancellationToken: ct);
            if (replay == null)
            {
                logger.LogWarning("Replay {ReplayId} not found", replayId);
                return Result.Fail($"Replay {replayId} not found", 404);
            }

            // Cancel the replay
            replay.Cancel(cancelledBy);

            // Store updated aggregate
            await repository.StoreAsync(replay, ct);

            logger.LogInformation("Replay {ReplayId} cancelled by {CancelledBy}", replayId, cancelledBy);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to cancel replay {ReplayId}", replayId);
            return Result.Fail($"Failed to cancel replay: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Background job that executes the replay workflow
    /// </summary>
    private async Task ExecuteReplayAsync(Guid replayId, string projectionName, CancellationToken ct)
    {
        try
        {
            logger.LogInformation("Executing replay {ReplayId} for projection '{ProjectionName}'",
                replayId, projectionName);

            // 1. Count total events (Validating phase)
            await UpdateStatusAsync(replayId, ReplayStatus.Validating, 0, ct);

            var totalEvents = await rebuilder.CountEventsForProjectionAsync(projectionName, ct);

            logger.LogInformation("Replay {ReplayId}: Will replay {EventCount} events",
                replayId, totalEvents);

            // Update aggregate with total event count
            var replay = await repository.LoadRequiredAsync<ProjectionReplay>(replayId, cancellationToken: ct);
            replay.SetTotalEvents(totalEvents);
            await repository.StoreAsync(replay, ct);

            // 2. Delete existing snapshots (DeletingSnapshots phase)
            await UpdateStatusAsync(replayId, ReplayStatus.DeletingSnapshots, 0, ct);

            logger.LogInformation("Replay {ReplayId}: Deleting snapshots for projection '{ProjectionName}'",
                replayId, projectionName);

            await rebuilder.DeleteSnapshotsAsync(projectionName, ct);

            logger.LogInformation("Replay {ReplayId}: Snapshots deleted", replayId);

            // 3. Rebuild projection with progress tracking (Replaying phase)
            await UpdateStatusAsync(replayId, ReplayStatus.Replaying, 0, ct);

            logger.LogInformation("Replay {ReplayId}: Starting projection rebuild", replayId);

            // Progress reporter
            var lastReportedProgress = 0L;
            var progress = new Progress<long>(async processedEvents =>
            {
                // Report progress every 10% or every 1000 events
                if (processedEvents - lastReportedProgress >= 1000 ||
                    (totalEvents > 0 && (processedEvents * 100 / totalEvents) != (lastReportedProgress * 100 / totalEvents)))
                {
                    await UpdateProgressAsync(replayId, ReplayStatus.Replaying, processedEvents, ct);
                    lastReportedProgress = processedEvents;
                }
            });

            await rebuilder.RebuildProjectionAsync(projectionName, progress, ct);

            logger.LogInformation("Replay {ReplayId}: Projection rebuild completed", replayId);

            // 4. Mark as completed
            replay = await repository.LoadRequiredAsync<ProjectionReplay>(replayId, cancellationToken: ct);
            replay.UpdateProgress(ReplayStatus.Replaying, totalEvents); // Final progress update
            replay.Complete();
            await repository.StoreAsync(replay, ct);

            logger.LogInformation("Replay {ReplayId} completed successfully", replayId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Replay {ReplayId} failed for projection '{ProjectionName}'",
                replayId, projectionName);

            try
            {
                // Mark as failed
                var replay = await repository.LoadRequiredAsync<ProjectionReplay>(replayId, cancellationToken: ct);
                replay.Fail(ex.Message);
                await repository.StoreAsync(replay, ct);
            }
            catch (Exception innerEx)
            {
                logger.LogError(innerEx, "Failed to mark replay {ReplayId} as failed", replayId);
            }
        }
    }

    private async Task UpdateStatusAsync(Guid replayId, ReplayStatus status, long processedEvents, CancellationToken ct)
    {
        try
        {
            var replay = await repository.LoadRequiredAsync<ProjectionReplay>(replayId, cancellationToken: ct);
            replay.UpdateProgress(status, processedEvents);
            await repository.StoreAsync(replay, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update status for replay {ReplayId}", replayId);
        }
    }

    private async Task UpdateProgressAsync(Guid replayId, ReplayStatus status, long processedEvents, CancellationToken ct)
    {
        try
        {
            var replay = await repository.LoadRequiredAsync<ProjectionReplay>(replayId, cancellationToken: ct);
            replay.UpdateProgress(status, processedEvents);
            await repository.StoreAsync(replay, ct);

            logger.LogDebug("Replay {ReplayId}: Progress {ProcessedEvents} ({Percentage}%)",
                replayId, processedEvents, replay.ProgressPercentage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update progress for replay {ReplayId}", replayId);
        }
    }
}
