namespace ti8m.BeachBreak.Client.Models;

public sealed class EmployeeFeedbackConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.EmployeeFeedback;
    public bool ShowFeedbackSection { get; set; } = true;

    public bool IsValid() => true;
}
