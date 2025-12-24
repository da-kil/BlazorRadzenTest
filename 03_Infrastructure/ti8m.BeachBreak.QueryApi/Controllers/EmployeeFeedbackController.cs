using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeFeedbackQueries;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.Controllers;

/// <summary>
/// API controller for employee feedback query operations.
/// Supports retrieving feedback with filtering, pagination, and source type categorization.
/// </summary>
[ApiController]
[Route("q/api/v{version:apiVersion}/employee-feedbacks")]
[Authorize(Policy = "TeamLead")] // TeamLead, HR, and Admin can access feedback queries
public class EmployeeFeedbackController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;

    public EmployeeFeedbackController(IQueryDispatcher queryDispatcher)
    {
        this.queryDispatcher = queryDispatcher;
    }

    /// <summary>
    /// Gets employee feedback with filtering and pagination.
    /// </summary>
    /// <param name="parameters">Query parameters for filtering</param>
    /// <returns>List of feedback summaries matching the criteria</returns>
    [HttpGet]
    public async Task<IActionResult> GetEmployeeFeedback([FromQuery] FeedbackQueryParams parameters)
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

        var result = await queryDispatcher.QueryAsync(query);

        if (!result.Succeeded)
            return CreateResponse(result);

        var dtos = result.Payload?.Select(EmployeeFeedbackSummaryDto.FromReadModel).ToList() ?? new List<EmployeeFeedbackSummaryDto>();

        return CreateResponse(Result<List<EmployeeFeedbackSummaryDto>>.Success(dtos));
    }

    /// <summary>
    /// Gets a specific feedback record by ID.
    /// </summary>
    /// <param name="id">Feedback ID</param>
    /// <param name="includeDeleted">Whether to include deleted feedback</param>
    /// <returns>Detailed feedback information</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFeedbackById(Guid id, [FromQuery] bool includeDeleted = false)
    {
        var query = new GetFeedbackByIdQuery(id)
        {
            IncludeDeleted = includeDeleted
        };

        var result = await queryDispatcher.QueryAsync(query);

        if (!result.Succeeded)
            return CreateResponse(result);

        // Convert ReadModel to DTO for consistent API response format
        var dto = EmployeeFeedbackSummaryDto.FromReadModel(result.Payload!);
        return CreateResponse(Result<EmployeeFeedbackSummaryDto>.Success(dto));
    }

    /// <summary>
    /// Gets current fiscal year feedback for a specific employee.
    /// Used for questionnaire review integration.
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Current year feedback grouped by source type</returns>
    [HttpGet("employee/{employeeId}/current-year")]
    public async Task<IActionResult> GetCurrentYearFeedback(Guid employeeId)
    {
        var query = new GetCurrentYearFeedbackQuery(employeeId);

        var result = await queryDispatcher.QueryAsync(query);

        if (!result.Succeeded)
            return CreateResponse(result);

        var dtos = result.Payload.Select(EmployeeFeedbackSummaryDto.FromReadModel).ToList();

        // Group by source type for better organization in reviews
        var groupedFeedback = dtos
            .GroupBy(f => f.SourceType)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(f => f.FeedbackDate).ToList()
            );

        return CreateResponse(Result<Dictionary<int, List<EmployeeFeedbackSummaryDto>>>.Success(groupedFeedback));
    }

    /// <summary>
    /// Gets all feedback templates (non-deleted).
    /// </summary>
    /// <returns>List of all feedback templates</returns>
    [HttpGet("templates")]
    public async Task<IActionResult> GetAllFeedbackTemplates()
    {
        var query = new Application.Query.Queries.FeedbackTemplateQueries.GetAllFeedbackTemplatesQuery();
        var templates = await queryDispatcher.QueryAsync(query);
        var dtos = templates.Select(FeedbackTemplateDto.FromReadModel).ToList();
        return CreateResponse(Result<List<FeedbackTemplateDto>>.Success(dtos));
    }

    /// <summary>
    /// Gets a specific feedback template by ID.
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <returns>Feedback template details</returns>
    [HttpGet("templates/{id}")]
    public async Task<IActionResult> GetFeedbackTemplateById(Guid id)
    {
        var query = new Application.Query.Queries.FeedbackTemplateQueries.GetFeedbackTemplateByIdQuery(id);
        var template = await queryDispatcher.QueryAsync(query);

        if (template == null)
            return CreateResponse(Result.Fail($"Feedback template with ID {id} not found", Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound));

        var dto = FeedbackTemplateDto.FromReadModel(template);
        return CreateResponse(Result<FeedbackTemplateDto>.Success(dto));
    }

    /// <summary>
    /// Gets feedback templates filtered by source type.
    /// </summary>
    /// <param name="sourceType">Feedback source type (0=Customer, 1=Peer, 2=ProjectColleague)</param>
    /// <returns>List of templates for the specified source type</returns>
    [HttpGet("templates/by-source/{sourceType}")]
    public async Task<IActionResult> GetFeedbackTemplatesBySourceType(int sourceType)
    {
        var query = new Application.Query.Queries.FeedbackTemplateQueries.GetFeedbackTemplatesBySourceTypeQuery(
            (Domain.EmployeeFeedbackAggregate.FeedbackSourceType)sourceType);
        var templates = await queryDispatcher.QueryAsync(query);
        var dtos = templates.Select(FeedbackTemplateDto.FromReadModel).ToList();
        return CreateResponse(Result<List<FeedbackTemplateDto>>.Success(dtos));
    }

    /// <summary>
    /// Gets source type options for feedback recording.
    /// Returns metadata about available source types (Customer, Peer, ProjectColleague).
    /// </summary>
    /// <returns>Source type options with validation requirements</returns>
    [HttpGet("source-types")]
    public async Task<IActionResult> GetSourceTypeOptions()
    {
        var query = new GetFeedbackTemplatesQuery();
        var result = await queryDispatcher.QueryAsync(query);
        return CreateResponse(result);
    }

}