using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.ProjectionReplayQueries;
using ti8m.BeachBreak.Core.Infrastructure;

namespace ti8m.BeachBreak.QueryApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for projection replay queries.
/// </summary>
public static class ReplayEndpoints
{
    /// <summary>
    /// Maps projection replay query endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapReplayEndpoints(this WebApplication app)
    {
        var replayGroup = app.MapGroup("/q/api/v{version:apiVersion}/admin/replay")
            .WithTags("Replay")
            .RequireAuthorization("Admin");

        // Get replay status
        replayGroup.MapGet("/{replayId:guid}", async (
            Guid replayId,
            IQueryDispatcher queryDispatcher,
            [FromServices] ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                logger.LogRetrieveReplayStatusRequest(replayId);
                var query = new GetReplayStatusQuery(replayId);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result != null)
                {
                    logger.LogRetrieveReplayStatusSuccess(replayId);
                    if (result.Succeeded)
                    {
                        return Results.Ok(result.Payload);
                    }
                    else
                    {
                        return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                    }
                }
                else
                {
                    logger.LogReplayNotFound(replayId);
                    return Results.NotFound($"Replay {replayId} not found");
                }
            }
            catch (Exception ex)
            {
                logger.LogRetrieveReplayStatusError(ex, replayId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving the replay status",
                    statusCode: 500);
            }
        })
        .WithName("GetReplayStatus")
        .WithSummary("Get replay status")
        .WithDescription("Retrieves the status of a specific projection replay")
        .Produces<ProjectionReplayReadModel>(200)
        .Produces(404)
        .Produces(500);

        // Get replay history
        replayGroup.MapGet("/history", async (
            IQueryDispatcher queryDispatcher,
            [FromServices] ILogger logger,
            int limit = 50,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                logger.LogRetrieveReplayHistoryRequest(limit);
                var query = new GetReplayHistoryQuery(limit);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result.Succeeded)
                {
                    if (result.Payload != null)
                    {
                        var count = result.Payload.Count();
                        logger.LogRetrieveReplayHistorySuccess(count);
                        return Results.Ok(result.Payload);
                    }
                    else
                    {
                        return Results.Ok(Enumerable.Empty<ProjectionReplayReadModel>());
                    }
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogRetrieveReplayHistoryError(ex);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving the replay history",
                    statusCode: 500);
            }
        })
        .WithName("GetReplayHistory")
        .WithSummary("Get replay history")
        .WithDescription("Retrieves the history of projection replays with optional limit")
        .Produces<IEnumerable<ProjectionReplayReadModel>>(200)
        .Produces(500);

        // Get available projections
        replayGroup.MapGet("/projections", async (
            IQueryDispatcher queryDispatcher,
            [FromServices] ILogger logger,
            bool rebuildableOnly = true,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                logger.LogRetrieveProjectionsRequest(rebuildableOnly);
                var query = new GetAvailableProjectionsQuery(rebuildableOnly);
                var result = await queryDispatcher.QueryAsync(query, cancellationToken);

                if (result.Succeeded)
                {
                    if (result.Payload != null)
                    {
                        var count = result.Payload.Count();
                        logger.LogRetrieveProjectionsSuccess(count);
                        return Results.Ok(result.Payload);
                    }
                    else
                    {
                        return Results.Ok(Enumerable.Empty<ProjectionInfo>());
                    }
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogRetrieveProjectionsError(ex);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving the available projections",
                    statusCode: 500);
            }
        })
        .WithName("GetAvailableProjections")
        .WithSummary("Get available projections")
        .WithDescription("Retrieves available projections that can be replayed")
        .Produces<IEnumerable<ProjectionInfo>>(200)
        .Produces(500);
    }
}