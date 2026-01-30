using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Mappers;
using ti8m.BeachBreak.Application.Command.Models;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Domain;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles editing of questionnaire answers during a review meeting.
/// This is the most complex handler as it:
/// 1. Raises an audit event on the assignment aggregate (who edited what)
/// 2. Updates the actual answer in the response aggregate (the new value)
/// </summary>
public class EditAnswerDuringReviewCommandHandler
    : ICommandHandler<EditAnswerDuringReviewCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly IQuestionnaireResponseAggregateRepository responseRepository;
    private readonly ILogger<EditAnswerDuringReviewCommandHandler> logger;

    public EditAnswerDuringReviewCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        IQuestionnaireResponseAggregateRepository responseRepository,
        ILogger<EditAnswerDuringReviewCommandHandler> logger)
    {
        this.repository = repository;
        this.responseRepository = responseRepository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(EditAnswerDuringReviewCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Editing answer during review for assignment {AssignmentId}", command.AssignmentId);

            // 1. Raise audit event on assignment aggregate
            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.EditAnswerAsManagerDuringReview(
                command.SectionId,
                command.QuestionId,
                ApplicationRoleMapper.MapToDomain(command.OriginalCompletionRole),
                command.AnswerJson,
                command.EditedByEmployeeId);
            await repository.StoreAsync(assignment, cancellationToken);

            // 2. Update the actual answer in the response aggregate
            var response = await responseRepository.FindByAssignmentIdAsync(command.AssignmentId, cancellationToken);
            if (response == null)
            {
                logger.LogError("QuestionnaireResponse not found for assignment {AssignmentId}", command.AssignmentId);
                return Result.Fail("Response not found", 404);
            }

            // Map ApplicationRole to CompletionRole for compatibility with Response aggregate
            var completionRole = command.OriginalCompletionRole == ApplicationRole.Employee ? CompletionRole.Employee : CompletionRole.Manager;

            // Record the updated section response (Answer is already a domain object)
            response.RecordSectionResponse(command.SectionId, completionRole, command.Answer);
            await responseRepository.StoreAsync(response, cancellationToken);

            logger.LogInformation("Successfully edited answer during review for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Answer edited");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error editing answer during review for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to edit answer: " + ex.Message, 500);
        }
    }
}
