namespace ti8m.BeachBreak.Client.Models;

public static class WorkflowStateHelper
{
    private static readonly List<WorkflowState> StateOrder = new()
    {
        WorkflowState.Assigned,
        WorkflowState.EmployeeInProgress,
        WorkflowState.ManagerInProgress,
        WorkflowState.BothInProgress,
        WorkflowState.EmployeeSubmitted,
        WorkflowState.ManagerSubmitted,
        WorkflowState.BothSubmitted,
        WorkflowState.InReview,
        WorkflowState.ReviewFinished,
        WorkflowState.EmployeeReviewConfirmed,
        WorkflowState.Finalized
    };

    public static bool IsStateAfter(WorkflowState currentState, WorkflowState referenceState)
    {
        var currentIndex = StateOrder.IndexOf(currentState);
        var referenceIndex = StateOrder.IndexOf(referenceState);
        return currentIndex > referenceIndex;
    }

    public static bool IsStateBefore(WorkflowState currentState, WorkflowState referenceState)
    {
        var currentIndex = StateOrder.IndexOf(currentState);
        var referenceIndex = StateOrder.IndexOf(referenceState);
        return currentIndex < referenceIndex;
    }

    public static string GetStateDisplayName(WorkflowState state)
    {
        return state switch
        {
            WorkflowState.Assigned => "workflow-states.assigned",
            WorkflowState.EmployeeInProgress => "workflow-states.employee-working",
            WorkflowState.ManagerInProgress => "workflow-states.manager-working",
            WorkflowState.BothInProgress => "workflow-states.both-working",
            WorkflowState.EmployeeSubmitted => "workflow-states.employee-submitted",
            WorkflowState.ManagerSubmitted => "workflow-states.manager-submitted",
            WorkflowState.BothSubmitted => "workflow-states.both-submitted-ready-review",
            WorkflowState.InReview => "workflow-states.in-review",
            WorkflowState.ReviewFinished => "workflow-states.review-finished",
            WorkflowState.EmployeeReviewConfirmed => "workflow-states.employee-review-confirmed",
            WorkflowState.Finalized => "workflow-states.finalized",
            _ => state.ToString()
        };
    }

    public static string GetStateColor(WorkflowState state)
    {
        return state switch
        {
            WorkflowState.Assigned => "var(--rz-base-500)",
            WorkflowState.EmployeeInProgress or WorkflowState.ManagerInProgress or WorkflowState.BothInProgress => "var(--rz-primary)",
            WorkflowState.EmployeeSubmitted or WorkflowState.ManagerSubmitted or WorkflowState.BothSubmitted => "var(--rz-secondary)",
            WorkflowState.InReview => "var(--rz-warning)",
            WorkflowState.ReviewFinished => "var(--rz-success)",
            WorkflowState.EmployeeReviewConfirmed => "var(--rz-success)",
            WorkflowState.Finalized => "var(--rz-success-dark)",
            _ => "var(--rz-base-500)"
        };
    }

    public static string GetStateIcon(WorkflowState state)
    {
        return state switch
        {
            WorkflowState.Assigned => "assignment",
            WorkflowState.EmployeeInProgress or WorkflowState.ManagerInProgress or WorkflowState.BothInProgress => "edit",
            WorkflowState.EmployeeSubmitted or WorkflowState.ManagerSubmitted or WorkflowState.BothSubmitted => "send",
            WorkflowState.InReview => "rate_review",
            WorkflowState.ReviewFinished => "verified",
            WorkflowState.EmployeeReviewConfirmed => "verified",
            WorkflowState.Finalized => "lock",
            _ => "help"
        };
    }

    public static bool CanEmployeeEdit(QuestionnaireAssignment assignment)
    {
        if (assignment.IsLocked) return false;

        var state = assignment.WorkflowState;

        // Employee can edit until they themselves submit
        // Employee can continue editing even if manager has already submitted (ManagerSubmitted)
        // Employee and manager can work simultaneously before any submission
        // Employee is READ-ONLY during InReview (manager-led review meeting)
        return state is WorkflowState.Assigned or WorkflowState.EmployeeInProgress or WorkflowState.ManagerInProgress or WorkflowState.BothInProgress or WorkflowState.ManagerSubmitted;

        // Cannot edit after employee submits: EmployeeSubmitted, BothSubmitted
        // Cannot edit during review: InReview
        // Cannot edit after finalization: Finalized
    }

    public static bool CanManagerEdit(QuestionnaireAssignment assignment)
    {
        if (assignment.IsLocked) return false;

        var state = assignment.WorkflowState;

        // Manager can edit until they themselves submit
        // Manager can continue editing even if employee has already submitted (EmployeeSubmitted)
        // During InReview, manager has special editing privileges (handled via CanManagerEditDuringReview)
        return state is WorkflowState.Assigned or WorkflowState.ManagerInProgress or WorkflowState.BothInProgress or WorkflowState.EmployeeSubmitted;

        // Cannot edit after manager submits: ManagerSubmitted, BothSubmitted
        // Cannot edit after finalization: Finalized
    }

    public static bool CanManagerEditDuringReview(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState == WorkflowState.InReview;
    }

    public static bool CanEmployeeSubmit(QuestionnaireAssignment assignment)
    {
        var state = assignment.WorkflowState;
        // Employee can submit when they're in progress, or when manager has already submitted
        return state is WorkflowState.EmployeeInProgress or WorkflowState.BothInProgress or WorkflowState.ManagerSubmitted;
    }

    public static bool CanManagerSubmit(QuestionnaireAssignment assignment)
    {
        var state = assignment.WorkflowState;
        return state is WorkflowState.ManagerInProgress or WorkflowState.BothInProgress or WorkflowState.EmployeeSubmitted;
    }

    public static bool CanInitiateReview(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState == WorkflowState.BothSubmitted;
    }

    public static bool CanManagerFinishReviewMeeting(QuestionnaireAssignment assignment)
    {
        // Manager finishes the review meeting, transitioning from InReview to ReviewFinished
        return assignment.WorkflowState == WorkflowState.InReview;
    }

    public static bool CanEmployeeSignOff(QuestionnaireAssignment assignment)
    {
        // Employee signs-off on review outcome when review is finished
        return assignment.WorkflowState == WorkflowState.ReviewFinished;
    }

    public static bool CanEmployeeConfirmReview(QuestionnaireAssignment assignment)
    {
        // Employee can confirm review when review is finished
        return assignment.WorkflowState == WorkflowState.ReviewFinished;
    }

    public static bool CanManagerFinalize(QuestionnaireAssignment assignment)
    {
        // Manager finalizes after employee confirms the review outcome
        return assignment.WorkflowState == WorkflowState.EmployeeReviewConfirmed;
    }

    public static string GetNextActionForEmployee(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState switch
        {
            WorkflowState.Assigned => "actions.employee.start-completing-sections",
            WorkflowState.EmployeeInProgress => "actions.employee.complete-submit-sections",
            WorkflowState.BothInProgress => "actions.employee.complete-submit-sections",
            WorkflowState.EmployeeSubmitted => "actions.employee.waiting-manager-submit",
            WorkflowState.ManagerSubmitted => "actions.employee.complete-submit-sections",
            WorkflowState.BothSubmitted => "actions.employee.waiting-manager-review",
            WorkflowState.InReview => "actions.employee.review-meeting-readonly",
            WorkflowState.ReviewFinished => "actions.employee.signoff-review-outcome",
            WorkflowState.EmployeeReviewConfirmed => "actions.employee.waiting-manager-finalize",
            WorkflowState.Finalized => "actions.employee.questionnaire-finalized",
            _ => "actions.employee.no-action-required"
        };
    }

    public static string GetNextActionForManager(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState switch
        {
            WorkflowState.Assigned => "actions.manager.start-completing-sections",
            WorkflowState.ManagerInProgress => "actions.manager.complete-submit-sections",
            WorkflowState.BothInProgress => "actions.manager.complete-submit-sections",
            WorkflowState.EmployeeSubmitted => "actions.manager.complete-submit-sections",
            WorkflowState.ManagerSubmitted => "actions.manager.waiting-employee-submit",
            WorkflowState.BothSubmitted => "actions.manager.initiate-review-meeting",
            WorkflowState.InReview => "actions.manager.conduct-review-meeting",
            WorkflowState.ReviewFinished => "actions.manager.waiting-employee-signoff",
            WorkflowState.EmployeeReviewConfirmed => "actions.manager.finalize-questionnaire",
            WorkflowState.Finalized => "actions.manager.questionnaire-finalized",
            _ => "actions.manager.no-action-required"
        };
    }
}
