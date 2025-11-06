using ti8m.BeachBreak.Application.Command.Models;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record LinkPredecessorQuestionnaireCommand(
    Guid AssignmentId,
    Guid QuestionId,
    Guid PredecessorAssignmentId,
    ApplicationRole LinkedByRole,
    Guid LinkedByEmployeeId) : ICommand<Result>;
