namespace ti8m.BeachBreak.Client.Models;

public class QuestionnairePageConfiguration
{
    public string PageRoute { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public string PageDescription { get; set; } = string.Empty;
    public string PageIcon { get; set; } = string.Empty;

    public QuestionnairePageType PageType { get; set; }
    public List<QuestionnairePageTab> Tabs { get; set; } = new();
    public List<QuestionnairePageFilter> Filters { get; set; } = new();
    public List<QuestionnairePageAction> Actions { get; set; } = new();
    public QuestionnaireStatsConfig StatsConfig { get; set; } = new();
}