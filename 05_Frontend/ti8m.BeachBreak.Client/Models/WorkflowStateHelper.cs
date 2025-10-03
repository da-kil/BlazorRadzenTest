namespace ti8m.BeachBreak.Client.Models;

public static class WorkflowStateHelper
{
    private static readonly List<string> StateOrder = new()
    {
        "Assigned",
        "EmployeeInProgress",
        "ManagerInProgress",
        "BothInProgress",
        "EmployeeConfirmed",
        "ManagerConfirmed",
        "BothConfirmed",
        "InReview",
        "EmployeeReviewConfirmed",
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
            "EmployeeConfirmed" => "Employee Confirmed",
            "ManagerConfirmed" => "Manager Confirmed",
            "BothConfirmed" => "Ready for Review",
            "InReview" => "In Review",
            "EmployeeReviewConfirmed" => "Review Confirmed",
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
            "EmployeeConfirmed" or "ManagerConfirmed" or "BothConfirmed" => "#935BA9",
            "InReview" => "#FF9800",
            "EmployeeReviewConfirmed" => "#00E6C8",
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
            "EmployeeConfirmed" or "ManagerConfirmed" or "BothConfirmed" => "check_circle",
            "InReview" => "rate_review",
            "EmployeeReviewConfirmed" => "verified",
            "Finalized" => "lock",
            _ => "help"
        };
    }

    public static bool CanEmployeeEdit(QuestionnaireAssignment assignment)
    {
        if (assignment.IsLocked) return false;

        var state = assignment.WorkflowState;
        return state is "Assigned" or "EmployeeInProgress" or "BothInProgress" or "InReview";
    }

    public static bool CanManagerEdit(QuestionnaireAssignment assignment)
    {
        if (assignment.IsLocked) return false;

        var state = assignment.WorkflowState;
        return state is "Assigned" or "ManagerInProgress" or "BothInProgress" or "InReview";
    }

    public static bool CanEmployeeConfirm(QuestionnaireAssignment assignment)
    {
        var state = assignment.WorkflowState;
        return state is "EmployeeInProgress" or "BothInProgress" &&
               assignment.EmployeeConfirmedDate == null;
    }

    public static bool CanManagerConfirm(QuestionnaireAssignment assignment)
    {
        var state = assignment.WorkflowState;
        return state is "ManagerInProgress" or "BothInProgress" &&
               assignment.ManagerConfirmedDate == null;
    }

    public static bool CanInitiateReview(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState == "BothConfirmed";
    }

    public static bool CanEmployeeConfirmReview(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState == "InReview" &&
               assignment.EmployeeReviewConfirmedDate == null;
    }

    public static bool CanManagerFinalize(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState == "EmployeeReviewConfirmed";
    }

    public static string GetNextActionForEmployee(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState switch
        {
            "Assigned" => "Start completing your sections",
            "EmployeeInProgress" => "Complete remaining sections",
            "BothInProgress" => "Complete and confirm your sections",
            "EmployeeConfirmed" => "Waiting for manager",
            "ManagerConfirmed" => "Complete your sections",
            "BothConfirmed" => "Waiting for manager to initiate review",
            "InReview" => "Review all sections and confirm",
            "EmployeeReviewConfirmed" => "Waiting for manager to finalize",
            "Finalized" => "Questionnaire is finalized",
            _ => "No action required"
        };
    }

    public static string GetNextActionForManager(QuestionnaireAssignment assignment)
    {
        return assignment.WorkflowState switch
        {
            "Assigned" => "Start completing your sections",
            "ManagerInProgress" => "Complete remaining sections",
            "BothInProgress" => "Complete and confirm your sections",
            "EmployeeConfirmed" => "Complete your sections",
            "ManagerConfirmed" => "Waiting for employee",
            "BothConfirmed" => "Initiate performance review",
            "InReview" => "Collaborate on final review",
            "EmployeeReviewConfirmed" => "Finalize the questionnaire",
            "Finalized" => "Questionnaire is finalized",
            _ => "No action required"
        };
    }
}
