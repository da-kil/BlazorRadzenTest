namespace ti8m.BeachBreak.Client.Models.Dto;

public class AvailablePredecessorDto
{
    public Guid AssignmentId { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}
