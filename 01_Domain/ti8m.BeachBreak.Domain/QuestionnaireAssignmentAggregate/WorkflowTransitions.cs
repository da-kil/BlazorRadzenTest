namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

/// <summary>
/// Defines all valid state transitions in the questionnaire workflow.
/// This is the single source of truth for state machine behavior.
/// </summary>
public static class WorkflowTransitions
{
    /// <summary>
    /// Forward transitions (normal workflow progression).
    /// These represent the typical flow from assignment to finalization.
    /// </summary>
    public static readonly Dictionary<WorkflowState, List<StateTransition>> ForwardTransitions = new()
    {
        [WorkflowState.Assigned] = new List<StateTransition>
        {
            new(WorkflowState.Initialized, "transitions.manager-starts-initialization"),
            new(WorkflowState.EmployeeInProgress, "transitions.employee-starts-filling"),
            new(WorkflowState.ManagerInProgress, "transitions.manager-starts-filling"),
            new(WorkflowState.BothInProgress, "transitions.both-start-filling")
        },

        [WorkflowState.Initialized] = new List<StateTransition>
        {
            new(WorkflowState.EmployeeInProgress, "transitions.employee-starts-filling"),
            new(WorkflowState.ManagerInProgress, "transitions.manager-starts-filling"),
            new(WorkflowState.BothInProgress, "transitions.both-start-filling")
        },

        [WorkflowState.EmployeeInProgress] = new List<StateTransition>
        {
            new(WorkflowState.BothInProgress, "transitions.manager-joins-filling"),
            new(WorkflowState.EmployeeSubmitted, "transitions.employee-submits")
        },

        [WorkflowState.ManagerInProgress] = new List<StateTransition>
        {
            new(WorkflowState.BothInProgress, "transitions.employee-joins-filling"),
            new(WorkflowState.ManagerSubmitted, "transitions.manager-submits")
        },

        [WorkflowState.BothInProgress] = new List<StateTransition>
        {
            new(WorkflowState.EmployeeSubmitted, "transitions.employee-submits-first"),
            new(WorkflowState.ManagerSubmitted, "transitions.manager-submits-first")
        },

        [WorkflowState.EmployeeSubmitted] = new List<StateTransition>
        {
            new(WorkflowState.BothSubmitted, "transitions.manager-completes-submission"),
            new(WorkflowState.Finalized, "transitions.auto-finalize-no-review")
        },

        [WorkflowState.ManagerSubmitted] = new List<StateTransition>
        {
            new(WorkflowState.BothSubmitted, "transitions.employee-completes-submission")
        },

        [WorkflowState.BothSubmitted] = new List<StateTransition>
        {
            new(WorkflowState.InReview, "transitions.manager-initiates-review")
        },

        [WorkflowState.InReview] = new List<StateTransition>
        {
            new(WorkflowState.ReviewFinished, "transitions.manager-finishes-review")
        },

        [WorkflowState.ReviewFinished] = new List<StateTransition>
        {
            new(WorkflowState.EmployeeReviewConfirmed, "transitions.employee-confirms-review")
        },

        [WorkflowState.EmployeeReviewConfirmed] = new List<StateTransition>
        {
            new(WorkflowState.Finalized, "transitions.manager-finalizes")
        },

        [WorkflowState.Finalized] = new List<StateTransition>() // Terminal state - no transitions
    };

    /// <summary>
    /// Backward transitions (reopening for corrections).
    /// Requires special permissions: Admin, HR, or TeamLead (for their own team).
    /// Finalized state CANNOT be reopened - must create new assignment.
    /// </summary>
    public static readonly Dictionary<WorkflowState, List<ReopenTransition>> BackwardTransitions = new()
    {
        [WorkflowState.Initialized] = new List<ReopenTransition>
        {
            new(WorkflowState.Assigned,
                "reopen.reset-initialization",
                new[] { "Admin", "HR", "TeamLead" })
        },

        [WorkflowState.EmployeeSubmitted] = new List<ReopenTransition>
        {
            new(WorkflowState.EmployeeInProgress,
                "reopen.employee-questionnaire-corrections",
                new[] { "Admin", "HR", "TeamLead" })
        },

        [WorkflowState.ManagerSubmitted] = new List<ReopenTransition>
        {
            new(WorkflowState.ManagerInProgress,
                "reopen.manager-questionnaire-corrections",
                new[] { "Admin", "HR", "TeamLead" })
        },

        [WorkflowState.BothSubmitted] = new List<ReopenTransition>
        {
            new(WorkflowState.BothInProgress,
                "reopen.both-questionnaires-corrections",
                new[] { "Admin", "HR", "TeamLead" })
        },

        [WorkflowState.ReviewFinished] = new List<ReopenTransition>
        {
            new(WorkflowState.InReview,
                "reopen.review-meeting-manager-revisions",
                new[] { "Admin", "HR", "TeamLead" }) // TeamLead can reopen review states for their team
        },

        [WorkflowState.EmployeeReviewConfirmed] = new List<ReopenTransition>
        {
            new(WorkflowState.ReviewFinished,
                "reopen.return-to-employee-signoff",
                new[] { "Admin", "HR", "TeamLead" }),
            new(WorkflowState.InReview,
                "reopen.review-meeting-after-confirmation",
                new[] { "Admin", "HR", "TeamLead" }) // TeamLead can reopen review states for their team
        }

        // Note: Finalized state is NOT in this dictionary - it cannot be reopened
        // If changes needed after finalization, create a new assignment
    };

    /// <summary>
    /// Represents a forward state transition.
    /// </summary>
    public record StateTransition(WorkflowState TargetState, string TriggerDescription);

    /// <summary>
    /// Represents a backward state transition (reopening).
    /// Includes allowed roles for authorization.
    /// </summary>
    public record ReopenTransition(
        WorkflowState TargetState,
        string ReopenReason,
        string[] AllowedRoles);
}
