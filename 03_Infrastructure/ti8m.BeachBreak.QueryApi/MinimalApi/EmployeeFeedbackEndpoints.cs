using Microsoft.AspNetCore.Authorization;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeFeedbackQueries;
using ti8m.BeachBreak.Application.Query.Queries.FeedbackTemplateQueries;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for employee feedback queries.
/// </summary>
public static class EmployeeFeedbackEndpoints
{
    /// <summary>
    /// Maps employee feedback query endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapEmployeeFeedbackEndpoints(this WebApplication app)
    {
        var feedbackGroup = app.MapGroup("/q/api/v{version:apiVersion}/employee-feedbacks")
            .WithTags("Employee Feedback")
            .RequireAuthorization("TeamLead"); // TeamLead, HR, and Admin can access feedback queries

        // Get employee feedback with filtering
        feedbackGroup.MapGet("/", async (
            IQueryDispatcher queryDispatcher,
            [AsParameters] FeedbackQueryParams parameters,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetEmployeeFeedbackQuery
            {
                EmployeeId = parameters.EmployeeId,
                SourceType = parameters.SourceType.HasValue ? (Domain.EmployeeFeedbackAggregate.FeedbackSourceType)parameters.SourceType.Value : null,
                FromDate = parameters.FromDate,
                ToDate = parameters.ToDate,
                ProviderName = parameters.ProviderName,
                ProjectName = parameters.ProjectName,
                IncludeDeleted = parameters.IncludeDeleted,
                CurrentFiscalYearOnly = parameters.CurrentFiscalYearOnly,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                SortField = parameters.SortField,
                SortAscending = parameters.SortAscending
            };

            var result = await queryDispatcher.QueryAsync(query, cancellationToken);

            if (!result.Succeeded)
            {
                return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
            }

            var dtos = result.Payload?.Select(EmployeeFeedbackSummaryDto.FromReadModel).ToList() ?? new List<EmployeeFeedbackSummaryDto>();
            return Results.Ok(dtos);
        })
        .WithName("GetEmployeeFeedback")
        .WithSummary("Get employee feedback")
        .WithDescription("Gets employee feedback with filtering and pagination")
        .Produces<List<EmployeeFeedbackSummaryDto>>(200)
        .Produces(500);

        // Get feedback by ID
        feedbackGroup.MapGet("/{id:guid}", async (
            Guid id,
            IQueryDispatcher queryDispatcher,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetFeedbackByIdQuery(id)
            {
                IncludeDeleted = includeDeleted
            };

            var result = await queryDispatcher.QueryAsync(query, cancellationToken);

            if (!result.Succeeded)
            {
                return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
            }

            // Convert ReadModel to DTO for consistent API response format
            var dto = EmployeeFeedbackSummaryDto.FromReadModel(result.Payload!);
            return Results.Ok(dto);
        })
        .WithName("GetFeedbackById")
        .WithSummary("Get feedback by ID")
        .WithDescription("Gets a specific feedback record by ID")
        .Produces<EmployeeFeedbackSummaryDto>(200)
        .Produces(500);

        // Get current year feedback for employee
        feedbackGroup.MapGet("/employee/{employeeId:guid}/current-year", async (
            Guid employeeId,
            IQueryDispatcher queryDispatcher,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetCurrentYearFeedbackQuery(employeeId);

            var result = await queryDispatcher.QueryAsync(query, cancellationToken);

            if (!result.Succeeded)
            {
                return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
            }

            var dtos = result.Payload.Select(EmployeeFeedbackSummaryDto.FromReadModel).ToList();

            // Group by source type for better organization in reviews
            var groupedFeedback = dtos
                .GroupBy(f => f.SourceType)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(f => f.FeedbackDate).ToList()
                );

            return Results.Ok(groupedFeedback);
        })
        .WithName("GetCurrentYearFeedback")
        .WithSummary("Get current year feedback")
        .WithDescription("Gets current fiscal year feedback for a specific employee - used for questionnaire review integration")
        .Produces<Dictionary<int, List<EmployeeFeedbackSummaryDto>>>(200)
        .Produces(500);

        // Get all feedback templates
        feedbackGroup.MapGet("/templates", async (
            IQueryDispatcher queryDispatcher,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetAllFeedbackTemplatesQuery();
            var templates = await queryDispatcher.QueryAsync(query, cancellationToken);
            var dtos = templates.Select(FeedbackTemplateDto.FromReadModel).ToList();
            return Results.Ok(dtos);
        })
        .WithName("GetAllFeedbackTemplates")
        .WithSummary("Get all feedback templates")
        .WithDescription("Gets all feedback templates (non-deleted)")
        .Produces<List<FeedbackTemplateDto>>(200);

        // Get feedback template by ID
        feedbackGroup.MapGet("/templates/{id:guid}", async (
            Guid id,
            IQueryDispatcher queryDispatcher,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetFeedbackTemplateByIdQuery(id);
            var template = await queryDispatcher.QueryAsync(query, cancellationToken);

            if (template == null)
            {
                return Results.NotFound($"Feedback template with ID {id} not found");
            }

            var dto = FeedbackTemplateDto.FromReadModel(template);
            return Results.Ok(dto);
        })
        .WithName("GetFeedbackTemplateById")
        .WithSummary("Get feedback template by ID")
        .WithDescription("Gets a specific feedback template by ID")
        .Produces<FeedbackTemplateDto>(200)
        .Produces(404);

        // Get feedback templates by source type
        feedbackGroup.MapGet("/templates/by-source/{sourceType:int}", async (
            int sourceType,
            IQueryDispatcher queryDispatcher,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetFeedbackTemplatesBySourceTypeQuery(
                (Domain.EmployeeFeedbackAggregate.FeedbackSourceType)sourceType);
            var templates = await queryDispatcher.QueryAsync(query, cancellationToken);
            var dtos = templates.Select(FeedbackTemplateDto.FromReadModel).ToList();
            return Results.Ok(dtos);
        })
        .WithName("GetFeedbackTemplatesBySourceType")
        .WithSummary("Get feedback templates by source type")
        .WithDescription("Gets feedback templates filtered by source type (0=Customer, 1=Peer, 2=ProjectColleague)")
        .Produces<List<FeedbackTemplateDto>>(200);

        // Get source type options
        feedbackGroup.MapGet("/source-types", async (
            IQueryDispatcher queryDispatcher,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetFeedbackTemplatesQuery();
            var result = await queryDispatcher.QueryAsync(query, cancellationToken);

            if (result.Succeeded)
            {
                return Results.Ok(result.Payload);
            }
            else
            {
                return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
            }
        })
        .WithName("GetSourceTypeOptions")
        .WithSummary("Get source type options")
        .WithDescription("Gets source type options for feedback recording - returns metadata about available source types")
        .Produces(200)
        .Produces(500);
    }
}