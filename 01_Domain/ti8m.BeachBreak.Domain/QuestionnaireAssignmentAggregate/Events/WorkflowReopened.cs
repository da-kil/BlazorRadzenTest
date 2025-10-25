using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a workflow is reopened by an administrator for corrections.
/// This is a special backward transition that requires elevated permissions.
///
/// Authorization:
/// - Admin: Can reopen ANY non-finalized state for ALL questionnaires
/// - HR: Can reopen ANY non-finalized state for ALL questionnaires
/// - TeamLead: Can reopen ALL non-finalized states for THEIR TEAM ONLY
///
/// Note: Finalized state CANNOT be reopened. Create new assignment instead.
/// Note: Email notifications are sent to affected parties when questionnaire is reopened.
/// </summary>
public record WorkflowReopened(
    WorkflowState FromState,
    WorkflowState ToState,
    string ReopenReason,
    DateTime ReopenedAt,
    Guid ReopenedByEmployeeId,
    string ReopenedByRole) : IDomainEvent;
