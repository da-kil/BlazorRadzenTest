namespace ti8m.BeachBreak.Client.Models;

public class QuestionnairePageAction
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ButtonStyle { get; set; } = "ButtonStyle.Light";
    public bool IsVisible { get; set; } = true;
    public Func<Task>? OnClick { get; set; }
}