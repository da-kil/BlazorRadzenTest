using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record LinkPredecessorQuestionnaireCommand(
    Guid AssignmentId,
    Guid QuestionId,
    Guid PredecessorAssignmentId,
    CompletionRole LinkedByRole,
    Guid LinkedByEmployeeId) : ICommand<Result>;
