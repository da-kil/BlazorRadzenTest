using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

public class QuestionnaireSettings : ValueObject
{
    public string SuccessMessage { get; private set; } = "Questionnaire completed successfully!";
    public string IncompleteMessage { get; private set; } = "Please complete all required sections.";
    public TimeSpan? TimeLimit { get; private set; }

    private QuestionnaireSettings() { }

    public QuestionnaireSettings(
        string successMessage = "Questionnaire completed successfully!",
        string incompleteMessage = "Please complete all required sections.",
        TimeSpan? timeLimit = null)
    {
        SuccessMessage = successMessage ?? "Questionnaire completed successfully!";
        IncompleteMessage = incompleteMessage ?? "Please complete all required sections.";
        TimeLimit = timeLimit;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return SuccessMessage;
        yield return IncompleteMessage;
        yield return TimeLimit ?? TimeSpan.Zero;
    }
}