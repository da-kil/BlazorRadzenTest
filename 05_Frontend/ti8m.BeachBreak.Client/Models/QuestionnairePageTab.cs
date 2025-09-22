namespace ti8m.BeachBreak.Client.Models;

public class QuestionnairePageTab
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public QuestionnaireTabType Type { get; set; }
    public bool IsVisible { get; set; } = true;
}