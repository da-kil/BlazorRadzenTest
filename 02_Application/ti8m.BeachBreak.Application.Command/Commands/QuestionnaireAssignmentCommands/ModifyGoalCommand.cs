using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record ModifyGoalCommand(
    Guid AssignmentId,
    Guid GoalId,
    DateTime? TimeframeFrom,
    DateTime? TimeframeTo,
    string? ObjectiveDescription,
    string? MeasurementMetric,
    decimal? WeightingPercentage,
    CompletionRole ModifiedByRole,
    string ChangeReason,
    Guid ModifiedByEmployeeId) : ICommand<Result>;
