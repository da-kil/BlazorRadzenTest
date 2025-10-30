namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to delete an existing goal from a questionnaire assignment.
/// </summary>
public record DeleteGoalCommand(
    Guid AssignmentId,
    Guid GoalId,
    Guid DeletedByEmployeeId) : ICommand<Result>;
