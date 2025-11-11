using ti8m.BeachBreak.Application.Command.Models;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record RatePredecessorGoalCommand(
    Guid AssignmentId,
    Guid QuestionId,
    Guid SourceAssignmentId,
    Guid SourceGoalId,
    PredecessorGoalData PredecessorGoalData,
    ApplicationRole RatedByRole,
    decimal DegreeOfAchievement,
    string Justification,
    Guid RatedByEmployeeId) : ICommand<Result>;
