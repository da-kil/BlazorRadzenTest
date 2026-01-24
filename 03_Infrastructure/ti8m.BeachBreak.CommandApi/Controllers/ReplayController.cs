using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.ProjectionReplayCommands;
using ti8m.BeachBreak.CommandApi.Dto;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/admin/replay")]
[Authorize(Policy = "Admin")]
public class ReplayController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly UserContext userContext;
    private readonly ILogger<ReplayController> logger;

    public ReplayController(
        ICommandDispatcher commandDispatcher,
        UserContext userContext,
        ILogger<ReplayController> logger)
    {
        this.commandDispatcher = commandDispatcher;
        this.userContext = userContext;
        this.logger = logger;
    }

    [HttpPost("start")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartReplay([FromBody] StartProjectionReplayRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return CreateResponse(Result<Guid>.Fail("Invalid model state", 400));

            if (string.IsNullOrWhiteSpace(request.ProjectionName))
                return CreateResponse(Result<Guid>.Fail("Projection name is required", 400));

            if (string.IsNullOrWhiteSpace(request.Reason))
                return CreateResponse(Result<Guid>.Fail("Reason is required", 400));

            if (!Guid.TryParse(userContext.Id, out var initiatedBy))
            {
                logger.LogWarning("StartReplay failed: Unable to parse user ID from context");
                return CreateResponse(Result<Guid>.Fail("User identification failed", StatusCodes.Status401Unauthorized));
            }

            logger.LogStartProjectionReplay(request.ProjectionName, initiatedBy);

            var command = new StartProjectionReplayCommand(
                request.ProjectionName,
                initiatedBy,
                request.Reason);

            var result = await commandDispatcher.SendAsync<Result<Guid>>(command);

            if (result.Succeeded)
            {
                logger.LogProjectionReplayStarted(result.Payload!, request.ProjectionName);
            }
            else
            {
                logger.LogProjectionNotRebuildable(request.ProjectionName);
            }

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogStartProjectionReplayFailed(request.ProjectionName, ex.Message, ex);
            return CreateResponse(Result<Guid>.Fail("An error occurred while starting the projection replay", 500));
        }
    }

    [HttpPost("{replayId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelReplay(Guid replayId)
    {
        try
        {
            if (!Guid.TryParse(userContext.Id, out var cancelledBy))
            {
                logger.LogWarning("CancelReplay failed: Unable to parse user ID from context");
                return CreateResponse(Result.Fail("User identification failed", StatusCodes.Status401Unauthorized));
            }

            logger.LogCancelProjectionReplay(replayId, cancelledBy);

            var command = new CancelProjectionReplayCommand(replayId, cancelledBy);

            var result = await commandDispatcher.SendAsync<Result>(command);

            if (result.Succeeded)
            {
                logger.LogProjectionReplayCancelled(replayId);
            }

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogCancelProjectionReplayFailed(replayId, ex.Message, ex);
            return CreateResponse(Result.Fail("An error occurred while cancelling the projection replay", 500));
        }
    }
}
