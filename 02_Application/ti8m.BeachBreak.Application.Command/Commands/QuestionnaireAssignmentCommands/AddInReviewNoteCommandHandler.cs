using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles adding a note during the InReview phase of a questionnaire assignment
/// </summary>
public class AddInReviewNoteCommandHandler
    : ICommandHandler<AddInReviewNoteCommand, Result<Guid>>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly UserContext userContext;
    private readonly ILogger<AddInReviewNoteCommandHandler> logger;

    public AddInReviewNoteCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        UserContext userContext,
        ILogger<AddInReviewNoteCommandHandler> logger)
    {
        this.repository = repository;
        this.userContext = userContext;
        this.logger = logger;
    }

    public async Task<Result<Guid>> HandleAsync(AddInReviewNoteCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Adding InReview note for assignment {AssignmentId}", command.AssignmentId);

            if (!Guid.TryParse(userContext.Id, out var authorEmployeeId))
            {
                logger.LogError("Invalid user ID in context: {UserId}", userContext.Id);
                return Result<Guid>.Fail("Invalid user ID", 401);
            }

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                cancellationToken: cancellationToken);

            var noteId = assignment.AddInReviewNote(
                command.Content,
                command.SectionId,
                authorEmployeeId);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully added InReview note for assignment {AssignmentId} with note ID {NoteId}", command.AssignmentId, noteId);

            return Result<Guid>.Success(noteId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding InReview note for assignment {AssignmentId}", command.AssignmentId);
            return Result<Guid>.Fail($"Failed to add note: {ex.Message}", 500);
        }
    }
}