namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

/// <summary>
/// Configuration for a multiple choice question.
/// Users select between MinSelections and MaxSelections options from the Choices list.
/// When MinSelections is 0, the question is optional (not required for completion).
/// </summary>
public sealed class MultipleChoiceConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.MultipleChoice;
    public List<ChoiceOption> Choices { get; set; } = new();
    public int MinSelections { get; set; } = 1;
    public int MaxSelections { get; set; } = 1;

    /// <summary>When true, the user must select at least MinSelections choices to complete the question.</summary>
    public bool IsRequired => MinSelections > 0;

    public bool IsValid()
    {
        if (!Choices.Any()) return false;
        if (MinSelections < 0) return false;
        if (MaxSelections < MinSelections) return false;
        if (MaxSelections > Choices.Count) return false;
        return Choices.All(c => !string.IsNullOrWhiteSpace(c.LabelEnglish) || !string.IsNullOrWhiteSpace(c.LabelGerman));
    }
}
