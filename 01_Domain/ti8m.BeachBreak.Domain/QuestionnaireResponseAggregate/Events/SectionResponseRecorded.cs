using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.Events;

/// <summary>
/// Domain event raised when an employee records/updates their responses for a questionnaire section.
/// This captures the business-significant act of the employee providing answers to questions.
/// </summary>
public record SectionResponseRecorded(
    Guid SectionId,
    Dictionary<Guid, object> QuestionResponses,
    DateTime RecordedDate) : IDomainEvent;
