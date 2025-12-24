using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.Events;

public record EmployeeFeedbackDeleted(
    Guid FeedbackId,
    Guid DeletedByEmployeeId,
    DateTime DeletedDate,
    string? DeleteReason = null) : IDomainEvent;