using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record LinkPredecessorQuestionnaireCommand(
    Guid AssignmentId,
    Guid QuestionId,
    Guid PredecessorAssignmentId,
    ApplicationRole LinkedByRole,
    Guid LinkedByEmployeeId) : ICommand<Result>;
