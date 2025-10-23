using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.QueryApi.Dto;

/// <summary>
/// DTO for team assignments with enriched template metadata.
/// Used by managers to view their team's questionnaire assignments.
/// </summary>
public class TeamAssignmentDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public Guid? TemplateCategoryId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }

    // Workflow properties
    public WorkflowState WorkflowState { get; set; } = WorkflowState.Assigned;
    public List<SectionProgressDto> SectionProgress { get; set; } = new();

    // Submission phase
    public DateTime? EmployeeSubmittedDate { get; set; }
    public string? EmployeeSubmittedBy { get; set; }
    public DateTime? ManagerSubmittedDate { get; set; }
    public string? ManagerSubmittedBy { get; set; }

    // Review phase
    public DateTime? ReviewInitiatedDate { get; set; }
    public string? ReviewInitiatedBy { get; set; }
    public DateTime? ManagerReviewFinishedDate { get; set; }
    public string? ManagerReviewFinishedBy { get; set; }
    public string? ManagerReviewSummary { get; set; }
    public DateTime? EmployeeReviewConfirmedDate { get; set; }
    public string? EmployeeReviewConfirmedBy { get; set; }
    public string? EmployeeReviewComments { get; set; }

    // Final state
    public DateTime? FinalizedDate { get; set; }
    public string? FinalizedBy { get; set; }
    public string? ManagerFinalNotes { get; set; }
    public bool IsLocked { get; set; }
}
