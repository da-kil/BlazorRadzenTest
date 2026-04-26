namespace ti8m.BeachBreak.Client.Models;

public sealed class MultipleChoiceConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.MultipleChoice;
    public List<ChoiceOption> Choices { get; set; } = new();
    public int MinSelections { get; set; } = 1;
    public int MaxSelections { get; set; } = 1;

    public bool IsRequired => MinSelections > 0;
}
