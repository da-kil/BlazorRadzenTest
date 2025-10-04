using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

public class QuestionnaireSettings : ValueObject
{
    public bool AllowSaveProgress { get; private set; } = true;
    public bool ShowProgressBar { get; private set; } = true;
    public bool RequireAllSections { get; private set; } = true;
    public string SuccessMessage { get; private set; } = "Questionnaire completed successfully!";
    public string IncompleteMessage { get; private set; } = "Please complete all required sections.";
    public TimeSpan? TimeLimit { get; private set; }
    public bool AllowReviewBeforeSubmit { get; private set; } = true;

    private QuestionnaireSettings() { }

    public QuestionnaireSettings(
        bool allowSaveProgress = true,
        bool showProgressBar = true,
        bool requireAllSections = true,
        string successMessage = "Questionnaire completed successfully!",
        string incompleteMessage = "Please complete all required sections.",
        TimeSpan? timeLimit = null,
        bool allowReviewBeforeSubmit = true)
    {
        AllowSaveProgress = allowSaveProgress;
        ShowProgressBar = showProgressBar;
        RequireAllSections = requireAllSections;
        SuccessMessage = successMessage ?? "Questionnaire completed successfully!";
        IncompleteMessage = incompleteMessage ?? "Please complete all required sections.";
        TimeLimit = timeLimit;
        AllowReviewBeforeSubmit = allowReviewBeforeSubmit;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AllowSaveProgress;
        yield return ShowProgressBar;
        yield return RequireAllSections;
        yield return SuccessMessage;
        yield return IncompleteMessage;
        yield return TimeLimit ?? TimeSpan.Zero;
        yield return AllowReviewBeforeSubmit;
    }
}