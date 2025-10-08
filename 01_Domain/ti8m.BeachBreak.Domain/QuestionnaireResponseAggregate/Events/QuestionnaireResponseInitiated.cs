using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.Events;

/// <summary>
/// Domain event raised when an employee initiates a response to a questionnaire assignment.
/// This represents the business-significant moment when the employee begins their questionnaire journey.
/// </summary>
public record QuestionnaireResponseInitiated(
    Guid AggregateId,
    Guid AssignmentId,
    Guid EmployeeId,
    DateTime InitiatedDate) : IDomainEvent;
