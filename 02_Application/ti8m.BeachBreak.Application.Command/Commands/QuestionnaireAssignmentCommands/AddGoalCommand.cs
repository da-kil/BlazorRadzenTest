using ti8m.BeachBreak.Application.Command.Models;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to add a new goal to a questionnaire assignment.
/// Weighting is optional during in-progress states (defaults to 0) and should be set during InReview.
/// </summary>
public record AddGoalCommand(
    Guid AssignmentId,
    Guid QuestionId,
    ApplicationRole AddedByRole,
    DateTime TimeframeFrom,
    DateTime TimeframeTo,
    string ObjectiveDescription,
    string MeasurementMetric,
    decimal? WeightingPercentage, // Optional - defaults to 0 if not provided
    Guid AddedByEmployeeId) : ICommand<Result<Guid>>; // Returns the new Goal ID
