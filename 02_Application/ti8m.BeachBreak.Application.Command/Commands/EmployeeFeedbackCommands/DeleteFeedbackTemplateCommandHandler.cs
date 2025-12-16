using Microsoft.AspNetCore.Http;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.FeedbackTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Handler for DeleteFeedbackTemplateCommand.
/// Soft deletes a feedback template with ownership validation.
/// </summary>
public class DeleteFeedbackTemplateCommandHandler : ICommandHandler<DeleteFeedbackTemplateCommand, Result>
{
    private readonly IFeedbackTemplateAggregateRepository feedbackTemplateRepository;
    private readonly UserContext userContext;
    private readonly IEmployeeAggregateRepository employeeRepository;

    public DeleteFeedbackTemplateCommandHandler(
        IFeedbackTemplateAggregateRepository feedbackTemplateRepository,
        UserContext userContext,
        IEmployeeAggregateRepository employeeRepository)
    {
        this.feedbackTemplateRepository = feedbackTemplateRepository;
        this.userContext = userContext;
        this.employeeRepository = employeeRepository;
    }

    public async Task<Result> HandleAsync(DeleteFeedbackTemplateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user's employee ID and role
            var employeeId = Guid.Parse(userContext.Id);
            var employee = await employeeRepository.LoadAsync<Employee>(employeeId, cancellationToken: cancellationToken);

            if (employee == null)
            {
                return Result.Fail("Employee not found", StatusCodes.Status404NotFound);
            }

            var userRole = employee.ApplicationRole;

            // Load aggregate
            var template = await feedbackTemplateRepository.LoadAsync<FeedbackTemplate>(request.TemplateId, cancellationToken: cancellationToken);

            if (template == null)
            {
                return Result.Fail($"Feedback template with ID {request.TemplateId} not found", StatusCodes.Status404NotFound);
            }

            // Call domain method (includes ownership validation)
            template.Delete(employeeId, userRole);

            // Save via repository
            await feedbackTemplateRepository.StoreAsync(template, cancellationToken);

            return Result.Success();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Result.Fail(ex.Message, StatusCodes.Status403Forbidden);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete feedback template: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}
