namespace ti8m.BeachBreak.CommandApi.Dto;

public class CreateAssignmentDto
{
    public Guid TemplateId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }
}