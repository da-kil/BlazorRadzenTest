using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

public record QuestionnaireAssignmentAssigned(
    Guid AggregateId,
    Guid TemplateId,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeEmail,
    DateTime AssignedDate,
    DateTime? DueDate,
    string? AssignedBy,
    string? Notes) : IDomainEvent;