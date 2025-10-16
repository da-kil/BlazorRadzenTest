namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Represents the workflow state of a questionnaire assignment.
/// This enum is synchronized with ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.WorkflowState.
/// </summary>
public enum WorkflowState
{
    Assigned,
    EmployeeInProgress,
    ManagerInProgress,
    BothInProgress,

    // Submission phase (Phase 1 Read-Only)
    EmployeeSubmitted,
    ManagerSubmitted,
    BothSubmitted,

    // Review phase
    InReview,

    // Post-review confirmation
    EmployeeReviewConfirmed,
    ManagerReviewConfirmed,

    // Final state (Phase 2 Read-Only)
    Finalized
}
