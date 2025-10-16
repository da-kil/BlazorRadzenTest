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
        WorkflowState.EmployeeReviewConfirmed,
        WorkflowState.ManagerReviewConfirmed,
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
            WorkflowState.Assigned => "Assigned",
            WorkflowState.EmployeeInProgress => "Employee Working",
            WorkflowState.ManagerInProgress => "Manager Working",
            WorkflowState.BothInProgress => "Both Working",
            WorkflowState.EmployeeSubmitted => "Employee Submitted",
            WorkflowState.ManagerSubmitted => "Manager Submitted",
            WorkflowState.BothSubmitted => "Both Submitted - Ready for Review",
            WorkflowState.InReview => "In Review",
            WorkflowState.EmployeeReviewConfirmed => "Employee Review Confirmed",
            WorkflowState.ManagerReviewConfirmed => "Manager Review Confirmed",
            WorkflowState.Finalized => "Finalized",
            _ => state.ToString()
        };
    }

    public static string GetStateColor(WorkflowState state)
    {
        return state switch
        {
            WorkflowState.Assigned => "#6c757d",
            WorkflowState.EmployeeInProgress or WorkflowState.ManagerInProgress or WorkflowState.BothInProgress => "#0F60FF",
            WorkflowState.EmployeeSubmitted or WorkflowState.ManagerSubmitted or WorkflowState.BothSubmitted => "#935BA9",
            WorkflowState.InReview => "#FF9800",
            WorkflowState.EmployeeReviewConfirmed or WorkflowState.ManagerReviewConfirmed => "#00E6C8",
            WorkflowState.Finalized => "#4CAF50",
            _ => "#6c757d"
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
            WorkflowState.EmployeeReviewConfirmed or WorkflowState.ManagerReviewConfirmed => "verified",
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
        // Employee is READ-ONLY during InReview (manager-led review meeting)
        return state is WorkflowState.Assigned or WorkflowState.EmployeeInProgress or WorkflowState.BothInProgress or WorkflowState.ManagerSubmitted;

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
        // Manager finishes the review meeting, transitioning from InReview to ManagerReviewConfirmed
        return assignment.WorkflowState == WorkflowState.InReview;
    }

    public static bool CanEmployeeConfirmReview(QuestionnaireAssignment assignment)
    {
        // Employee confirms review outcome after manager finishes the review meeting
        return assignment.WorkflowState == WorkflowState.ManagerReviewConfirmed;
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
            WorkflowState.Assigned => "Start completing your sections",
            WorkflowState.EmployeeInProgress => "Complete and submit your sections",
            WorkflowState.BothInProgress => "Complete and submit your sections",
            WorkflowState.EmployeeSubmitted => "Waiting for manager to submit",
            WorkflowState.ManagerSubmitted => "Complete and submit your sections",
            WorkflowState.BothSubmitted => "Waiting for manager to initiate review meeting",
            WorkflowState.InReview => "Review meeting in progress (read-only)",
            WorkflowState.ManagerReviewConfirmed => "Confirm the review outcome",
            WorkflowState.EmployeeReviewConfirmed => "Waiting for manager to finalize",
            WorkflowState.Finalized => "Questionnaire is finalized",
            _ => "No action required"
        };
    }

    public static string GetNextActionForManager(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState switch
        {
            WorkflowState.Assigned => "Start completing your sections",
            WorkflowState.ManagerInProgress => "Complete and submit your sections",
            WorkflowState.BothInProgress => "Complete and submit your sections",
            WorkflowState.EmployeeSubmitted => "Complete and submit your sections",
            WorkflowState.ManagerSubmitted => "Waiting for employee to submit",
            WorkflowState.BothSubmitted => "Initiate performance review meeting",
            WorkflowState.InReview => "Conduct review meeting and finish when done",
            WorkflowState.ManagerReviewConfirmed => "Waiting for employee to confirm review",
            WorkflowState.EmployeeReviewConfirmed => "Finalize the questionnaire",
            WorkflowState.Finalized => "Questionnaire is finalized",
            _ => "No action required"
        };
    }
}
