namespace ti8m.BeachBreak.Client.Models;

public static class WorkflowStateHelper
{
    private static readonly List<string> StateOrder = new()
    {
        "Assigned",
        "EmployeeInProgress",
        "ManagerInProgress",
        "BothInProgress",
        "EmployeeSubmitted",
        "ManagerSubmitted",
        "BothSubmitted",
        "InReview",
        "EmployeeReviewConfirmed",
        "ManagerReviewConfirmed",
        "Finalized"
    };

    public static bool IsStateAfter(string currentState, string referenceState)
    {
        var currentIndex = StateOrder.IndexOf(currentState);
        var referenceIndex = StateOrder.IndexOf(referenceState);
        return currentIndex > referenceIndex;
    }

    public static bool IsStateBefore(string currentState, string referenceState)
    {
        var currentIndex = StateOrder.IndexOf(currentState);
        var referenceIndex = StateOrder.IndexOf(referenceState);
        return currentIndex < referenceIndex;
    }

    public static string GetStateDisplayName(string state)
    {
        return state switch
        {
            "Assigned" => "Assigned",
            "EmployeeInProgress" => "Employee Working",
            "ManagerInProgress" => "Manager Working",
            "BothInProgress" => "Both Working",
            "EmployeeSubmitted" => "Employee Submitted",
            "ManagerSubmitted" => "Manager Submitted",
            "BothSubmitted" => "Both Submitted - Ready for Review",
            "InReview" => "In Review",
            "EmployeeReviewConfirmed" => "Employee Review Confirmed",
            "ManagerReviewConfirmed" => "Manager Review Confirmed",
            "Finalized" => "Finalized",
            _ => state
        };
    }

    public static string GetStateColor(string state)
    {
        return state switch
        {
            "Assigned" => "#6c757d",
            "EmployeeInProgress" or "ManagerInProgress" or "BothInProgress" => "#0F60FF",
            "EmployeeSubmitted" or "ManagerSubmitted" or "BothSubmitted" => "#935BA9",
            "InReview" => "#FF9800",
            "EmployeeReviewConfirmed" or "ManagerReviewConfirmed" => "#00E6C8",
            "Finalized" => "#4CAF50",
            _ => "#6c757d"
        };
    }

    public static string GetStateIcon(string state)
    {
        return state switch
        {
            "Assigned" => "assignment",
            "EmployeeInProgress" or "ManagerInProgress" or "BothInProgress" => "edit",
            "EmployeeSubmitted" or "ManagerSubmitted" or "BothSubmitted" => "send",
            "InReview" => "rate_review",
            "EmployeeReviewConfirmed" or "ManagerReviewConfirmed" => "verified",
            "Finalized" => "lock",
            _ => "help"
        };
    }

    public static bool CanEmployeeEdit(QuestionnaireAssignment assignment)
    {
        if (assignment.IsLocked) return false;

        var state = assignment.WorkflowState;

        // Can edit before submission or during review
        return state is "Assigned" or "EmployeeInProgress" or "BothInProgress" or "InReview";

        // Cannot edit after submission (Phase 1 read-only): EmployeeSubmitted, ManagerSubmitted, BothSubmitted
        // Cannot edit after finalization (Phase 2 read-only): Finalized
    }

    public static bool CanManagerEdit(QuestionnaireAssignment assignment)
    {
        if (assignment.IsLocked) return false;

        var state = assignment.WorkflowState;

        // Can edit before submission or during review
        return state is "Assigned" or "ManagerInProgress" or "BothInProgress" or "InReview";

        // Cannot edit after submission (Phase 1 read-only): EmployeeSubmitted, ManagerSubmitted, BothSubmitted
        // Cannot edit after finalization (Phase 2 read-only): Finalized
    }

    public static bool CanEmployeeSubmit(QuestionnaireAssignment assignment)
    {
        var state = assignment.WorkflowState;
        return state is "EmployeeInProgress" or "BothInProgress";
    }

    public static bool CanManagerSubmit(QuestionnaireAssignment assignment)
    {
        var state = assignment.WorkflowState;
        return state is "ManagerInProgress" or "BothInProgress" or "EmployeeSubmitted";
    }

    public static bool CanInitiateReview(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState == "BothSubmitted";
    }

    public static bool CanEmployeeConfirmReview(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState == "InReview" &&
               assignment.EmployeeReviewConfirmedDate == null;
    }

    public static bool CanManagerConfirmReview(QuestionnaireAssignment assignment)
    {
        return (assignment.WorkflowState == "InReview" ||
                assignment.WorkflowState == "EmployeeReviewConfirmed") &&
               assignment.ManagerReviewConfirmedDate == null;
    }

    public static bool CanManagerFinalize(QuestionnaireAssignment assignment)
    {
        return assignment.EmployeeReviewConfirmedDate.HasValue &&
               assignment.ManagerReviewConfirmedDate.HasValue;
    }

    public static string GetNextActionForEmployee(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState switch
        {
            "Assigned" => "Start completing your sections",
            "EmployeeInProgress" => "Complete and submit your sections",
            "BothInProgress" => "Complete and submit your sections",
            "EmployeeSubmitted" => "Waiting for manager to submit",
            "ManagerSubmitted" => "Complete and submit your sections",
            "BothSubmitted" => "Waiting for manager to initiate review",
            "InReview" => "Review all sections and confirm",
            "EmployeeReviewConfirmed" => "Waiting for manager to confirm and finalize",
            "ManagerReviewConfirmed" => "Waiting for manager to finalize",
            "Finalized" => "Questionnaire is finalized",
            _ => "No action required"
        };
    }

    public static string GetNextActionForManager(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState switch
        {
            "Assigned" => "Start completing your sections",
            "ManagerInProgress" => "Complete and submit your sections",
            "BothInProgress" => "Complete and submit your sections",
            "EmployeeSubmitted" => "Complete and submit your sections",
            "ManagerSubmitted" => "Waiting for employee to submit",
            "BothSubmitted" => "Initiate performance review",
            "InReview" => "Collaborate on final review and confirm",
            "EmployeeReviewConfirmed" => "Confirm review and finalize",
            "ManagerReviewConfirmed" => "Finalize the questionnaire",
            "Finalized" => "Questionnaire is finalized",
            _ => "No action required"
        };
    }
}
