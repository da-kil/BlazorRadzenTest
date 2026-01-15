using ti8m.BeachBreak.Application.Command.Models;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to unlink employee feedback from a questionnaire assignment question.
/// </summary>
public record UnlinkEmployeeFeedbackCommand(
    Guid AssignmentId,
    Guid QuestionId,
    Guid FeedbackId,
    ApplicationRole UnlinkedByRole,
    Guid UnlinkedByEmployeeId) : ICommand<Result>;
