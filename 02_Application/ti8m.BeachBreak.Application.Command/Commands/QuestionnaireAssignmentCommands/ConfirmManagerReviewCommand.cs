namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record ConfirmManagerReviewCommand(
    Guid AssignmentId,
    string ConfirmedBy,
    int ExpectedVersion) : ICommand<Result>;
