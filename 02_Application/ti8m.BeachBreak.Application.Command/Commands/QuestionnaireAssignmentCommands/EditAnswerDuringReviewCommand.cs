using ti8m.BeachBreak.Application.Command.Models;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record EditAnswerDuringReviewCommand(
    Guid AssignmentId,
    Guid SectionId,
    Guid QuestionId,
    ApplicationRole OriginalCompletionRole,
    string AnswerJson,
    QuestionResponseValue Answer,
    Guid EditedByEmployeeId
) : ICommand<Result>;
