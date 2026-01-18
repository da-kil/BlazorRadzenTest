using ti8m.BeachBreak.Application.Command;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.ProjectionReplayCommands;
using ti8m.BeachBreak.CommandApi.Dto;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.CommandApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for projection replay management.
/// </summary>
public static class ReplayEndpoints
{
    /// <summary>
    /// Maps projection replay management endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapReplayEndpoints(this WebApplication app)
    {
        var replayGroup = app.MapGroup("/c/api/v{version:apiVersion}/admin/replay")
            .WithTags("Projection Replay")
            .RequireAuthorization("Admin");

        // Start projection replay
        replayGroup.MapPost("/start", async (
            StartProjectionReplayRequestDto request,
            ICommandDispatcher commandDispatcher,
            UserContext userContext,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(request.ProjectionName))
                {
                    return Results.BadRequest("Projection name is required");
                }

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return Results.BadRequest("Reason is required");
                }

                if (!Guid.TryParse(userContext.Id, out var initiatedBy))
                {
                    logger.LogWarning("StartReplay failed: Unable to parse user ID from context");
                    return Results.Problem(
                        title: "User identification failed",
                        detail: "Unable to parse user ID from context",
                        statusCode: 401);
                }

                logger.LogStartProjectionReplay(request.ProjectionName, initiatedBy);

                var command = new StartProjectionReplayCommand(
                    request.ProjectionName,
                    initiatedBy,
                    request.Reason);

                var result = await commandDispatcher.SendAsync<Result<Guid>>(command, cancellationToken);

                if (result.Succeeded)
                {
                    logger.LogProjectionReplayStarted(result.Payload!, request.ProjectionName);
                    return Results.Accepted($"/c/api/v{{version:apiVersion}}/admin/replay/{result.Payload}", result.Payload);
                }
                else
                {
                    logger.LogProjectionNotRebuildable(request.ProjectionName);
                    return Results.Problem(
                        title: "Projection replay failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogStartProjectionReplayFailed(request.ProjectionName, ex.Message, ex);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while starting the projection replay",
                    statusCode: 500);
            }
        })
        .WithName("StartProjectionReplay")
        .WithSummary("Start projection replay")
        .WithDescription("Starts a background projection replay process")
        .Produces<Guid>(202)
        .Produces(400)
        .Produces(401)
        .Produces(500);

        // Cancel projection replay
        replayGroup.MapPost("/{replayId:guid}/cancel", async (
            Guid replayId,
            ICommandDispatcher commandDispatcher,
            UserContext userContext,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                if (!Guid.TryParse(userContext.Id, out var cancelledBy))
                {
                    logger.LogWarning("CancelReplay failed: Unable to parse user ID from context");
                    return Results.Problem(
                        title: "User identification failed",
                        detail: "Unable to parse user ID from context",
                        statusCode: 401);
                }

                logger.LogCancelProjectionReplay(replayId, cancelledBy);

                var command = new CancelProjectionReplayCommand(replayId, cancelledBy);

                var result = await commandDispatcher.SendAsync<Result>(command, cancellationToken);

                if (result.Succeeded)
                {
                    logger.LogProjectionReplayCancelled(replayId);
                    return Results.Ok($"Projection replay {replayId} cancelled successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Projection replay cancellation failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogCancelProjectionReplayFailed(replayId, ex.Message, ex);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while cancelling the projection replay",
                    statusCode: 500);
            }
        })
        .WithName("CancelProjectionReplay")
        .WithSummary("Cancel projection replay")
        .WithDescription("Cancels a running projection replay process")
        .Produces(200)
        .Produces(401)
        .Produces(404)
        .Produces(500);
    }
}