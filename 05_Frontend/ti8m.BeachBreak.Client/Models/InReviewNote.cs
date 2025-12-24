namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Frontend model for InReview notes during questionnaire review meetings
/// </summary>
public class InReviewNote
{
    /// <summary>
    /// Unique identifier for the note
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Content of the note (maximum 2000 characters)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When the note was created or last modified
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional section ID for context (null = general note)
    /// </summary>
    public Guid? SectionId { get; set; }

    /// <summary>
    /// Section title for display purposes
    /// </summary>
    public string SectionTitle { get; set; } = "General";

    /// <summary>
    /// ID of the employee who authored the note
    /// </summary>
    public Guid AuthorEmployeeId { get; set; }

    /// <summary>
    /// Display name of the author (denormalized for performance)
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;
}