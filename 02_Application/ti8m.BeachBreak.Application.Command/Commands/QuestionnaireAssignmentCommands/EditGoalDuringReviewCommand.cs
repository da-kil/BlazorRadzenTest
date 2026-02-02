using ti8m.BeachBreak.Application.Command.Models;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to edit a specific goal during review process.
/// Dedicated command for goal editing with focused parameters.
/// </summary>
public record EditGoalDuringReviewCommand(
    Guid AssignmentId,
    Guid GoalId,
    Guid SectionId,
    Guid QuestionId,
    ApplicationRole OriginalCompletionRole,
    string ObjectiveDescription,
    string MeasurementMetric,
    DateTime TimeframeFrom,
    DateTime TimeframeTo,
    decimal WeightingPercentage,
    Guid EditedByEmployeeId
) : ICommand<Result>;