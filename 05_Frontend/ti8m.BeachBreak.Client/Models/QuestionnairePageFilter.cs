namespace ti8m.BeachBreak.Client.Models;

public class QuestionnairePageFilter
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public QuestionnaireFilterType Type { get; set; }
    public bool IsVisible { get; set; } = true;
    public object? DefaultValue { get; set; }
    public List<string> Options { get; set; } = new();
}