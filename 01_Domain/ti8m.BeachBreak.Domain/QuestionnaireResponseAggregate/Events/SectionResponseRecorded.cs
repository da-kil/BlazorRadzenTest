using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.Events;

/// <summary>
/// Domain event raised when a response is recorded/updated for a questionnaire section.
/// This captures the business-significant act of providing answers to questions,
/// with role-based separation for employee vs manager responses.
/// </summary>
public record SectionResponseRecorded(
    Guid SectionId,
    CompletionRole Role,
    Dictionary<Guid, object> QuestionResponses,
    DateTime RecordedDate) : IDomainEvent;
