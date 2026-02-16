namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO for extending questionnaire assignment due dates.
/// Maps to the backend ExtendAssignmentDueDateDto for assignment due date extension functionality.
/// </summary>
public class ExtendAssignmentDueDateDto
{
    public Guid AssignmentId { get; set; }
    public DateTime NewDueDate { get; set; }
    public string? ExtensionReason { get; set; }
}