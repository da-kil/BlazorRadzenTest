using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when the workflow state changes via forward transition.
/// This is a first-class event that provides complete audit trail of normal state transitions.
/// Use WorkflowReopened event for backward transitions.
/// </summary>
public record WorkflowStateTransitioned(
    WorkflowState FromState,
    WorkflowState ToState,
    string TransitionReason,
    DateTime TransitionedAt,
    Guid? TransitionedByEmployeeId = null) : IDomainEvent;
