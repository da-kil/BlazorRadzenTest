namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Marker interface for all question configuration types.
/// Enables type-safe discrimination and polymorphic deserialization from API.
/// </summary>
public interface IQuestionConfiguration
{
    /// <summary>
    /// Gets the question type that this configuration applies to.
    /// </summary>
    QuestionType QuestionType { get; }
}
