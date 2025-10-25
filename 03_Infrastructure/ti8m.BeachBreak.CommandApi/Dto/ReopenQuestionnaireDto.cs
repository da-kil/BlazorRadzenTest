using System.ComponentModel.DataAnnotations;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for reopening a questionnaire assignment.
/// Reopen reason is required and must be at least 10 characters.
/// </summary>
public class ReopenQuestionnaireDto
{
    /// <summary>
    /// Target workflow state to reopen to.
    /// Valid options depend on current state (see WorkflowStateMachine).
    /// </summary>
    [Required]
    public WorkflowState TargetState { get; set; }

    /// <summary>
    /// Reason for reopening the questionnaire.
    /// This will be included in email notifications and audit logs.
    /// Minimum length: 10 characters.
    /// </summary>
    [Required(ErrorMessage = "Reopen reason is required")]
    [MinLength(10, ErrorMessage = "Reopen reason must be at least 10 characters")]
    public string ReopenReason { get; set; } = string.Empty;
}
