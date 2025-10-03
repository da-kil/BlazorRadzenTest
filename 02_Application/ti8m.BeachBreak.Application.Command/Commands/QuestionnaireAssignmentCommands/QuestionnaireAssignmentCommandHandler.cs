using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class QuestionnaireAssignmentCommandHandler :
    ICommandHandler<CreateBulkAssignmentsCommand, Result>,
    ICommandHandler<StartAssignmentWorkCommand, Result>,
    ICommandHandler<CompleteAssignmentWorkCommand, Result>,
    ICommandHandler<ExtendAssignmentDueDateCommand, Result>,
    ICommandHandler<WithdrawAssignmentCommand, Result>,
    ICommandHandler<CompleteSectionAsEmployeeCommand, Result>,
    ICommandHandler<CompleteSectionAsManagerCommand, Result>,
    ICommandHandler<ConfirmEmployeeCompletionCommand, Result>,
    ICommandHandler<ConfirmManagerCompletionCommand, Result>,
    ICommandHandler<InitiateReviewCommand, Result>,
    ICommandHandler<EditAnswerDuringReviewCommand, Result>,
    ICommandHandler<ConfirmEmployeeReviewCommand, Result>,
    ICommandHandler<FinalizeQuestionnaireCommand, Result>
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

    public async Task<Result> HandleAsync(CompleteSectionAsEmployeeCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Employee completing section {SectionId} for assignment {AssignmentId}",
                command.SectionId, command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.CompleteSectionAsEmployee(command.SectionId);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully completed section {SectionId} as employee for assignment {AssignmentId}",
                command.SectionId, command.AssignmentId);
            return Result.Success("Section completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing section {SectionId} as employee for assignment {AssignmentId}",
                command.SectionId, command.AssignmentId);
            return Result.Fail("Failed to complete section: " + ex.Message, 500);
        }
    }

    public async Task<Result> HandleAsync(CompleteSectionAsManagerCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Manager completing section {SectionId} for assignment {AssignmentId}",
                command.SectionId, command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.CompleteSectionAsManager(command.SectionId);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully completed section {SectionId} as manager for assignment {AssignmentId}",
                command.SectionId, command.AssignmentId);
            return Result.Success("Section completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing section {SectionId} as manager for assignment {AssignmentId}",
                command.SectionId, command.AssignmentId);
            return Result.Fail("Failed to complete section: " + ex.Message, 500);
        }
    }

    public async Task<Result> HandleAsync(ConfirmEmployeeCompletionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Employee confirming completion for assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.ConfirmEmployeeCompletion(command.ConfirmedBy);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully confirmed employee completion for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Employee completion confirmed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming employee completion for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to confirm employee completion: " + ex.Message, 500);
        }
    }

    public async Task<Result> HandleAsync(ConfirmManagerCompletionCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Manager confirming completion for assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.ConfirmManagerCompletion(command.ConfirmedBy);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully confirmed manager completion for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Manager completion confirmed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming manager completion for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to confirm manager completion: " + ex.Message, 500);
        }
    }

    public async Task<Result> HandleAsync(InitiateReviewCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Initiating review for assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.InitiateReview(command.InitiatedBy);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully initiated review for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Review initiated");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initiating review for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to initiate review: " + ex.Message, 500);
        }
    }

    public async Task<Result> HandleAsync(EditAnswerDuringReviewCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Editing answer during review for assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.EditAnswerDuringReview(command.SectionId, command.QuestionId, command.Answer, command.EditedBy);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully edited answer during review for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Answer edited");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error editing answer during review for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to edit answer: " + ex.Message, 500);
        }
    }

    public async Task<Result> HandleAsync(ConfirmEmployeeReviewCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Employee confirming review for assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.ConfirmEmployeeReview(command.ConfirmedBy);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully confirmed employee review for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Employee review confirmed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming employee review for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to confirm employee review: " + ex.Message, 500);
        }
    }

    public async Task<Result> HandleAsync(FinalizeQuestionnaireCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Finalizing questionnaire for assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.Finalize(command.FinalizedBy);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully finalized questionnaire for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Questionnaire finalized");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finalizing questionnaire for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to finalize questionnaire: " + ex.Message, 500);
        }
    }

}
