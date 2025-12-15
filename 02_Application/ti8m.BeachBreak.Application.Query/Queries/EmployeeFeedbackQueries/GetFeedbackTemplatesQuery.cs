using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Core;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeFeedbackQueries;

/// <summary>
/// Query to get available feedback templates and evaluation criteria.
/// Used by the frontend to populate criteria selection UI.
/// </summary>
public class GetFeedbackTemplatesQuery : IQuery<Result<FeedbackTemplatesResponse>>
{
    /// <summary>
    /// Filter by specific source type (optional).
    /// If provided, returns only templates/criteria relevant to that source type.
    /// </summary>
    public int? SourceType { get; set; }

    /// <summary>
    /// Whether to include all available criteria or just default ones.
    /// Default: true (include all).
    /// </summary>
    public bool IncludeAllCriteria { get; set; } = true;

    public GetFeedbackTemplatesQuery() { }

    public GetFeedbackTemplatesQuery(int? sourceType = null)
    {
        SourceType = sourceType;
    }
}

/// <summary>
/// Response containing available feedback templates and criteria.
/// </summary>
public class FeedbackTemplatesResponse
{
    /// <summary>
    /// Default templates for each source type.
    /// </summary>
    public Dictionary<int, EmployeeFeedbackConfiguration> DefaultTemplates { get; set; } = new();

    /// <summary>
    /// All available evaluation criteria.
    /// </summary>
    public List<EvaluationItem> AvailableCriteria { get; set; } = new();

    /// <summary>
    /// Standard text sections for unstructured feedback.
    /// </summary>
    public List<TextSectionDefinition> StandardTextSections { get; set; } = new();

    /// <summary>
    /// Source type options for UI display.
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