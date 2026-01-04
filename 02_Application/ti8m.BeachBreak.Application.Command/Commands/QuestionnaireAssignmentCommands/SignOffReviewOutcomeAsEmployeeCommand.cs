namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command for employee to sign-off on review outcome.
/// This is the intermediate step after manager finishes review meeting
/// but before final employee confirmation.
/// Transitions from AwaitingEmployeeSignOff to EmployeeReviewConfirmed state.
/// </summary>
public record SignOffReviewOutcomeAsEmployeeCommand(
    Guid AssignmentId,
    Guid SignedOffByEmployeeId,
    string? SignOffComments,
    int? ExpectedVersion = null) : ICommand<Result>;