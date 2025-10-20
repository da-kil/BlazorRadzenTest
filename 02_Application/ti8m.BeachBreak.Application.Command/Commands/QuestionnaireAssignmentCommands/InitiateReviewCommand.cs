namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record InitiateReviewCommand(
    Guid AssignmentId,
    Guid InitiatedByEmployeeId) : ICommand<Result>;
