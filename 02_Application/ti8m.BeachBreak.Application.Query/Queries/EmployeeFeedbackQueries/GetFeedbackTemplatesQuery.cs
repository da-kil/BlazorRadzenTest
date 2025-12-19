using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Core;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeFeedbackQueries;

/// <summary>
/// Query to get feedback source type metadata.
/// Used by the frontend to display available feedback source types and their validation requirements.
/// Templates are fully customizable - no predefined criteria are returned.
/// </summary>
public class GetFeedbackTemplatesQuery : IQuery<Result<FeedbackTemplatesResponse>>
{
    /// <summary>
    /// Filter by specific source type (optional).
    /// If provided, returns only metadata for that source type.
    /// </summary>
    public int? SourceType { get; set; }
}

/// <summary>
/// Response containing feedback source type metadata for template building.
/// Templates are fully customizable - users create their own rating+comment or text-only questions.
/// </summary>
public class FeedbackTemplatesResponse
{
    /// <summary>
    /// Fixed system-level source type options (Customer, Peer, Project Colleague).
    /// Includes validation requirements like RequiresProjectContext and RequiresProviderRole.
    /// </summary>
    public List<SourceTypeOption> SourceTypeOptions { get; set; } = new();
}

/// <summary>
/// Source type option for UI display.
/// </summary>
public class SourceTypeOption
{
    public int Value { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool RequiresProjectContext { get; set; }
    public bool RequiresProviderRole { get; set; }

    public SourceTypeOption() { }

    public SourceTypeOption(int value, string displayName, string description, bool requiresProjectContext, bool requiresProviderRole)
    {
        Value = value;
        DisplayName = displayName;
        Description = description;
        RequiresProjectContext = requiresProjectContext;
        RequiresProviderRole = requiresProviderRole;
    }
}