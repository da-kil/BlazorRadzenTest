using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.Events;

/// <summary>
/// Domain event raised when an employee submits their completed questionnaire response.
/// This represents the critical business moment when the employee commits their answers for review.
/// </summary>
public record QuestionnaireResponseSubmitted(
    DateTime SubmittedDate) : IDomainEvent;
