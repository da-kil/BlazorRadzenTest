using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.PhaseRecords;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a manager finalizes the questionnaire after employee confirmation.
/// Transitions from EmployeeReviewConfirmed to Finalized state.
/// Questionnaire becomes permanently locked and archived.
/// </summary>
public record ManagerFinalizedQuestionnaire(
    Guid AggregateId,
    FinalizationRecord Finalization) : IDomainEvent;
