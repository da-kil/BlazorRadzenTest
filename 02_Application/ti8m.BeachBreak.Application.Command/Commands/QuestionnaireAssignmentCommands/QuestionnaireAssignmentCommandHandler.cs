using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class QuestionnaireAssignmentCommandHandler :
    ICommandHandler<CreateAssignmentCommand, Result>,
    ICommandHandler<CreateBulkAssignmentsCommand, Result>,
    ICommandHandler<StartAssignmentWorkCommand, Result>,
    ICommandHandler<CompleteAssignmentWorkCommand, Result>,
    ICommandHandler<ExtendAssignmentDueDateCommand, Result>,
    ICommandHandler<WithdrawAssignmentCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<QuestionnaireAssignmentCommandHandler> logger;

    public QuestionnaireAssignmentCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<QuestionnaireAssignmentCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(CreateAssignmentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var assignmentId = Guid.NewGuid();

            logger.LogInformation("Creating assignment {AssignmentId} for employee {EmployeeId} with template {TemplateId}",
                assignmentId, command.EmployeeId, command.TemplateId);

            var assignment = new Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment(
                assignmentId,
                command.TemplateId,
                command.EmployeeId,
                command.EmployeeName,
                command.EmployeeEmail,
                DateTime.UtcNow,
                command.DueDate,
                command.AssignedBy,
                command.Notes);

            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully created assignment {AssignmentId}", assignmentId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating questionnaire assignment");
            return Result.Fail("Failed to create assignment: " + ex.Message, 500);
        }
    }

    public async Task<Result> HandleAsync(CreateBulkAssignmentsCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Creating {EmployeeCount} assignments with template {TemplateId}",
                command.EmployeeAssignments.Count, command.TemplateId);

            var createdAssignmentIds = new List<Guid>();
            var assignedDate = DateTime.UtcNow;

            foreach (var employeeData in command.EmployeeAssignments)
            {
                var assignmentId = Guid.NewGuid();

                var assignment = new Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment(
                    assignmentId,
                    command.TemplateId,
                    employeeData.EmployeeId,
                    employeeData.EmployeeName,
                    employeeData.EmployeeEmail,
                    assignedDate,
                    command.DueDate,
                    command.AssignedBy,
                    command.Notes);

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

    public async Task<Result> HandleAsync(StartAssignmentWorkCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting work on assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.StartWork();
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully started work on assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Assignment work started");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting work on assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to start assignment work: " + ex.Message, 500);
        }
    }

    public async Task<Result> HandleAsync(CompleteAssignmentWorkCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Completing work on assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.CompleteWork();
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully completed work on assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Assignment work completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing work on assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to complete assignment work: " + ex.Message, 500);
        }
    }

    public async Task<Result> HandleAsync(ExtendAssignmentDueDateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Extending due date for assignment {AssignmentId} to {NewDueDate}",
                command.AssignmentId, command.NewDueDate);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.ExtendDueDate(command.NewDueDate, command.ExtensionReason);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully extended due date for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Assignment due date extended");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extending due date for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to extend assignment due date: " + ex.Message, 500);
        }
    }

    public async Task<Result> HandleAsync(WithdrawAssignmentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Withdrawing assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.Withdraw(command.WithdrawnBy, command.WithdrawalReason);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully withdrew assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Assignment withdrawn");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error withdrawing assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to withdraw assignment: " + ex.Message, 500);
        }
    }

}
