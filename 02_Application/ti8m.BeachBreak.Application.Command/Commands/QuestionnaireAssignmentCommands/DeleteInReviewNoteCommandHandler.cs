using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles deleting a note during the InReview phase of a questionnaire assignment
/// </summary>
public class DeleteInReviewNoteCommandHandler
    : ICommandHandler<DeleteInReviewNoteCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly UserContext userContext;
    private readonly ILogger<DeleteInReviewNoteCommandHandler> logger;

    public DeleteInReviewNoteCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        UserContext userContext,
        ILogger<DeleteInReviewNoteCommandHandler> logger)
    {
        this.repository = repository;
        this.userContext = userContext;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(DeleteInReviewNoteCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Deleting InReview note {NoteId} for assignment {AssignmentId}", command.NoteId, command.AssignmentId);

            if (!Guid.TryParse(userContext.Id, out var deleterEmployeeId))
            {
                logger.LogError("Invalid user ID in context: {UserId}", userContext.Id);
                return Result.Fail("Invalid user ID", 401);
            }

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                cancellationToken: cancellationToken);

            assignment.DeleteInReviewNote(command.NoteId, deleterEmployeeId);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully deleted InReview note {NoteId} for assignment {AssignmentId}", command.NoteId, command.AssignmentId);

            return Result.Success("Note deleted successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting InReview note {NoteId} for assignment {AssignmentId}", command.NoteId, command.AssignmentId);
            return Result.Fail($"Failed to delete note: {ex.Message}", 500);
        }
    }
}