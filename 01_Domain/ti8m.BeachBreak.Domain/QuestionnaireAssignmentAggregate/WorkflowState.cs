namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

public enum WorkflowState
{
    Assigned,
    EmployeeInProgress,
    ManagerInProgress,
    BothInProgress,
    EmployeeConfirmed,
    ManagerConfirmed,
    BothConfirmed,
    InReview,
    EmployeeReviewConfirmed,
    Finalized
}
