namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// DTO representing a change made during a review meeting.
/// Shows what was edited, who edited it, and when.
/// </summary>
public class ReviewChangeDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid SectionId { get; set; }
    public string SectionTitle { get; set; } = string.Empty;
    public Guid QuestionId { get; set; }
    public string QuestionTitle { get; set; } = string.Empty;
    public string OriginalCompletionRole { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string NewValue { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}
