using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record AddGoalCommand(
    Guid AssignmentId,
    Guid QuestionId,
    CompletionRole AddedByRole,
    DateTime TimeframeFrom,
    DateTime TimeframeTo,
    string ObjectiveDescription,
    string MeasurementMetric,
    decimal WeightingPercentage,
    Guid AddedByEmployeeId) : ICommand<Result<Guid>>; // Returns the new Goal ID
