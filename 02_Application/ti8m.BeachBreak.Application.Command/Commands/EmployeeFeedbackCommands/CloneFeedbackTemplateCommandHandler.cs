using Microsoft.AspNetCore.Http;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.FeedbackTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Handler for CloneFeedbackTemplateCommand.
/// Creates a copy of an existing template with new ownership.
/// </summary>
public class CloneFeedbackTemplateCommandHandler : ICommandHandler<CloneFeedbackTemplateCommand, Result>
{
    private readonly IFeedbackTemplateAggregateRepository feedbackTemplateRepository;
    private readonly UserContext userContext;
    private readonly IEmployeeAggregateRepository employeeRepository;

    public CloneFeedbackTemplateCommandHandler(
        IFeedbackTemplateAggregateRepository feedbackTemplateRepository,
        UserContext userContext,
        IEmployeeAggregateRepository employeeRepository)
    {
        this.feedbackTemplateRepository = feedbackTemplateRepository;
        this.userContext = userContext;
        this.employeeRepository = employeeRepository;
    }

    public async Task<Result> HandleAsync(CloneFeedbackTemplateCommand request, CancellationToken cancellationToken)
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

            // Load source template
            var sourceTemplate = await feedbackTemplateRepository.LoadAsync<FeedbackTemplate>(request.SourceTemplateId, cancellationToken: cancellationToken);

            if (sourceTemplate == null)
            {
                return Result.Fail($"Source feedback template with ID {request.SourceTemplateId} not found", StatusCodes.Status404NotFound);
            }

            // Clone using domain method (cloned template owned by current user)
            var clonedTemplate = FeedbackTemplate.CloneFrom(
                request.NewTemplateId,
                sourceTemplate,
                employeeId,
                userRole,
                request.NamePrefix);

            // Save cloned template via repository
            await feedbackTemplateRepository.StoreAsync(clonedTemplate, cancellationToken);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to clone feedback template: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}
