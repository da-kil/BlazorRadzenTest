using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.Events;

/// <summary>
/// Domain event raised when a response is recorded/updated for a questionnaire section.
/// This captures the business-significant act of providing an answer to a section (which IS the question),
/// with role-based separation for employee vs manager responses.
/// Section IS the question - single response per section/role combination.
/// </summary>
public record SectionResponseRecorded(
    Guid SectionId,
    CompletionRole CompletionRole,
    QuestionResponseValue SectionResponse,
    DateTime RecordedDate) : IDomainEvent;
