using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.Domain.FeedbackTemplateAggregate.Events;

public record FeedbackTemplateCriteriaChanged(List<EvaluationItem> Criteria) : IDomainEvent;
