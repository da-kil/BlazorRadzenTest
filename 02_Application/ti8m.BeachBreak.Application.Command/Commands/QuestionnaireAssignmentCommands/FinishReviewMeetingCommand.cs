namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to finish a review meeting.
/// Manager finishes the review meeting after making any necessary edits.
/// Transitions from InReview to ManagerReviewConfirmed state.
/// </summary>
public record FinishReviewMeetingCommand(
    Guid AssignmentId,
    Guid FinishedByEmployeeId,
    string? ReviewSummary,
    int? ExpectedVersion = null) : ICommand<Result>;
