namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;

public class QuestionnaireSettings
{
    public string SuccessMessage { get; set; } = "Questionnaire completed successfully!";
    public string IncompleteMessage { get; set; } = "Please complete all required sections.";
    public TimeSpan? TimeLimit { get; set; }
}