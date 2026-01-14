using ti8m.BeachBreak.Application.Command.Models;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to link employee feedback to a questionnaire assignment question.
/// </summary>
public record LinkEmployeeFeedbackCommand(
    Guid AssignmentId,
    Guid QuestionId,
    Guid FeedbackId,
    ApplicationRole LinkedByRole,
    Guid LinkedByEmployeeId) : ICommand<Result>;
