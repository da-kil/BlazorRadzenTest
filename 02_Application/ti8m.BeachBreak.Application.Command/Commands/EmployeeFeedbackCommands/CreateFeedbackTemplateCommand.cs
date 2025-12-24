using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Domain;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Command to create a new feedback template.
/// Allows HR and TeamLead users to create reusable feedback templates with specific criteria and text sections.
/// </summary>
public record CreateFeedbackTemplateCommand(
    Guid Id,
    Translation Name,
    Translation Description,
    List<EvaluationItem> Criteria,
    List<TextSectionDefinition> TextSections,
    int RatingScale,
    string ScaleLowLabel,
    string ScaleHighLabel,
    List<FeedbackSourceType> AllowedSourceTypes) : ICommand<Result>;
