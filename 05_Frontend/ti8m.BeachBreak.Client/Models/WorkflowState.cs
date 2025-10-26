namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Represents the workflow state of a questionnaire assignment.
/// This enum is synchronized with ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.WorkflowState.
/// </summary>
public enum WorkflowState
{
    Assigned = 0,
    EmployeeInProgress = 1,
    ManagerInProgress = 2,
    BothInProgress = 3,

    // Submission phase (Phase 1 Read-Only)
    EmployeeSubmitted = 4,
    ManagerSubmitted = 5,
    BothSubmitted = 6,

    // Review phase
    InReview = 7,

    // Post-review confirmation
    EmployeeReviewConfirmed = 8,
    ManagerReviewConfirmed = 9,

    // Final state (Phase 2 Read-Only)
    Finalized = 10
}
