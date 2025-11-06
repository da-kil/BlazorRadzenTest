using ti8m.BeachBreak.Application.Command.Models;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record RatePredecessorGoalCommand(
    Guid AssignmentId,
    Guid QuestionId,
    Guid SourceAssignmentId,
    Guid SourceGoalId,
    ApplicationRole RatedByRole,
    decimal DegreeOfAchievement,
    string Justification,
    Guid RatedByEmployeeId) : ICommand<Result>;
