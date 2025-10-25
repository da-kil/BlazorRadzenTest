using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record ModifyPredecessorGoalRatingCommand(
    Guid AssignmentId,
    Guid SourceGoalId,
    decimal? DegreeOfAchievement,
    string? Justification,
    CompletionRole ModifiedByRole,
    string ChangeReason,
    Guid ModifiedByEmployeeId) : ICommand<Result>;
