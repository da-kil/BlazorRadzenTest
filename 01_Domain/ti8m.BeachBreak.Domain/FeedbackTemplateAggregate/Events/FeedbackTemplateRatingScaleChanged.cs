using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.FeedbackTemplateAggregate.Events;

public record FeedbackTemplateRatingScaleChanged(
    int RatingScale,
    string ScaleLowLabel,
    string ScaleHighLabel) : IDomainEvent;
