using ti8m.BeachBreak.Application.Command.Models;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record EditAnswerDuringReviewCommand(
    Guid AssignmentId,
    Guid SectionId,
    Guid QuestionId,
    ApplicationRole OriginalCompletionRole,
    string Answer,
    Guid EditedByEmployeeId) : ICommand<Result>;
