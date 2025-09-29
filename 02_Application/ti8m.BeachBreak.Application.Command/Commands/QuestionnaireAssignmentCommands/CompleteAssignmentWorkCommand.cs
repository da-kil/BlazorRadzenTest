namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class CompleteAssignmentWorkCommand : ICommand<Result>
{
    public Guid AssignmentId { get; init; }

    public CompleteAssignmentWorkCommand(Guid assignmentId)
    {
        AssignmentId = assignmentId;
    }
}