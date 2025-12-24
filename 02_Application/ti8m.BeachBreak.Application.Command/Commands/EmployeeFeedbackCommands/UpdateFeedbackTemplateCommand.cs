using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Domain;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Command to update an existing feedback template.
/// Updates name, description, criteria, text sections, rating scale, and source types.
/// </summary>
public record UpdateFeedbackTemplateCommand(
    Guid Id,
    Translation Name,
    Translation Description,
    List<EvaluationItem> Criteria,
    List<TextSectionDefinition> TextSections,
    int RatingScale,
    string ScaleLowLabel,
    string ScaleHighLabel,
    List<FeedbackSourceType> AllowedSourceTypes) : ICommand<Result>;
