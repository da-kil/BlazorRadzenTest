namespace ti8m.BeachBreak.Core.Domain;

/// <summary>
/// Defines the process type for questionnaires, controlling workflow behavior,
/// allowed question types, and review requirements.
/// </summary>
public enum QuestionnaireProcessType
{
    /// <summary>
    /// Performance review process with manager participation.
    /// - Requires manager review
    /// - Allows Goal and EmployeeFeedback question types
    /// - Supports Employee, Manager, and Both completion roles
    /// - Workflow: Employee submits → Manager reviews → Meeting → Finalized
    /// </summary>
    PerformanceReview = 0,

    /// <summary>
    /// Simple survey process without manager involvement.
    /// - No manager review required
    /// - Only Assessment and TextQuestion types allowed
    /// - Only Employee completion role allowed
    /// - Workflow: Employee submits → Auto-finalized
    /// </summary>
    Survey = 1
}
