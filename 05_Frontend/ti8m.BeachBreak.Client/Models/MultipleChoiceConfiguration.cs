namespace ti8m.BeachBreak.Client.Models;

public sealed class MultipleChoiceConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.MultipleChoice;
    public List<MultipleChoiceQuestion> Questions { get; set; } = new();
}
