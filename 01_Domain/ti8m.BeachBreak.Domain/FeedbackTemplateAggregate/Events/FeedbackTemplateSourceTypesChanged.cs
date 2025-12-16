using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Domain.FeedbackTemplateAggregate.Events;

public record FeedbackTemplateSourceTypesChanged(List<FeedbackSourceType> AllowedSourceTypes) : IDomainEvent;
