namespace ti8m.BeachBreak.Application.Command.Commands.ProjectionReplayCommands;

public class CancelProjectionReplayCommand : ICommand<Result>
{
    public Guid ReplayId { get; init; }
    public Guid CancelledBy { get; init; }

    public CancelProjectionReplayCommand(Guid replayId, Guid cancelledBy)
    {
        ReplayId = replayId;
        CancelledBy = cancelledBy;
    }
}
