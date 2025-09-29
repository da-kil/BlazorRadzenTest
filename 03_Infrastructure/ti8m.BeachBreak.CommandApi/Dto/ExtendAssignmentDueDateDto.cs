namespace ti8m.BeachBreak.CommandApi.Dto;

public class ExtendAssignmentDueDateDto
{
    public Guid AssignmentId { get; set; }
    public DateTime NewDueDate { get; set; }
    public string? ExtensionReason { get; set; }
}