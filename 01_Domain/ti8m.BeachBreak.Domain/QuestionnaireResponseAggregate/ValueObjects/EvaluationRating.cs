namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

/// <summary>
/// Represents an evaluation rating with score and optional comment.
/// Ensures type safety for assessment question responses.
/// </summary>
public record EvaluationRating
{
    public int Rating { get; init; }
    public string Comment { get; init; }

    public EvaluationRating(int rating, string comment = "")
    {
        if (rating < 0 || rating > 4)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 0 and 4");

        Rating = rating;
        Comment = comment ?? string.Empty;
    }

    /// <summary>
    /// Creates a rating without comment.
    /// </summary>
    public static EvaluationRating WithRating(int rating) => new(rating);

    /// <summary>
    /// Creates a rating with comment.
    /// </summary>
    public static EvaluationRating WithComment(int rating, string comment) => new(rating, comment);

    /// <summary>
    /// Indicates whether this rating has a valid score (greater than 0).
    /// </summary>
    public bool IsValidRating => Rating > 0;
}
