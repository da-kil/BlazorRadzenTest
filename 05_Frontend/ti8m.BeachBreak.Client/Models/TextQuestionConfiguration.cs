namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Configuration for text questions with single or multiple text input sections.
/// Each section represents a distinct text area where users can provide responses.
/// </summary>
public sealed class TextQuestionConfiguration : IQuestionConfiguration
{
    /// <summary>
    /// Gets the question type.
    /// </summary>
    public QuestionType QuestionType => QuestionType.TextQuestion;

    /// <summary>
    /// List of text sections to be displayed for user input.
    /// </summary>
    public List<TextSection> TextSections { get; set; } = new();
}