using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Application.Query.Services;

namespace ti8m.BeachBreak.Application.Query.Queries.ProjectionReplayQueries;

public class ProjectionReplayQueryHandler :
    IQueryHandler<GetReplayStatusQuery, Result<ProjectionReplayReadModel>>,
    IQueryHandler<GetReplayHistoryQuery, Result<IEnumerable<ProjectionReplayReadModel>>>,
    IQueryHandler<GetAvailableProjectionsQuery, Result<IEnumerable<ProjectionInfo>>>
{
    private readonly IProjectionReplayRepository replayRepository;
    private readonly IProjectionRegistry projectionRegistry;
    private readonly ILogger<ProjectionReplayQueryHandler> logger;

    public ProjectionReplayQueryHandler(
        IProjectionReplayRepository replayRepository,
        IProjectionRegistry projectionRegistry,
        ILogger<ProjectionReplayQueryHandler> logger)
    {
        this.replayRepository = replayRepository;
        this.projectionRegistry = projectionRegistry;
        this.logger = logger;
    }

    public async Task<Result<ProjectionReplayReadModel>> HandleAsync(
        GetReplayStatusQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var replay = await replayRepository.GetByIdAsync(query.ReplayId, cancellationToken);

            if (replay == null)
            {
                return Result<ProjectionReplayReadModel>.Fail($"Replay {query.ReplayId} not found", 404);
            }

            return Result<ProjectionReplayReadModel>.Success(replay);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving replay status for {ReplayId}", query.ReplayId);
            return Result<ProjectionReplayReadModel>.Fail("An error occurred while retrieving the replay status", 500);
        }
    }

    public async Task<Result<IEnumerable<ProjectionReplayReadModel>>> HandleAsync(
        GetReplayHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await replayRepository.GetHistoryAsync(query.Limit, cancellationToken);
            return Result<IEnumerable<ProjectionReplayReadModel>>.Success(history);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving replay history");
            return Result<IEnumerable<ProjectionReplayReadModel>>.Fail("An error occurred while retrieving the replay history", 500);
        }
    }

    public Task<Result<IEnumerable<ProjectionInfo>>> HandleAsync(
        GetAvailableProjectionsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var projections = projectionRegistry.GetAllProjections();

            if (query.RebuildableOnly)
            {
                projections = projections.Where(p => p.IsRebuildable).ToList();
            }

            return Task.FromResult(Result<IEnumerable<ProjectionInfo>>.Success(projections.AsEnumerable()));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving available projections");
            return Task.FromResult(Result<IEnumerable<ProjectionInfo>>.Fail("An error occurred while retrieving available projections", 500));
        }
    }
}
