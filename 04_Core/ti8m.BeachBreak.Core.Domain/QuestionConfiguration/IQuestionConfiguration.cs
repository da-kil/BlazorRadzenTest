namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

/// <summary>
/// Marker interface for all question configuration types.
/// Enables type-safe discrimination and polymorphic serialization.
/// </summary>
public interface IQuestionConfiguration
{
    /// <summary>
    /// Gets the question type that this configuration applies to.
    /// </summary>
    QuestionType QuestionType { get; }
}
