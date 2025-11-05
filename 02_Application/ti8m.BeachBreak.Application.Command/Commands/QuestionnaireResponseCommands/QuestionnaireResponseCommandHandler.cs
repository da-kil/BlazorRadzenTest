using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
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

            // Check workflow state to ensure editing is allowed for employee
            var assignment = await assignmentRepository.LoadAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                cancellationToken: cancellationToken);

            if (assignment == null)
            {
                logger.LogWarning("Assignment {AssignmentId} not found", command.AssignmentId);
                return Result<Guid>.Fail("Assignment not found", StatusCodes.Status404NotFound);
            }

            // Validate employee can edit based on workflow state
            if (!assignment.CanEmployeeEdit())
            {
                logger.LogWarning("Employee cannot edit in workflow state {WorkflowState}", assignment.WorkflowState);
                return Result<Guid>.Fail($"Cannot edit questionnaire in {assignment.WorkflowState} state", StatusCodes.Status400BadRequest);
            }

            // Try to load existing response by assignment ID
            var response = await repository.FindByAssignmentIdAsync(command.AssignmentId, cancellationToken);

            bool isFirstSave = response == null;

            if (response == null)
            {
                // First time - initiate new response with unique aggregate ID
                var responseId = Guid.NewGuid();
                response = new QuestionnaireResponse(
                    responseId,
                    command.AssignmentId,
                    assignment.TemplateId,
                    command.EmployeeId,
                    DateTime.UtcNow);

                logger.LogInformation("Initiated new questionnaire response {ResponseId} for AssignmentId: {AssignmentId}, TemplateId: {TemplateId}",
                    responseId, command.AssignmentId, assignment.TemplateId);
            }

            // Record section responses with Employee role
            foreach (var section in command.SectionResponses)
            {
                var sectionId = section.Key;
                var roleBasedResponses = section.Value;

                // Extract Employee role responses
                if (!roleBasedResponses.TryGetValue(CompletionRole.Employee, out var questionResponses))
                {
                    logger.LogDebug("Section {SectionId} has no Employee responses", sectionId);
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

            // If this is the first time the employee is saving progress, mark the assignment as started
            // StartWork() will automatically transition workflow state appropriately
            if (isFirstSave)
            {
                assignment.StartWork(ApplicationRole.Employee); // Sets StartedDate and updates workflow state
                await assignmentRepository.StoreAsync(assignment, cancellationToken);
                logger.LogInformation("Assignment {AssignmentId} marked as started by employee", command.AssignmentId);
            }

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

            // Load assignment and check workflow state
            var assignment = await assignmentRepository.LoadAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                cancellationToken: cancellationToken);

            if (assignment == null)
            {
                logger.LogWarning("Assignment {AssignmentId} not found", command.AssignmentId);
                return Result<Guid>.Fail("Assignment not found", StatusCodes.Status404NotFound);
            }

            // Validate manager can edit based on workflow state
            if (!assignment.CanManagerEdit())
            {
                logger.LogWarning("Manager cannot edit in workflow state {WorkflowState}", assignment.WorkflowState);
                return Result<Guid>.Fail($"Cannot edit questionnaire in {assignment.WorkflowState} state", StatusCodes.Status400BadRequest);
            }

            // Try to load existing response by assignment ID
            var response = await repository.FindByAssignmentIdAsync(command.AssignmentId, cancellationToken);

            bool isFirstSave = response == null;

            if (response == null)
            {
                // Initiate new response with employee ID from assignment
                var responseId = Guid.NewGuid();
                response = new QuestionnaireResponse(
                    responseId,
                    command.AssignmentId,
                    assignment.TemplateId,
                    assignment.EmployeeId,
                    DateTime.UtcNow);

                logger.LogInformation("Initiated new questionnaire response {ResponseId} for manager input on AssignmentId: {AssignmentId}, TemplateId: {TemplateId}, EmployeeId: {EmployeeId}",
                    responseId, command.AssignmentId, assignment.TemplateId, assignment.EmployeeId);
            }

            // Record section responses with Manager role
            foreach (var section in command.SectionResponses)
            {
                var sectionId = section.Key;

                var roleBasedResponses = section.Value;

                // Extract Manager role responses
                if (!roleBasedResponses.TryGetValue(CompletionRole.Manager, out var questionResponses))
                {
                    logger.LogDebug("Section {SectionId} has no Manager responses", sectionId);
                    continue;
                }

                // Skip empty sections
                if (questionResponses.Count == 0)
                {
                    logger.LogDebug("Skipping empty section {SectionId}", sectionId);
                    continue;
                }

                // Already type-safe - no conversion needed!
                response.RecordSectionResponse(sectionId, CompletionRole.Manager, questionResponses);
            }

            await repository.StoreAsync(response, cancellationToken);

            // If this is the first time the manager is saving a response, mark the assignment as started
            // StartWork() will automatically transition workflow state appropriately
            if (isFirstSave)
            {
                assignment.StartWork(ApplicationRole.TeamLead); // Sets StartedDate and updates workflow state
                await assignmentRepository.StoreAsync(assignment, cancellationToken);
                logger.LogInformation("Assignment {AssignmentId} marked as started by manager", command.AssignmentId);
            }

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

            // Submission is now handled by the QuestionnaireAssignment aggregate's workflow state
            // Update assignment workflow state via SubmitEmployeeQuestionnaire
            var assignment = await assignmentRepository.LoadAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                cancellationToken: cancellationToken);

            if (assignment == null)
            {
                logger.LogWarning("Assignment {AssignmentId} not found", command.AssignmentId);
                return Result.Fail("Assignment not found", StatusCodes.Status404NotFound);
            }

            // Submit via assignment aggregate (which manages workflow state)
            assignment.SubmitEmployeeQuestionnaire(command.EmployeeId);
            await assignmentRepository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully submitted employee questionnaire for AssignmentId: {AssignmentId}, new state: {WorkflowState}",
                command.AssignmentId, assignment.WorkflowState);
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
