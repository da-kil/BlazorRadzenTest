using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

public record QuestionnaireAssignmentAssigned(
    Guid AggregateId,
    Guid TemplateId,
    QuestionnaireProcessType ProcessType,
    Guid EmployeeId,
    DateTime AssignedDate,
    DateTime? DueDate,
    Guid AssignedByUserId,
    string? Notes) : IDomainEvent;
