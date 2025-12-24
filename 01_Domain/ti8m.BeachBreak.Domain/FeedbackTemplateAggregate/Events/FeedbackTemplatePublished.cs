using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.FeedbackTemplateAggregate.Events;

public record FeedbackTemplatePublished(
    Guid PublishedByEmployeeId,
    DateTime PublishedDate) : IDomainEvent;
