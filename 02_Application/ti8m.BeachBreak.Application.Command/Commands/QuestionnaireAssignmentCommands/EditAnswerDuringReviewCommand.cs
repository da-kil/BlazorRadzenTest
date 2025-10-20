using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record EditAnswerDuringReviewCommand(
    Guid AssignmentId,
    Guid SectionId,
    Guid QuestionId,
    CompletionRole OriginalCompletionRole,
    string Answer,
    Guid EditedByEmployeeId) : ICommand<Result>;
