using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.ValueObjects;
using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.Dto;

/// <summary>
/// DTO for configurable feedback data with ratings and comments.
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class ConfigurableFeedbackDataDto
{
    /// <summary>
    /// Dictionary of ratings keyed by evaluation criteria.
    /// Key is the criteria identifier, value is the rating data.
    /// </summary>
    public Dictionary<string, FeedbackRatingDto> Ratings { get; set; } = new();

    /// <summary>
    /// Dictionary of comments keyed by section identifier.
    /// Key is the section identifier, value is the comment text.
    /// </summary>
    public Dictionary<string, string> Comments { get; set; } = new();

    /// <summary>
    /// Converts DTO to domain value object.
    /// </summary>
    public ConfigurableFeedbackData ToValueObject()
    {
        var domainRatings = Ratings.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToValueObject());

        return new ConfigurableFeedbackData(domainRatings, Comments);
    }

    /// <summary>
    /// Creates DTO from domain value object.
    /// </summary>
    public static ConfigurableFeedbackDataDto FromValueObject(ConfigurableFeedbackData feedbackData)
    {
        var dtoRatings = feedbackData.Ratings.ToDictionary(
            kvp => kvp.Key,
            kvp => FeedbackRatingDto.FromValueObject(kvp.Value));

        return new ConfigurableFeedbackDataDto
        {
            Ratings = dtoRatings,
            Comments = new Dictionary<string, string>(feedbackData.Comments)
        };
    }
}

/// <summary>
/// DTO for individual feedback rating with optional comment.
/// </summary>
[RegisterJsonSerialization(typeof(CommandApiJsonSerializerContext))]
public class FeedbackRatingDto
{
    /// <summary>
    /// Rating value (0 = not rated, 1-10 = actual rating).
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Optional comment for this specific rating.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Converts DTO to domain value object.
    /// </summary>
    public FeedbackRating ToValueObject()
    {
        return new FeedbackRating(Rating, Comment);
    }

    /// <summary>
    /// Creates DTO from domain value object.
    /// </summary>
    public static FeedbackRatingDto FromValueObject(FeedbackRating rating)
    {
        return new FeedbackRatingDto
        {
            Rating = rating.Rating,
            Comment = rating.Comment
        };
    }
}