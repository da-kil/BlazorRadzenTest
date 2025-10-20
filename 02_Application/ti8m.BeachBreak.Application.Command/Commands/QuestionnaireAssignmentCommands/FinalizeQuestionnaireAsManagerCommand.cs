namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command for manager to finalize the questionnaire after employee confirmation.
/// This is the final step in the review process.
/// Transitions from EmployeeReviewConfirmed to Finalized state.
/// Questionnaire becomes permanently locked and archived.
/// </summary>
public record FinalizeQuestionnaireAsManagerCommand(
    Guid AssignmentId,
    Guid FinalizedByEmployeeId,
    string? ManagerFinalNotes,
    int? ExpectedVersion = null) : ICommand<Result>;
