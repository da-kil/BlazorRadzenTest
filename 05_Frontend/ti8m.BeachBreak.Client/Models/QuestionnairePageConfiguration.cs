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

public enum QuestionnairePageType
{
    Employee,  // My questionnaires view
    Manager,   // Team questionnaires view
    HR         // Organization questionnaires view
}

public class QuestionnairePageTab
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public QuestionnaireTabType Type { get; set; }
    public bool IsVisible { get; set; } = true;
}

public enum QuestionnaireTabType
{
    Current,
    Upcoming,
    Completed,
    Overdue,
    TeamView,
    QuestionnaireView,
    Analytics,
    DepartmentOverview,
    EmployeeStatus,
    QuestionnairePerformance
}

public class QuestionnairePageFilter
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public QuestionnaireFilterType Type { get; set; }
    public bool IsVisible { get; set; } = true;
    public object? DefaultValue { get; set; }
    public List<string> Options { get; set; } = new();
}

public enum QuestionnaireFilterType
{
    Search,
    Category,
    Status,
    Department,
    DateRange
}

public class QuestionnairePageAction
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ButtonStyle { get; set; } = "ButtonStyle.Light";
    public bool IsVisible { get; set; } = true;
    public Func<Task>? OnClick { get; set; }
}

public class QuestionnaireStatsConfig
{
    public List<StatCardConfiguration> StatCards { get; set; } = new();
    public int Columns { get; set; } = 4;
}

public class StatCardConfiguration
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public Func<object> ValueCalculator { get; set; } = () => "0";
}