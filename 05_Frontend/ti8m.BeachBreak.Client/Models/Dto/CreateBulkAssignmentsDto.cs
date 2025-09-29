namespace ti8m.BeachBreak.Client.Models.Dto;

public class CreateBulkAssignmentsDto
{
    public Guid TemplateId { get; set; }
    public List<EmployeeAssignmentDto> EmployeeAssignments { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }
}