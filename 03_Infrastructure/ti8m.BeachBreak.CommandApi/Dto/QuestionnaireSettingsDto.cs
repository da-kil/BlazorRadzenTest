namespace ti8m.BeachBreak.CommandApi.Dto;

public class QuestionnaireSettingsDto
{
    public string SuccessMessage { get; set; } = "Questionnaire completed successfully!";
    public string IncompleteMessage { get; set; } = "Please complete all required sections.";
    public TimeSpan? TimeLimit { get; set; }
}