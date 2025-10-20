using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Services;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class SendAssignmentReminderCommandHandler : ICommandHandler<SendAssignmentReminderCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly INotificationService notificationService;
    private readonly ILogger<SendAssignmentReminderCommandHandler> logger;

    public SendAssignmentReminderCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        INotificationService notificationService,
        ILogger<SendAssignmentReminderCommandHandler> logger)
    {
        this.repository = repository;
        this.notificationService = notificationService;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(SendAssignmentReminderCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Sending reminder for assignment {AssignmentId} by {SentByEmployeeId}",
                command.AssignmentId, command.SentByEmployeeId);

            // Load the assignment to get employee details
            var assignment = await repository.LoadAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                cancellationToken: cancellationToken);

            if (assignment == null)
            {
                logger.LogWarning("Assignment {AssignmentId} not found", command.AssignmentId);
                return Result.Fail($"Assignment {command.AssignmentId} not found", 404);
            }

            // Validate assignment is not already completed or withdrawn
            if (assignment.IsWithdrawn)
            {
                logger.LogWarning("Cannot send reminder for withdrawn assignment {AssignmentId}", command.AssignmentId);
                return Result.Fail("Cannot send reminder for withdrawn assignment", 400);
            }

            if (assignment.CompletedDate.HasValue)
            {
                logger.LogWarning("Cannot send reminder for completed assignment {AssignmentId}", command.AssignmentId);
                return Result.Fail("Cannot send reminder for completed assignment", 400);
            }

            // Send notification to employee
            var subject = "Reminder: Questionnaire Assignment";
            var notificationSent = await notificationService.SendNotificationAsync(
                assignment.EmployeeEmail,
                subject,
                command.Message,
                cancellationToken);

            if (!notificationSent)
            {
                logger.LogError("Failed to send reminder notification for assignment {AssignmentId} to {Email}",
                    command.AssignmentId, assignment.EmployeeEmail);
                return Result.Fail("Failed to send reminder notification", 500);
            }

            logger.LogInformation(
                "Reminder sent successfully for assignment {AssignmentId} to {EmployeeName} ({Email}). Message: {Message}",
                command.AssignmentId, assignment.EmployeeName, assignment.EmployeeEmail, command.Message);

            return Result.Success("Reminder sent successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending reminder for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to send reminder: " + ex.Message, 500);
        }
    }
}
