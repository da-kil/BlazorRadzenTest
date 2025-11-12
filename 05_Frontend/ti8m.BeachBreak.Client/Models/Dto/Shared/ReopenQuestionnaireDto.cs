namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO for reopening a questionnaire assignment.
/// Used by TeamLead, HR, and Admin to reopen assignments for corrections.
/// </summary>
public class ReopenQuestionnaireDto
{
    /// <summary>
    /// Target workflow state to reopen to (e.g., EmployeeInProgress, ManagerInProgress, BothInProgress, InReview).
    /// </summary>
    public WorkflowState TargetState { get; set; }

    /// <summary>
    /// Required reason for reopening (minimum 10 characters).
    /// </summary>
    public string ReopenReason { get; set; } = string.Empty;
}
