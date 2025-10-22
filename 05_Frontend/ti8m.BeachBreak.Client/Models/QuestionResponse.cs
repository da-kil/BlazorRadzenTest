namespace ti8m.BeachBreak.Client.Models;

public class QuestionResponse
{
    public Guid QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }
    public Dictionary<string, object>? ComplexValue { get; set; } // All answer data stored here
    public DateTime LastModified { get; set; } = DateTime.Now;

    // Track edits made during review meeting
    public bool EditedDuringReview { get; set; }
    public Guid? EditedDuringReviewBy { get; set; }
    public DateTime? EditedDuringReviewAt { get; set; }
}