namespace ti8m.BeachBreak.Core.Domain.SharedKernel;

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
