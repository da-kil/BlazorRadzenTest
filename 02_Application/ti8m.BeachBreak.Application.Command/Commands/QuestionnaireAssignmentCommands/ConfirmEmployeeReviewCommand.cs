namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record ConfirmEmployeeReviewCommand(
    Guid AssignmentId,
    string ConfirmedBy) : ICommand<Result>;
