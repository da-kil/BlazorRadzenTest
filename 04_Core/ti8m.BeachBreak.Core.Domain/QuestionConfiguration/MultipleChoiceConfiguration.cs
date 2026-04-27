namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

public sealed class MultipleChoiceConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.MultipleChoice;
    public List<MultipleChoiceQuestion> Questions { get; set; } = new();

    public bool IsValid()
    {
        if (!Questions.Any()) return false;
        return Questions.All(q =>
            q.Choices.Any() &&
            q.MinSelections >= 0 &&
            q.MaxSelections >= q.MinSelections &&
            q.MaxSelections <= q.Choices.Count &&
            q.Choices.All(c => !string.IsNullOrWhiteSpace(c.LabelEnglish) || !string.IsNullOrWhiteSpace(c.LabelGerman)));
    }
}
