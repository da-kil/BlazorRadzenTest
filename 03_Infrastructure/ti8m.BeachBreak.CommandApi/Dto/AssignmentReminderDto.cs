namespace ti8m.BeachBreak.CommandApi.Dto;

public class AssignmentReminderDto
{
    public Guid AssignmentId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string SentBy { get; set; } = string.Empty;
}