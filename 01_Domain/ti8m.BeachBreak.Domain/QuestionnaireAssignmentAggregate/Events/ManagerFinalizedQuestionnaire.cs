using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

/// <summary>
/// Domain event raised when a manager finalizes the questionnaire after employee confirmation.
/// This is the final step in the review process.
/// Transitions from EmployeeReviewConfirmed to Finalized state.
/// Questionnaire becomes permanently locked and archived.
/// </summary>
public record ManagerFinalizedQuestionnaire(
    Guid AggregateId,
    DateTime FinalizedDate,
    string FinalizedBy,
    string? ManagerFinalNotes
) : IDomainEvent;
