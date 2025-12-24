using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Domain.FeedbackTemplateAggregate.Events;

public record FeedbackTemplateCloned(
    Guid NewTemplateId,
    Guid SourceTemplateId,
    Translation Name,
    Translation Description,
    List<EvaluationItem> Criteria,
    List<TextSectionDefinition> TextSections,
    int RatingScale,
    string ScaleLowLabel,
    string ScaleHighLabel,
    List<FeedbackSourceType> AllowedSourceTypes,
    Guid ClonedByEmployeeId,
    ApplicationRole ClonedByRole,
    DateTime CreatedDate) : IDomainEvent;
