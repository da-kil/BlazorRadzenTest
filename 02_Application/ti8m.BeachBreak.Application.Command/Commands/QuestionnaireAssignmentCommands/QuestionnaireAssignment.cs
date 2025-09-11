namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class QuestionnaireAssignment
{
    public Guid TemplateId { get; set; }
    public List<string> EmployeeIds { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
}
