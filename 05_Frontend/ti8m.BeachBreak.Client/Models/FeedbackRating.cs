namespace ti8m.BeachBreak.Client.Models;

public record FeedbackRating
{
    public int Rating { get; init; } // 0 = not rated, 1-10 = actual rating
    public string Comment { get; init; } = string.Empty;

    public FeedbackRating() { }

    public FeedbackRating(int rating, string comment = "")
    {
        Rating = rating;
        Comment = comment;
    }

    public bool IsRated => Rating > 0;
    public bool HasComment => !string.IsNullOrWhiteSpace(Comment);
}
