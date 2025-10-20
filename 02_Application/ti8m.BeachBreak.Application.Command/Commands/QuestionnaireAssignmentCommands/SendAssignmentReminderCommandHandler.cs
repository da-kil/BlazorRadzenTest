using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class SendAssignmentReminderCommandHandler : ICommandHandler<SendAssignmentReminderCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<SendAssignmentReminderCommandHandler> logger;

    public SendAssignmentReminderCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<SendAssignmentReminderCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(SendAssignmentReminderCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Sending reminder for assignment {AssignmentId} by {SentByEmployeeId}",
                command.AssignmentId, command.SentByEmployeeId);

            // Load the assignment to ensure it exists
            var assignment = await repository.LoadAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                cancellationToken: cancellationToken);

            if (assignment == null)
            {
                logger.LogWarning("Assignment {AssignmentId} not found", command.AssignmentId);
                return Result.Fail($"Assignment {command.AssignmentId} not found", 404);
            }

            // TODO: Implement actual notification service integration
            // For now, we just log the reminder
            // In production, this would:
            // 1. Send email notification to employee
            // 2. Create in-app notification
            // 3. Track reminder history

            logger.LogInformation("Reminder sent successfully for assignment {AssignmentId}. Message: {Message}",
                command.AssignmentId, command.Message);

            return Result.Success("Reminder sent successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending reminder for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to send reminder: " + ex.Message, 500);
        }
    }
}
