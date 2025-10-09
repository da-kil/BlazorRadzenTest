using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;

public class QuestionnaireResponseCommandHandler :
    ICommandHandler<SaveEmployeeResponseCommand, Result<Guid>>,
    ICommandHandler<SaveManagerResponseCommand, Result<Guid>>,
    ICommandHandler<SubmitEmployeeResponseCommand, Result>
{
    private readonly IQuestionnaireResponseAggregateRepository repository;
    private readonly IQuestionnaireAssignmentAggregateRepository assignmentRepository;
    private readonly ILogger<QuestionnaireResponseCommandHandler> logger;

    public QuestionnaireResponseCommandHandler(
        IQuestionnaireResponseAggregateRepository repository,
        IQuestionnaireAssignmentAggregateRepository assignmentRepository,
        ILogger<QuestionnaireResponseCommandHandler> logger)
    {
        this.repository = repository;
        this.assignmentRepository = assignmentRepository;
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

            // Record section responses with Employee role
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

                response.RecordSectionResponse(sectionId, CompletionRole.Employee, questionResponses);
            }

            await repository.StoreAsync(response, cancellationToken);

            logger.LogInformation("Successfully saved employee response for AssignmentId: {AssignmentId}", command.AssignmentId);
            return Result<Guid>.Success(response.Id);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation while saving employee response");
            return Result<Guid>.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving employee response for AssignmentId: {AssignmentId}", command.AssignmentId);
            return Result<Guid>.Fail($"Failed to save employee response: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<Guid>> HandleAsync(SaveManagerResponseCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Processing save manager response for ManagerId: {ManagerId}, AssignmentId: {AssignmentId}",
                command.ManagerId, command.AssignmentId);

            // Try to load existing response by assignment ID
            var response = await repository.FindByAssignmentIdAsync(command.AssignmentId, cancellationToken);

            if (response == null)
            {
                // First time - need to get employee ID from assignment
                var assignment = await assignmentRepository.LoadAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                    command.AssignmentId,
                    cancellationToken: cancellationToken);

                if (assignment == null)
                {
                    logger.LogWarning("Assignment {AssignmentId} not found", command.AssignmentId);
                    return Result<Guid>.Fail("Assignment not found", StatusCodes.Status404NotFound);
                }

                // Initiate new response with employee ID from assignment
                var responseId = Guid.NewGuid();
                response = new QuestionnaireResponse(
                    responseId,
                    command.AssignmentId,
                    assignment.EmployeeId,
                    DateTime.UtcNow);

                logger.LogInformation("Initiated new questionnaire response {ResponseId} for manager input on AssignmentId: {AssignmentId}, EmployeeId: {EmployeeId}",
                    responseId, command.AssignmentId, assignment.EmployeeId);
            }

            // Record section responses with Manager role
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

                response.RecordSectionResponse(sectionId, CompletionRole.Manager, questionResponses);
            }

            await repository.StoreAsync(response, cancellationToken);

            logger.LogInformation("Successfully saved manager response for AssignmentId: {AssignmentId}", command.AssignmentId);
            return Result<Guid>.Success(response.Id);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation while saving manager response");
            return Result<Guid>.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving manager response for AssignmentId: {AssignmentId}", command.AssignmentId);
            return Result<Guid>.Fail($"Failed to save manager response: {ex.Message}", StatusCodes.Status500InternalServerError);
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
