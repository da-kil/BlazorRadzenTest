using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record RatePredecessorGoalCommand(
    Guid AssignmentId,
    Guid QuestionId,
    Guid SourceAssignmentId,
    Guid SourceGoalId,
    CompletionRole RatedByRole,
    decimal DegreeOfAchievement,
    string Justification,
    Guid RatedByEmployeeId) : ICommand<Result>;
