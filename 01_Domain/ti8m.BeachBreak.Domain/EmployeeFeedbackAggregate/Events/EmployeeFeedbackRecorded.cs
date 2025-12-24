using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.ValueObjects;

namespace ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.Events;

public record EmployeeFeedbackRecorded(
    Guid FeedbackId,
    Guid EmployeeId,
    FeedbackSourceType SourceType,
    FeedbackProviderInfo ProviderInfo,
    DateTime FeedbackDate,
    ConfigurableFeedbackData FeedbackData,
    Guid RecordedByEmployeeId,
    DateTime RecordedDate) : IDomainEvent;