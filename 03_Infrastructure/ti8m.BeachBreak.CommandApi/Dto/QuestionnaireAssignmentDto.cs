namespace ti8m.BeachBreak.CommandApi.Dto;

public class QuestionnaireAssignmentDto
{
    public Guid TemplateId { get; set; }
    public List<string> EmployeeIds { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
}
