namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

public enum WorkflowState
{
    Assigned = 0,
    Initialized = 1,
    EmployeeInProgress = 2,
    ManagerInProgress = 3,
    BothInProgress = 4,

    // Submission phase (Phase 1 Read-Only)
    EmployeeSubmitted = 5,
    ManagerSubmitted = 6,
    BothSubmitted = 7,

    // Review phase
    InReview = 8,

    // Post-review confirmation
    ReviewFinished = 9,
    EmployeeReviewConfirmed = 10,

    // Final state (Phase 2 Read-Only)
    Finalized = 11
}
