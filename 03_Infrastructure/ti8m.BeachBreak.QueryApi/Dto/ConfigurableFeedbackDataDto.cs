using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.QueryApi.Serialization;

namespace ti8m.BeachBreak.QueryApi.Dto;

/// <summary>
/// DTO for configurable feedback data with ratings and comments.
/// </summary>
[RegisterJsonSerialization(typeof(QueryApiJsonSerializerContext))]
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
}

/// <summary>
/// DTO for individual feedback rating with optional comment.
/// </summary>
[RegisterJsonSerialization(typeof(QueryApiJsonSerializerContext))]
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
}