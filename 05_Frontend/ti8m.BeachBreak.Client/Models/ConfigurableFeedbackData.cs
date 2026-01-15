namespace ti8m.BeachBreak.Client.Models;

public record ConfigurableFeedbackData
{
    public Dictionary<string, FeedbackRating> Ratings { get; init; } = new();
    public Dictionary<string, string> Comments { get; init; } = new();

    public ConfigurableFeedbackData() { }

    public ConfigurableFeedbackData(Dictionary<string, FeedbackRating> ratings, Dictionary<string, string> comments)
    {
        Ratings = ratings ?? new Dictionary<string, FeedbackRating>();
        Comments = comments ?? new Dictionary<string, string>();
    }

    public bool HasAnyRating => Ratings.Values.Any(r => r.Rating > 0);
    public bool HasAnyComment => Comments.Values.Any(c => !string.IsNullOrWhiteSpace(c));
    public bool HasAnyContent => HasAnyRating || HasAnyComment;

    public int RatedItemsCount => Ratings.Values.Count(r => r.Rating > 0);
    public decimal? AverageRating => RatedItemsCount > 0
        ? (decimal)Ratings.Values.Where(r => r.Rating > 0).Average(r => r.Rating)
        : null;
}
