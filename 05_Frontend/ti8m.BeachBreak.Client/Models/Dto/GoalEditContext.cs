namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// Context information needed for goal edit permission validation.
/// Encapsulates all the business context required to determine if a goal modification is allowed.
/// </summary>
public record GoalEditContext
{
    /// <summary>
    /// The role of the currently logged-in user making the request.
    /// Used for permission validation - this is who has the authority.
    /// </summary>
    public string LoggedInUserRole { get; init; } = string.Empty;

    /// <summary>
    /// The role that originally owned/created the answer being modified.
    /// Used for UI context and business rule evaluation.
    /// </summary>
    public string AnswerOwnerRole { get; init; } = string.Empty;

    /// <summary>
    /// Current workflow state of the assignment.
    /// Critical for determining which permissions apply (e.g., review meetings have different rules).
    /// </summary>
    public WorkflowState AssignmentWorkflowState { get; init; }

    /// <summary>
    /// The assignment ID for context and audit trail.
    /// </summary>
    public Guid AssignmentId { get; init; }
}