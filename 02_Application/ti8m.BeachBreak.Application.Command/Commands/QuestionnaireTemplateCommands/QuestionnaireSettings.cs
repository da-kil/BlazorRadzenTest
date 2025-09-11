namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class QuestionnaireSettings
{
    public bool AllowSaveProgress { get; set; } = true;
    public bool ShowProgressBar { get; set; } = true;
    public bool RequireAllSections { get; set; } = true;
    public string SuccessMessage { get; set; } = "Questionnaire completed successfully!";
    public string IncompleteMessage { get; set; } = "Please complete all required sections.";
    public TimeSpan? TimeLimit { get; set; }
    public bool AllowReviewBeforeSubmit { get; set; } = true;
}