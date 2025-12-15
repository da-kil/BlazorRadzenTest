using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.ValueObjects;

namespace ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.Events;

public record EmployeeFeedbackUpdated(
    Guid FeedbackId,
    FeedbackProviderInfo ProviderInfo,
    DateTime FeedbackDate,
    ConfigurableFeedbackData FeedbackData,
    Guid UpdatedByEmployeeId,
    DateTime UpdatedDate) : IDomainEvent;