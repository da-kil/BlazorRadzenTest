using ti8m.BeachBreak.Application.Command.Models;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record LinkAssignmentPredecessorCommand(
    Guid AssignmentId,
    Guid PredecessorAssignmentId,
    ApplicationRole LinkedByRole,
    Guid LinkedByEmployeeId) : ICommand<Result>;