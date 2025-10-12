namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command for employee to confirm the review outcome.
/// Employee cannot reject but can add comments about the review.
/// Transitions from ManagerReviewConfirmed to EmployeeReviewConfirmed state.
/// </summary>
public record ConfirmReviewOutcomeAsEmployeeCommand(
    Guid AssignmentId,
    string ConfirmedBy,
    string? EmployeeComments,
    int? ExpectedVersion = null) : ICommand<Result>;
