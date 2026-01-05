namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command for employee to confirm the review outcome.
/// Employee cannot reject but can add comments about the review.
/// Transitions from ReviewFinished to EmployeeReviewConfirmed state.
/// </summary>
public record ConfirmReviewOutcomeAsEmployeeCommand(
    Guid AssignmentId,
    Guid ConfirmedByEmployeeId,
    string? EmployeeComments,
    int? ExpectedVersion = null) : ICommand<Result>;
