using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

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
                command.OriginalCompletionRole,
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

            // Get current section responses for this role
            var currentSectionResponses = new Dictionary<Guid, object>();
            if (response.SectionResponses.TryGetValue(command.SectionId, out var roleResponses) &&
                roleResponses.TryGetValue(command.OriginalCompletionRole, out var existingQuestions))
            {
                // Copy existing responses
                currentSectionResponses = new Dictionary<Guid, object>(existingQuestions);
            }

            // Parse the answer - frontend now sends complete QuestionResponse structure as JSON
            object questionResponseStructure = command.Answer;

            // Deserialize the JSON string to a dictionary (QuestionResponse structure from frontend)
            if (command.Answer is string answerString && answerString.TrimStart().StartsWith("{"))
            {
                try
                {
                    var deserialized = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                        answerString,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (deserialized != null)
                    {
                        // Add metadata to track edit during review
                        deserialized["EditedDuringReview"] = true;
                        deserialized["EditedDuringReviewBy"] = command.EditedByEmployeeId.ToString();
                        deserialized["EditedDuringReviewAt"] = DateTime.UtcNow.ToString("O"); // ISO 8601 format

                        questionResponseStructure = deserialized;
                    }
                }
                catch (System.Text.Json.JsonException jsonEx)
                {
                    // If deserialization fails, keep as string (fallback for unexpected format)
                    logger.LogWarning(jsonEx,
                        "Failed to deserialize answer JSON for assignment {AssignmentId}, question {QuestionId}. Using string fallback.",
                        command.AssignmentId, command.QuestionId);
                    questionResponseStructure = answerString;
                }
            }

            // Update or add the question answer (frontend provides complete structure with metadata)
            currentSectionResponses[command.QuestionId] = questionResponseStructure;

            // Record the updated section response
            response.RecordSectionResponse(command.SectionId, command.OriginalCompletionRole, currentSectionResponses);
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
