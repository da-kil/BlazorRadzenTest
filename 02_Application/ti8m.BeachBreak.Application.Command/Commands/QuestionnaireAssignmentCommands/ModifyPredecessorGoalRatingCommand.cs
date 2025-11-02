using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record ModifyPredecessorGoalRatingCommand(
    Guid AssignmentId,
    Guid SourceGoalId,
    decimal? DegreeOfAchievement,
    string? Justification,
    ApplicationRole ModifiedByRole,
    string ChangeReason,
    Guid ModifiedByEmployeeId) : ICommand<Result>;
