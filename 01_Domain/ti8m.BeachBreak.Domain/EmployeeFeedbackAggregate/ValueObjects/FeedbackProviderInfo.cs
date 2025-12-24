namespace ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.ValueObjects;

public record FeedbackProviderInfo
{
    public string ProviderName { get; init; } = string.Empty;
    public string ProviderRole { get; init; } = string.Empty;
    public string? ProjectName { get; init; }
    public string? ProjectContext { get; init; }

    public FeedbackProviderInfo() { }

    public FeedbackProviderInfo(string providerName, string providerRole, string? projectName = null, string? projectContext = null)
    {
        ProviderName = providerName;
        ProviderRole = providerRole;
        ProjectName = projectName;
        ProjectContext = projectContext;
    }

    public bool HasProjectContext => !string.IsNullOrWhiteSpace(ProjectName);
}