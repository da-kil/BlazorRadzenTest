namespace ti8m.BeachBreak.Application.Query.Queries.AnalyticsQueries;

public class QuestionnaireUsageStats
{
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public TimeSpan? AverageCompletionTime { get; set; }
    public decimal CompletionRate { get; set; }
    public DateTime? LastUsedDate { get; set; }
    public decimal PopularityScore { get; set; }
}