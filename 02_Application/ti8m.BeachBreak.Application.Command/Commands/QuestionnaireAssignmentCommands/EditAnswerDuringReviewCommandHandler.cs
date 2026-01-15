using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Mappers;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Application.Command.Models;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

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
                command.Answer,
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

            // Parse the answer - frontend sends QuestionResponseValue as JSON
            QuestionResponseValue? convertedResponse = null;

            // Deserialize QuestionResponseValue directly from JSON
            if (command.Answer is string answerString && answerString.TrimStart().StartsWith("{"))
            {
                try
                {
                    // Directly deserialize the type-safe QuestionResponseValue from JSON
                    convertedResponse = System.Text.Json.JsonSerializer.Deserialize<QuestionResponseValue>(answerString);

                    // TODO: Add audit metadata for edit tracking if needed
                    // This could be handled by adding metadata to the domain event instead
                }
                catch (System.Text.Json.JsonException jsonEx)
                {
                    logger.LogWarning(jsonEx,
                        "Failed to deserialize QuestionResponseValue JSON for assignment {AssignmentId}, question {QuestionId}. Attempting text fallback.",
                        command.AssignmentId, command.QuestionId);

                    // Fallback - create a simple TextResponse from the string
                    convertedResponse = new QuestionResponseValue.TextResponse(new[] { answerString });
                }
            }
            else if (command.Answer is string textAnswer)
            {
                // Handle simple text answers
                convertedResponse = new QuestionResponseValue.TextResponse(new[] { textAnswer });
            }
            else
            {
                logger.LogWarning("Unsupported answer format for assignment {AssignmentId}, question {QuestionId}. Answer type: {Type}",
                    command.AssignmentId, command.QuestionId, command.Answer?.GetType()?.Name ?? "null");
                return Result.Fail("Unsupported answer format", 400);
            }

            // Update or add the section answer if conversion was successful
            if (convertedResponse == null)
            {
                logger.LogWarning("Failed to convert answer for assignment {AssignmentId}, section {SectionId}. Answer will be skipped.",
                    command.AssignmentId, command.SectionId);
                return Result.Fail("Unable to convert answer to the required format", 400);
            }

            // Record the updated section response
            response.RecordSectionResponse(command.SectionId, completionRole, convertedResponse);
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
