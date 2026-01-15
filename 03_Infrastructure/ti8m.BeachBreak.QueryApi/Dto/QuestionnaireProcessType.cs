namespace ti8m.BeachBreak.QueryApi.Dto;

/// <summary>
/// Defines the process type for questionnaires, controlling workflow behavior,
/// allowed question types, and review requirements.
/// Values must match Core.Domain.QuestionnaireProcessType exactly.
/// </summary>
public enum QuestionnaireProcessType
{
    /// <summary>
    /// Performance review process with manager participation.
    /// </summary>
    PerformanceReview = 0,

    /// <summary>
    /// Simple survey process without manager involvement.
    /// </summary>
    Survey = 1
}
