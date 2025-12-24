namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to delete a note during the InReview phase of a questionnaire assignment
/// </summary>
public record DeleteInReviewNoteCommand(
    Guid AssignmentId,
    Guid NoteId
) : ICommand<Result>;