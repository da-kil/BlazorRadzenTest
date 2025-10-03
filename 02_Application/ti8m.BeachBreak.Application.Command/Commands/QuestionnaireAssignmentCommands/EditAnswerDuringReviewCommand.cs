namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public record EditAnswerDuringReviewCommand(
    Guid AssignmentId,
    Guid SectionId,
    Guid QuestionId,
    string Answer,
    string EditedBy) : ICommand<Result>;
