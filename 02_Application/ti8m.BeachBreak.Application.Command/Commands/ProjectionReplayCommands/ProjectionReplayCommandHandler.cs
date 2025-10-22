using Microsoft.AspNetCore.Http;
using ti8m.BeachBreak.Application.Command.Services;

namespace ti8m.BeachBreak.Application.Command.Commands.ProjectionReplayCommands;

public class ProjectionReplayCommandHandler :
    ICommandHandler<StartProjectionReplayCommand, Result<Guid>>,
    ICommandHandler<CancelProjectionReplayCommand, Result>
{
    private readonly IProjectionReplayService replayService;

    public ProjectionReplayCommandHandler(IProjectionReplayService replayService)
    {
        this.replayService = replayService;
    }

    public async Task<Result<Guid>> HandleAsync(
        StartProjectionReplayCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await replayService.StartReplayAsync(
                command.ProjectionName,
                command.InitiatedBy,
                command.Reason,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Fail($"Failed to start replay: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result> HandleAsync(
        CancelProjectionReplayCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await replayService.CancelReplayAsync(
                command.ReplayId,
                command.CancelledBy,
                cancellationToken);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to cancel replay: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}
