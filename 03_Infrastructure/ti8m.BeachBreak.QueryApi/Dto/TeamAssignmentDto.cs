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
    public AssignmentStatus Status { get; set; }
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }
}
