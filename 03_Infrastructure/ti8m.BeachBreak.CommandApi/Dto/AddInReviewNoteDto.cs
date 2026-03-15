namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for adding a note during the InReview phase of a questionnaire assignment
/// </summary>
public class AddInReviewNoteDto
{
    /// <summary>
    /// Note content (maximum 2000 characters)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Optional section ID for context (null = general note)
    /// </summary>
    public Guid? SectionId { get; set; }

    /// <summary>
    /// Optional item key to identify a specific evaluation item within a section (e.g. assessment competency key)
    /// </summary>
    public string? ItemKey { get; set; }
}