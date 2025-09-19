namespace ti8m.BeachBreak.CommandApi.Dto;

public class BulkAssignmentReminderDto
{
    public IEnumerable<Guid> AssignmentIds { get; set; } = new List<Guid>();
    public string Message { get; set; } = string.Empty;
    public string SentBy { get; set; } = string.Empty;
}