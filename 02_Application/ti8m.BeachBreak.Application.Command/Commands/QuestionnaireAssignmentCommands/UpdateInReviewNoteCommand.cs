namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to update a note during the InReview phase of a questionnaire assignment
/// </summary>
public record UpdateInReviewNoteCommand(
    Guid AssignmentId,
    Guid NoteId,
    string Content
) : ICommand<Result>;