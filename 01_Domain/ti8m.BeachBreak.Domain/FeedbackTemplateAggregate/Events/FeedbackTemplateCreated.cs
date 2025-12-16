using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Domain.FeedbackTemplateAggregate.Events;

public record FeedbackTemplateCreated(
    Guid AggregateId,
    Translation Name,
    Translation Description,
    List<EvaluationItem> Criteria,
    List<TextSectionDefinition> TextSections,
    int RatingScale,
    string ScaleLowLabel,
    string ScaleHighLabel,
    List<FeedbackSourceType> AllowedSourceTypes,
    Guid CreatedByEmployeeId,
    ApplicationRole CreatedByRole,
    DateTime CreatedDate) : IDomainEvent;
