namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

/// <summary>
/// Configuration for text questions with one or more text input sections.
/// Users provide free-form text responses in each section.
/// </summary>
public sealed class TextQuestionConfiguration : IQuestionConfiguration
{
    /// <summary>
    /// Gets the question type.
    /// </summary>
    public QuestionType QuestionType => QuestionType.TextQuestion;

    /// <summary>
    /// List of text sections within this question.
    /// Each section represents a distinct text area for user input.
    /// </summary>
    public List<TextSectionDefinition> TextSections { get; set; } = new();

    /// <summary>
    /// Validates that the configuration has all required data.
    /// </summary>
    public bool IsValid()
    {
        return TextSections.Any() &&
               TextSections.All(s => !string.IsNullOrWhiteSpace(s.TitleEnglish) || !string.IsNullOrWhiteSpace(s.TitleGerman));
    }

    /// <summary>
    /// Gets the list of required text sections that must be filled for the question to be complete.
    /// Used by domain validation logic. Respects the Order property for proper sequence validation.
    /// </summary>
    public List<RequiredTextSection> GetRequiredTextSections()
    {
        return TextSections
            .OrderBy(section => section.Order)
            .Select((section, index) => new RequiredTextSection(index, section.IsRequired))
            .Where(item => item.IsRequired)
            .ToList();
    }
}

/// <summary>
/// Represents a required text section for validation purposes.
/// Contains only the minimal information needed to validate responses.
/// </summary>
public record RequiredTextSection(int Index, bool IsRequired);
