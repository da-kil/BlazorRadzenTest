using ti8m.BeachBreak.Client.Models.DTOs;

namespace ti8m.BeachBreak.Client.Models;

public class QuestionResponse
{
    public Guid QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }

    /// <summary>
    /// Strongly-typed response data that corresponds to the QuestionType.
    /// </summary>
    public QuestionResponseDataDto? ResponseData { get; set; }

    public DateTime LastModified { get; set; } = DateTime.Now;

    // Track edits made during review meeting
    public bool EditedDuringReview { get; set; }
    public Guid? EditedDuringReviewBy { get; set; }
    public DateTime? EditedDuringReviewAt { get; set; }
}