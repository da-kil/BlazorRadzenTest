using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Handles the soft deletion of employee feedback.
/// Maintains audit trail while marking feedback as deleted.
/// </summary>
public class DeleteEmployeeFeedbackCommandHandler
    : ICommandHandler<DeleteEmployeeFeedbackCommand, Result>
{
    private readonly IEmployeeFeedbackAggregateRepository repository;
    private readonly UserContext userContext;
    private readonly ILogger<DeleteEmployeeFeedbackCommandHandler> logger;

    public DeleteEmployeeFeedbackCommandHandler(
        IEmployeeFeedbackAggregateRepository repository,
        UserContext userContext,
        ILogger<DeleteEmployeeFeedbackCommandHandler> logger)
    {
        this.repository = repository;
        this.userContext = userContext;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(
        DeleteEmployeeFeedbackCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Deleting feedback {FeedbackId}", command.FeedbackId);

            // Load the existing feedback
            var feedback = await repository.LoadAsync(command.FeedbackId, cancellationToken);

            if (feedback == null)
            {
                logger.LogWarning("Feedback {FeedbackId} not found", command.FeedbackId);
                return Result.Fail("Feedback not found", 404);
            }

            // Get the current user ID from UserContext
            if (!Guid.TryParse(userContext.Id, out var deletedByEmployeeId))
            {
                logger.LogWarning("Invalid user context ID: {UserId}", userContext.Id);
                return Result.Fail("Invalid user context", 401);
            }

            // Additional authorization checks can be added here if needed
            // For now, API-level authorization ensures only HR+ role can delete

            // Soft delete the feedback
            feedback.DeleteFeedback(deletedByEmployeeId, command.DeleteReason);

            // Store the updated aggregate
            await repository.StoreAsync(feedback, cancellationToken);

            logger.LogInformation(
                "Successfully deleted feedback {FeedbackId} by user {UserId}",
                command.FeedbackId,
                deletedByEmployeeId);

            return Result.Success("Feedback deleted successfully");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation for deleting feedback {FeedbackId}", command.FeedbackId);
            return Result.Fail(ex.Message, 409);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting feedback {FeedbackId}", command.FeedbackId);
            return Result.Fail("An error occurred while deleting the feedback", 500);
        }
    }
}