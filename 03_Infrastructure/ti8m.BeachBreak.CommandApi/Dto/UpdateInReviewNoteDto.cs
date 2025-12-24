namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for updating a note during the InReview phase of a questionnaire assignment
/// </summary>
public class UpdateInReviewNoteDto
{
    /// <summary>
    /// Updated note content (maximum 2000 characters)
    /// </summary>
    public string Content { get; set; } = string.Empty;
}