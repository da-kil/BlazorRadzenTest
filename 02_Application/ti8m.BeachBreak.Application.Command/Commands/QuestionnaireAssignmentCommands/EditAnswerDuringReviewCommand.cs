using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record EditAnswerDuringReviewCommand(
    Guid AssignmentId,
    Guid SectionId,
    Guid QuestionId,
    ApplicationRole OriginalCompletionRole,
    string Answer,
    Guid EditedByEmployeeId) : ICommand<Result>;
