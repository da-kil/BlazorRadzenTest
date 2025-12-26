namespace ti8m.BeachBreak.QueryApi.Dto;

/// <summary>
/// DTO for InReview notes in questionnaire assignments
/// </summary>
public class InReviewNoteDto
{
    /// <summary>
    /// Unique identifier for the note
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Content of the note
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When the note was created or last modified
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Optional section ID for context (null = general note)
    /// </summary>
    public Guid? SectionId { get; set; }

    /// <summary>
    /// Section title for display purposes
    /// </summary>
    public string SectionTitle { get; set; } = string.Empty;

    /// <summary>
    /// ID of the employee who authored the note
    /// </summary>
    public Guid AuthorEmployeeId { get; set; }

    /// <summary>
    /// Display name of the author
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;
}