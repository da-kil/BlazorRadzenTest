namespace ti8m.BeachBreak.Client.Models.DTOs;

/// <summary>
/// Strongly-typed DTO for individual question responses.
/// Matches the new API structure with ResponseData property.
/// </summary>
public class QuestionResponseDto
{
    public Guid QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Strongly-typed response data that corresponds to the QuestionType.
    /// </summary>
    public QuestionResponseDataDto? ResponseData { get; set; }
}