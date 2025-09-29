namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class StartAssignmentWorkCommand : ICommand<Result>
{
    public Guid AssignmentId { get; init; }

    public StartAssignmentWorkCommand(Guid assignmentId)
    {
        AssignmentId = assignmentId;
    }
}