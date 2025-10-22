namespace ti8m.BeachBreak.Application.Command.Commands.ProjectionReplayCommands;

public class StartProjectionReplayCommand : ICommand<Result<Guid>>
{
    public string ProjectionName { get; init; } = null!;
    public Guid InitiatedBy { get; init; }
    public string Reason { get; init; } = null!;

    public StartProjectionReplayCommand(string projectionName, Guid initiatedBy, string reason)
    {
        ProjectionName = projectionName;
        InitiatedBy = initiatedBy;
        Reason = reason;
    }
}
