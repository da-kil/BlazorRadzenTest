using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the creation of multiple questionnaire assignments in a single operation.
/// </summary>
public class CreateBulkAssignmentsCommandHandler
    : ICommandHandler<CreateBulkAssignmentsCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly IQuestionnaireTemplateAggregateRepository templateRepository;
    private readonly ILogger<CreateBulkAssignmentsCommandHandler> logger;

    public CreateBulkAssignmentsCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        IQuestionnaireTemplateAggregateRepository templateRepository,
        ILogger<CreateBulkAssignmentsCommandHandler> logger)
    {
        this.repository = repository;
        this.templateRepository = templateRepository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(CreateBulkAssignmentsCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Creating {EmployeeCount} assignments with template {TemplateId}",
                command.EmployeeAssignments.Count, command.TemplateId);

            // Load the template to check auto-initialization setting
            var template = await templateRepository.LoadRequiredAsync<QuestionnaireTemplate>(command.TemplateId, null, cancellationToken);

            logger.LogInformation("Template {TemplateId} AutoInitialize: {AutoInitialize}",
                command.TemplateId, template.AutoInitialize);

            var createdAssignmentIds = new List<Guid>();
            var assignedDate = DateTime.UtcNow;

            foreach (var employeeData in command.EmployeeAssignments)
            {
                var assignmentId = Guid.NewGuid();

                var assignment = new Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment(
                    assignmentId,
                    command.TemplateId,
                    command.ProcessType,
                    employeeData.EmployeeId,
                    employeeData.EmployeeName,
                    employeeData.EmployeeEmail,
                    assignedDate,
                    command.DueDate,
                    command.AssignedBy,
                    command.Notes);

                // Auto-initialize templates configured for auto-initialization
                if (template.AutoInitialize && command.AssignedByEmployeeId.HasValue)
                {
                    assignment.StartInitialization(
                        command.AssignedByEmployeeId.Value,
                        "Auto-initialized per template configuration");

                    logger.LogInformation("Auto-initialized assignment {AssignmentId} per template configuration",
                        assignmentId);
                }

                await repository.StoreAsync(assignment, cancellationToken);
                createdAssignmentIds.Add(assignmentId);

                logger.LogInformation("Created assignment {AssignmentId} for employee {EmployeeId}",
                    assignmentId, employeeData.EmployeeId);
            }

            logger.LogInformation("Successfully created {AssignmentCount} assignments", createdAssignmentIds.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating bulk assignments");
            return Result.Fail("Failed to create assignments: " + ex.Message, 500);
        }
    }
}
