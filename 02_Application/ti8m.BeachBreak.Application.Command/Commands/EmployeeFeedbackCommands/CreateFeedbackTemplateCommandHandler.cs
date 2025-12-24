using Microsoft.AspNetCore.Http;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.FeedbackTemplateAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;

/// <summary>
/// Handler for CreateFeedbackTemplateCommand.
/// Creates a new feedback template using event sourcing.
/// </summary>
public class CreateFeedbackTemplateCommandHandler : ICommandHandler<CreateFeedbackTemplateCommand, Result>
{
    private readonly IFeedbackTemplateAggregateRepository feedbackTemplateRepository;
    private readonly UserContext userContext;
    private readonly IEmployeeAggregateRepository employeeRepository;

    public CreateFeedbackTemplateCommandHandler(
        IFeedbackTemplateAggregateRepository feedbackTemplateRepository,
        UserContext userContext,
        IEmployeeAggregateRepository employeeRepository)
    {
        this.feedbackTemplateRepository = feedbackTemplateRepository;
        this.userContext = userContext;
        this.employeeRepository = employeeRepository;
    }

    public async Task<Result> HandleAsync(CreateFeedbackTemplateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user's employee ID and role - follows EmployeeCommandHandler pattern exactly
            var employeeId = Guid.TryParse(userContext.Id, out var parsedUserId) ? parsedUserId : Guid.Empty;

            ApplicationRole userRole;
            if (employeeId != Guid.Empty)
            {
                var employee = await employeeRepository.LoadAsync<Employee>(employeeId, cancellationToken: cancellationToken);
                if (employee != null)
                {
                    userRole = employee.ApplicationRole;
                }
                else
                {
                    // Employee not found - default to Admin for service principals
                    userRole = ApplicationRole.Admin;
                }
            }
            else
            {
                // Service principal operation (no valid employee ID) - default to Admin
                userRole = ApplicationRole.Admin;
            }

            // Validate user has permission (TeamLead or higher)
            if (userRole < ApplicationRole.TeamLead)
            {
                return Result.Fail("Only HR and TeamLead users can create feedback templates", StatusCodes.Status403Forbidden);
            }

            // Create aggregate using domain logic (all validation happens in aggregate constructor)
            var template = new FeedbackTemplate(
                request.Id,
                request.Name,
                request.Description,
                request.Criteria,
                request.TextSections,
                request.RatingScale,
                request.ScaleLowLabel,
                request.ScaleHighLabel,
                request.AllowedSourceTypes,
                employeeId,
                userRole);

            // Save via repository (persists events)
            await feedbackTemplateRepository.StoreAsync(template, cancellationToken);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            // Domain validation errors
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create feedback template: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}
