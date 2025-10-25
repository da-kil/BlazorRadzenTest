using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Services;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;
using ti8m.BeachBreak.Domain.EmployeeAggregate.Services;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the reopening of a questionnaire assignment for corrections.
/// Implements authorization checks and sends email notifications.
/// </summary>
public class ReopenQuestionnaireCommandHandler
    : ICommandHandler<ReopenQuestionnaireCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly IEmployeeAggregateRepository employeeRepository;
    private readonly IEmployeeHierarchyService hierarchyService;
    private readonly INotificationService notificationService;
    private readonly ILogger<ReopenQuestionnaireCommandHandler> logger;

    public ReopenQuestionnaireCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        IEmployeeAggregateRepository employeeRepository,
        IEmployeeHierarchyService hierarchyService,
        INotificationService notificationService,
        ILogger<ReopenQuestionnaireCommandHandler> logger)
    {
        this.repository = repository;
        this.employeeRepository = employeeRepository;
        this.hierarchyService = hierarchyService;
        this.notificationService = notificationService;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(
        ReopenQuestionnaireCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Attempting to reopen assignment {AssignmentId} from {CurrentState} to {TargetState} by user {UserId} with role {Role}",
                command.AssignmentId,
                "Unknown", // Will be determined after loading
                command.TargetState,
                command.ReopenedByEmployeeId,
                command.ReopenedByRole);

            // Validate reopen reason
            if (string.IsNullOrWhiteSpace(command.ReopenReason))
            {
                logger.LogWarning(
                    "Reopen rejected for assignment {AssignmentId}: Reopen reason is required",
                    command.AssignmentId);
                return Result.Fail("Reopen reason is required and cannot be empty", 400);
            }

            if (command.ReopenReason.Length < 10)
            {
                logger.LogWarning(
                    "Reopen rejected for assignment {AssignmentId}: Reopen reason too short ({Length} characters)",
                    command.AssignmentId,
                    command.ReopenReason.Length);
                return Result.Fail("Reopen reason must be at least 10 characters", 400);
            }

            // Load assignment aggregate
            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                cancellationToken: cancellationToken);

            // Check if finalized
            if (assignment.IsLocked)
            {
                logger.LogWarning(
                    "Reopen rejected for assignment {AssignmentId}: Assignment is finalized and locked",
                    command.AssignmentId);
                return Result.Fail("Cannot reopen finalized questionnaire. Create a new assignment instead.", 400);
            }

            // Check if withdrawn
            if (assignment.IsWithdrawn)
            {
                logger.LogWarning(
                    "Reopen rejected for assignment {AssignmentId}: Assignment is withdrawn",
                    command.AssignmentId);
                return Result.Fail("Cannot reopen withdrawn assignment", 400);
            }

            // TeamLead data-scoped authorization check using hierarchy service
            if (command.ReopenedByRole.Equals("TeamLead", StringComparison.OrdinalIgnoreCase))
            {
                var isInTeam = await hierarchyService.IsInTeamHierarchyAsync(
                    command.ReopenedByEmployeeId,
                    assignment.EmployeeId,
                    cancellationToken);

                if (!isInTeam)
                {
                    logger.LogWarning(
                        "Reopen rejected for assignment {AssignmentId}: TeamLead {TeamLeadId} cannot manage employee {EmployeeId}",
                        command.AssignmentId,
                        command.ReopenedByEmployeeId,
                        assignment.EmployeeId);
                    return Result.Fail("TeamLead can only reopen questionnaires for their own team members", 403);
                }
            }

            // Execute reopen (aggregate validates role-level authorization)
            assignment.ReopenWorkflow(
                command.TargetState,
                command.ReopenReason,
                command.ReopenedByEmployeeId,
                command.ReopenedByRole);

            // Capture old state before storing
            var oldState = assignment.WorkflowState;

            // Store aggregate
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation(
                "Successfully reopened assignment {AssignmentId} from {FromState} to {TargetState} by {Role}",
                command.AssignmentId,
                oldState,
                command.TargetState,
                command.ReopenedByRole);

            // Send email notifications
            await SendNotificationsAsync(
                assignment,
                oldState,
                command.TargetState,
                command.ReopenReason,
                command.ReopenedByEmployeeId,
                command.ReopenedByRole,
                cancellationToken);

            return Result.Success("Questionnaire reopened successfully. Email notifications sent.");
        }
        catch (InvalidWorkflowTransitionException ex)
        {
            logger.LogWarning(
                ex,
                "Reopen rejected for assignment {AssignmentId}: Invalid workflow transition",
                command.AssignmentId);
            return Result.Fail(ex.Message, 400);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(
                ex,
                "Reopen rejected for assignment {AssignmentId}: Invalid argument",
                command.AssignmentId);
            return Result.Fail(ex.Message, 400);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error reopening assignment {AssignmentId}",
                command.AssignmentId);
            return Result.Fail("Failed to reopen questionnaire: " + ex.Message, 500);
        }
    }

    /// <summary>
    /// Sends email notifications to affected parties when questionnaire is reopened.
    /// </summary>
    private async Task SendNotificationsAsync(
        Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment assignment,
        WorkflowState oldState,
        WorkflowState newState,
        string reopenReason,
        Guid reopenedByEmployeeId,
        string reopenedByRole,
        CancellationToken cancellationToken)
    {
        try
        {
            // Load the person who reopened (for their name)
            var reopenedByEmployee = await employeeRepository.LoadAsync<Domain.EmployeeAggregate.Employee>(
                reopenedByEmployeeId,
                cancellationToken: cancellationToken);

            var reopenedByName = reopenedByEmployee != null
                ? $"{reopenedByEmployee.FirstName} {reopenedByEmployee.LastName}"
                : "System Administrator";

            // Always notify the employee
            var notificationSent = await notificationService.SendQuestionnaireReopenedNotificationAsync(
                assignment.EmployeeEmail,
                assignment.EmployeeName,
                assignment.Id,
                oldState.ToString(),
                newState.ToString(),
                reopenReason,
                reopenedByName,
                reopenedByRole,
                cancellationToken);

            if (notificationSent)
            {
                logger.LogInformation(
                    "Sent reopened notification email to employee {EmployeeName} ({EmployeeEmail}) for assignment {AssignmentId}",
                    assignment.EmployeeName,
                    assignment.EmployeeEmail,
                    assignment.Id);
            }
            else
            {
                logger.LogWarning(
                    "Failed to send reopened notification email to employee {EmployeeName} ({EmployeeEmail}) for assignment {AssignmentId}",
                    assignment.EmployeeName,
                    assignment.EmployeeEmail,
                    assignment.Id);
            }

            // TODO: If manager review is required, also notify the manager
            // Would need to load employee aggregate to get ManagerId, then load manager to get email
        }
        catch (Exception ex)
        {
            // Log error but don't fail the operation
            logger.LogError(
                ex,
                "Failed to send email notifications for reopened assignment {AssignmentId}",
                assignment.Id);
        }
    }
}
