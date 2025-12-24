using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles updating a note during the InReview phase of a questionnaire assignment
/// </summary>
public class UpdateInReviewNoteCommandHandler
    : ICommandHandler<UpdateInReviewNoteCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly UserContext userContext;
    private readonly ILogger<UpdateInReviewNoteCommandHandler> logger;

    public UpdateInReviewNoteCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        UserContext userContext,
        ILogger<UpdateInReviewNoteCommandHandler> logger)
    {
        this.repository = repository;
        this.userContext = userContext;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(UpdateInReviewNoteCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Updating InReview note {NoteId} for assignment {AssignmentId}", command.NoteId, command.AssignmentId);

            if (!Guid.TryParse(userContext.Id, out var editorEmployeeId))
            {
                logger.LogError("Invalid user ID in context: {UserId}", userContext.Id);
                return Result.Fail("Invalid user ID", 401);
            }

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                cancellationToken: cancellationToken);

            assignment.UpdateInReviewNote(command.NoteId, command.Content, editorEmployeeId);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully updated InReview note {NoteId} for assignment {AssignmentId}", command.NoteId, command.AssignmentId);

            return Result.Success("Note updated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating InReview note {NoteId} for assignment {AssignmentId}", command.NoteId, command.AssignmentId);
            return Result.Fail($"Failed to update note: {ex.Message}", 500);
        }
    }
}