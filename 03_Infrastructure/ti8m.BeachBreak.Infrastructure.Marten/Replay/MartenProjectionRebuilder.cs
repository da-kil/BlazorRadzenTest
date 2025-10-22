using Marten;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Services;

namespace ti8m.BeachBreak.Infrastructure.Marten.Replay;

/// <summary>
/// Rebuilds Marten snapshot projections from the event store
/// </summary>
public class MartenProjectionRebuilder : IProjectionRebuilder
{
    private readonly IDocumentStore store;
    private readonly IProjectionRegistry registry;
    private readonly ILogger<MartenProjectionRebuilder> logger;

    public MartenProjectionRebuilder(
        IDocumentStore store,
        IProjectionRegistry registry,
        ILogger<MartenProjectionRebuilder> logger)
    {
        this.store = store;
        this.registry = registry;
        this.logger = logger;
    }

    public async Task<long> CountEventsForProjectionAsync(string projectionName, CancellationToken ct = default)
    {
        var projection = registry.GetProjection(projectionName);
        if (projection == null)
        {
            throw new ArgumentException($"Projection '{projectionName}' not found", nameof(projectionName));
        }

        await using var session = store.QuerySession();

        // Count all events in the event store
        // For snapshots, we replay ALL events of the aggregate type
        var totalEvents = await session.Events.QueryAllRawEvents().LongCountAsync(ct);

        logger.LogInformation("Projection {ProjectionName} will replay {EventCount} events",
            projectionName, totalEvents);

        return totalEvents;
    }

    public async Task DeleteSnapshotsAsync(string projectionName, CancellationToken ct = default)
    {
        var projection = registry.GetProjection(projectionName);
        if (projection == null)
        {
            throw new ArgumentException($"Projection '{projectionName}' not found", nameof(projectionName));
        }

        if (!projection.IsRebuildable)
        {
            throw new InvalidOperationException($"Projection '{projectionName}' cannot be rebuilt");
        }

        logger.LogInformation("Deleting all snapshots for projection {ProjectionName}", projectionName);

        await using var session = store.LightweightSession();

        // Delete all documents of this type
        var documentType = projection.DocumentType;

        // Use reflection to call session.DeleteWhere<T>(x => true)
        var deleteWhereMethod = typeof(IDocumentSession)
            .GetMethods()
            .First(m => m.Name == "DeleteWhere" && m.IsGenericMethod && m.GetParameters().Length == 1);

        var genericMethod = deleteWhereMethod.MakeGenericMethod(documentType);

        // Create expression: x => true (delete all)
        var parameter = System.Linq.Expressions.Expression.Parameter(documentType, "x");
        var body = System.Linq.Expressions.Expression.Constant(true);
        var lambdaType = typeof(Func<,>).MakeGenericType(documentType, typeof(bool));
        var lambda = System.Linq.Expressions.Expression.Lambda(lambdaType, body, parameter);

        genericMethod.Invoke(session, new object[] { lambda });

        await session.SaveChangesAsync(ct);

        logger.LogInformation("Successfully deleted all snapshots for projection {ProjectionName}", projectionName);
    }

    public async Task RebuildProjectionAsync(
        string projectionName,
        IProgress<long>? progress = null,
        CancellationToken ct = default)
    {
        var projection = registry.GetProjection(projectionName);
        if (projection == null)
        {
            throw new ArgumentException($"Projection '{projectionName}' not found", nameof(projectionName));
        }

        if (!projection.IsRebuildable)
        {
            throw new InvalidOperationException($"Projection '{projectionName}' cannot be rebuilt");
        }

        logger.LogInformation("Starting projection rebuild for {ProjectionName}", projectionName);

        try
        {
            // Use Marten's projection daemon to rebuild
            var daemon = await store.BuildProjectionDaemonAsync();

            // Rebuild the projection by name
            // Note: For snapshot projections, Marten will replay all events for that aggregate type
            await daemon.RebuildProjectionAsync(projectionName, ct);

            logger.LogInformation("Successfully rebuilt projection {ProjectionName}", projectionName);

            // Report completion
            var totalEvents = await CountEventsForProjectionAsync(projectionName, ct);
            progress?.Report(totalEvents);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to rebuild projection {ProjectionName}", projectionName);
            throw;
        }
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateProjectionAsync(
        string projectionName,
        CancellationToken ct = default)
    {
        // Check if projection exists
        var projection = registry.GetProjection(projectionName);
        if (projection == null)
        {
            return (false, $"Projection '{projectionName}' not found");
        }

        // Check if projection is rebuildable
        if (!projection.IsRebuildable)
        {
            return (false, $"Projection '{projectionName}' cannot be rebuilt");
        }

        // Check if table exists
        var tableName = projection.TableName;
        if (string.IsNullOrEmpty(tableName))
        {
            return (false, $"Table name not configured for projection '{projectionName}'");
        }

        try
        {
            // Verify database connectivity
            await using var session = store.QuerySession();
            await session.Events.QueryAllRawEvents().Take(1).ToListAsync(ct);

            return (true, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Validation failed for projection {ProjectionName}", projectionName);
            return (false, $"Database connectivity error: {ex.Message}");
        }
    }
}
