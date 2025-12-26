namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to add a note during the InReview phase of a questionnaire assignment
/// </summary>
public record AddInReviewNoteCommand(
    Guid AssignmentId,
    string Content,
    Guid? SectionId
) : ICommand<Result<Guid>>;