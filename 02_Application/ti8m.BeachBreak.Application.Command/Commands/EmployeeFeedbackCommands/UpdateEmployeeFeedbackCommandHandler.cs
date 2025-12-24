using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Handles the updating of existing employee feedback.
/// Validates authorization and business rules before updating.
/// </summary>
public class UpdateEmployeeFeedbackCommandHandler
    : ICommandHandler<UpdateEmployeeFeedbackCommand, Result>
{
    private readonly IEmployeeFeedbackAggregateRepository repository;
    private readonly UserContext userContext;
    private readonly ILogger<UpdateEmployeeFeedbackCommandHandler> logger;

    public UpdateEmployeeFeedbackCommandHandler(
        IEmployeeFeedbackAggregateRepository repository,
        UserContext userContext,
        ILogger<UpdateEmployeeFeedbackCommandHandler> logger)
    {
        this.repository = repository;
        this.userContext = userContext;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(
        UpdateEmployeeFeedbackCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Updating feedback {FeedbackId} from {ProviderName}",
                command.FeedbackId,
                command.ProviderInfo.ProviderName);

            // Load the existing feedback
            var feedback = await repository.LoadAsync(command.FeedbackId, cancellationToken);

            if (feedback == null)
            {
                logger.LogWarning("Feedback {FeedbackId} not found", command.FeedbackId);
                return Result.Fail("Feedback not found", 404);
            }

            // Get the current user ID from UserContext
            if (!Guid.TryParse(userContext.Id, out var updatedByEmployeeId))
            {
                logger.LogWarning("Invalid user context ID: {UserId}", userContext.Id);
                return Result.Fail("Invalid user context", 401);
            }

            // Additional authorization checks can be added here if needed
            // For now, API-level authorization ensures only HR/TeamLead can update

            // Update the feedback
            feedback.UpdateFeedback(
                command.ProviderInfo,
                command.FeedbackDate,
                command.FeedbackData,
                updatedByEmployeeId);

            // Store the updated aggregate
            await repository.StoreAsync(feedback, cancellationToken);

            logger.LogInformation("Successfully updated feedback {FeedbackId}", command.FeedbackId);
            return Result.Success("Feedback updated successfully");
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid arguments for updating feedback {FeedbackId}", command.FeedbackId);
            return Result.Fail($"Invalid input: {ex.Message}", 400);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation for updating feedback {FeedbackId}", command.FeedbackId);
            return Result.Fail(ex.Message, 409);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating feedback {FeedbackId}", command.FeedbackId);
            return Result.Fail("An error occurred while updating the feedback", 500);
        }
    }
}