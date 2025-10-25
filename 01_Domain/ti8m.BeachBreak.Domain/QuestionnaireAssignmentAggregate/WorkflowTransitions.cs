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
            new(WorkflowState.EmployeeInProgress, "Employee starts filling sections"),
            new(WorkflowState.ManagerInProgress, "Manager starts filling sections"),
            new(WorkflowState.BothInProgress, "Both start filling sections")
        },

        [WorkflowState.EmployeeInProgress] = new List<StateTransition>
        {
            new(WorkflowState.BothInProgress, "Manager starts filling sections"),
            new(WorkflowState.EmployeeSubmitted, "Employee submits questionnaire")
        },

        [WorkflowState.ManagerInProgress] = new List<StateTransition>
        {
            new(WorkflowState.BothInProgress, "Employee starts filling sections"),
            new(WorkflowState.ManagerSubmitted, "Manager submits questionnaire")
        },

        [WorkflowState.BothInProgress] = new List<StateTransition>
        {
            new(WorkflowState.EmployeeSubmitted, "Employee submits first"),
            new(WorkflowState.ManagerSubmitted, "Manager submits first")
        },

        [WorkflowState.EmployeeSubmitted] = new List<StateTransition>
        {
            new(WorkflowState.BothSubmitted, "Manager submits questionnaire"),
            new(WorkflowState.Finalized, "Auto-finalize (no manager review required)")
        },

        [WorkflowState.ManagerSubmitted] = new List<StateTransition>
        {
            new(WorkflowState.BothSubmitted, "Employee submits questionnaire")
        },

        [WorkflowState.BothSubmitted] = new List<StateTransition>
        {
            new(WorkflowState.InReview, "Manager initiates review meeting")
        },

        [WorkflowState.InReview] = new List<StateTransition>
        {
            new(WorkflowState.ManagerReviewConfirmed, "Manager finishes review meeting")
        },

        [WorkflowState.ManagerReviewConfirmed] = new List<StateTransition>
        {
            new(WorkflowState.EmployeeReviewConfirmed, "Employee confirms review outcome")
        },

        [WorkflowState.EmployeeReviewConfirmed] = new List<StateTransition>
        {
            new(WorkflowState.Finalized, "Manager finalizes questionnaire")
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
        [WorkflowState.EmployeeSubmitted] = new List<ReopenTransition>
        {
            new(WorkflowState.EmployeeInProgress,
                "Reopen employee questionnaire for corrections",
                new[] { "Admin", "HR", "TeamLead" })
        },

        [WorkflowState.ManagerSubmitted] = new List<ReopenTransition>
        {
            new(WorkflowState.ManagerInProgress,
                "Reopen manager questionnaire for corrections",
                new[] { "Admin", "HR", "TeamLead" })
        },

        [WorkflowState.BothSubmitted] = new List<ReopenTransition>
        {
            new(WorkflowState.BothInProgress,
                "Reopen both questionnaires for corrections",
                new[] { "Admin", "HR", "TeamLead" })
        },

        [WorkflowState.ManagerReviewConfirmed] = new List<ReopenTransition>
        {
            new(WorkflowState.InReview,
                "Reopen review meeting for manager revisions",
                new[] { "Admin", "HR", "TeamLead" }) // TeamLead can reopen review states for their team
        },

        [WorkflowState.EmployeeReviewConfirmed] = new List<ReopenTransition>
        {
            new(WorkflowState.InReview,
                "Reopen review meeting after employee confirmation",
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
