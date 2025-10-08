using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;

public class QuestionnaireResponseCommandHandler :
    ICommandHandler<SaveEmployeeResponseCommand, Result<Guid>>,
    ICommandHandler<SubmitEmployeeResponseCommand, Result>
{
    private readonly IQuestionnaireResponseAggregateRepository repository;
    private readonly ILogger<QuestionnaireResponseCommandHandler> logger;

    public QuestionnaireResponseCommandHandler(
        IQuestionnaireResponseAggregateRepository repository,
        ILogger<QuestionnaireResponseCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result<Guid>> HandleAsync(SaveEmployeeResponseCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Processing save response for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                command.EmployeeId, command.AssignmentId);

            // Try to load existing response by assignment ID
            var response = await repository.FindByAssignmentIdAsync(command.AssignmentId, cancellationToken);

            if (response == null)
            {
                // First time - initiate new response with unique aggregate ID
                var responseId = Guid.NewGuid();
                response = new QuestionnaireResponse(
                    responseId,
                    command.AssignmentId,
                    command.EmployeeId,
                    DateTime.UtcNow);

                logger.LogInformation("Initiated new questionnaire response {ResponseId} for AssignmentId: {AssignmentId}", responseId, command.AssignmentId);
            }

            // Record section responses
            foreach (var section in command.SectionResponses)
            {
                var sectionId = section.Key;

                // The section.Value is already a Dictionary<Guid, object> after controller conversion
                if (section.Value is not Dictionary<Guid, object> questionResponses)
                {
                    logger.LogWarning("Section {SectionId} has invalid response type: {Type}", sectionId, section.Value?.GetType());
                    continue;
                }

                // Skip empty sections
                if (questionResponses.Count == 0)
                {
                    logger.LogDebug("Skipping empty section {SectionId}", sectionId);
                    continue;
                }

                response.RecordSectionResponse(sectionId, questionResponses);
            }

            await repository.StoreAsync(response, cancellationToken);

            logger.LogInformation("Successfully saved response for AssignmentId: {AssignmentId}", command.AssignmentId);
            return Result<Guid>.Success(response.Id);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation while saving response");
            return Result<Guid>.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving employee response for AssignmentId: {AssignmentId}", command.AssignmentId);
            return Result<Guid>.Fail($"Failed to save employee response: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result> HandleAsync(SubmitEmployeeResponseCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Processing submit response for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                command.EmployeeId, command.AssignmentId);

            var response = await repository.FindByAssignmentIdAsync(command.AssignmentId, cancellationToken);

            if (response == null)
            {
                logger.LogWarning("No response found for AssignmentId: {AssignmentId}", command.AssignmentId);
                return Result.Fail("No response found to submit", StatusCodes.Status404NotFound);
            }

            // Verify this response belongs to the requesting employee
            if (response.EmployeeId != command.EmployeeId)
            {
                logger.LogWarning("Employee {EmployeeId} attempted to submit response for Assignment {AssignmentId} belonging to {ActualEmployeeId}",
                    command.EmployeeId, command.AssignmentId, response.EmployeeId);
                return Result.Fail("Unauthorized to submit this response", StatusCodes.Status403Forbidden);
            }

            response.Submit();
            await repository.StoreAsync(response, cancellationToken);

            logger.LogInformation("Successfully submitted response for AssignmentId: {AssignmentId}", command.AssignmentId);
            return Result.Success("Response submitted successfully");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation while submitting response");
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting employee response for AssignmentId: {AssignmentId}", command.AssignmentId);
            return Result.Fail($"Failed to submit employee response: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}
