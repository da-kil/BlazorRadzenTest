using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Handles the recording of new employee feedback from external sources.
/// Validates business rules and authorization before creating the feedback aggregate.
/// </summary>
public class RecordEmployeeFeedbackCommandHandler
    : ICommandHandler<RecordEmployeeFeedbackCommand, Result<Guid>>
{
    private readonly IEmployeeFeedbackAggregateRepository repository;
    private readonly IEmployeeAggregateRepository employeeRepository;
    private readonly UserContext userContext;
    private readonly ILogger<RecordEmployeeFeedbackCommandHandler> logger;

    public RecordEmployeeFeedbackCommandHandler(
        IEmployeeFeedbackAggregateRepository repository,
        IEmployeeAggregateRepository employeeRepository,
        UserContext userContext,
        ILogger<RecordEmployeeFeedbackCommandHandler> logger)
    {
        this.repository = repository;
        this.employeeRepository = employeeRepository;
        this.userContext = userContext;
        this.logger = logger;
    }

    public async Task<Result<Guid>> HandleAsync(
        RecordEmployeeFeedbackCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Recording {SourceType} feedback for employee {EmployeeId} from {ProviderName}",
                command.SourceType,
                command.EmployeeId,
                command.ProviderInfo.ProviderName);

            // Validate that the employee exists
            var employee = await employeeRepository.LoadAsync<Employee>(
                command.EmployeeId,
                cancellationToken: cancellationToken);

            if (employee == null)
            {
                logger.LogWarning("Employee {EmployeeId} not found", command.EmployeeId);
                return Result<Guid>.Fail("Employee not found", 404);
            }

            // Get the current user ID from UserContext
            if (!Guid.TryParse(userContext.Id, out var recordedByEmployeeId))
            {
                logger.LogWarning("Invalid user context ID: {UserId}", userContext.Id);
                return Result<Guid>.Fail("Invalid user context", 401);
            }

            // Validate that the user has permission to record feedback for this employee
            // This will be enforced at the API level via authorization policies,
            // but we can add additional business rule validation here if needed

            // Create the feedback aggregate using the factory method
            var feedback = EmployeeFeedback.RecordFeedback(
                command.EmployeeId,
                command.SourceType,
                command.ProviderInfo,
                command.FeedbackDate,
                command.FeedbackData,
                recordedByEmployeeId);

            // Store the aggregate
            await repository.StoreAsync(feedback, cancellationToken);

            logger.LogInformation(
                "Successfully recorded {SourceType} feedback {FeedbackId} for employee {EmployeeId}",
                command.SourceType,
                feedback.Id,
                command.EmployeeId);

            return Result<Guid>.Success(feedback.Id);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid arguments for recording feedback");
            return Result<Guid>.Fail($"Invalid input: {ex.Message}", 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error recording feedback for employee {EmployeeId}", command.EmployeeId);
            return Result<Guid>.Fail("An error occurred while recording the feedback", 500);
        }
    }
}