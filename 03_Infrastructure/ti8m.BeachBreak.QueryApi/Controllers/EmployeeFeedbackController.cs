using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.QueryApi.Dto;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeFeedbackQueries;
using ti8m.BeachBreak.Application.Query.Projections;

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

        return Ok(groupedFeedback);
    }

    /// <summary>
    /// Gets available feedback templates and criteria.
    /// Used by frontend to populate criteria selection UI.
    /// </summary>
    /// <param name="sourceType">Filter by specific source type</param>
    /// <returns>Available templates and criteria</returns>
    [HttpGet("templates")]
    public async Task<IActionResult> GetFeedbackTemplates([FromQuery] int? sourceType = null)
    {
        var query = new GetFeedbackTemplatesQuery(sourceType);

        var result = await queryDispatcher.QueryAsync(query);

        return CreateResponse(result);
    }

    /// <summary>
    /// Gets feedback statistics for an employee.
    /// Provides summary metrics for dashboard display.
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="fromDate">Statistics from date</param>
    /// <param name="toDate">Statistics to date</param>
    /// <returns>Aggregated feedback statistics</returns>
    [HttpGet("employee/{employeeId}/statistics")]
    public async Task<IActionResult> GetFeedbackStatistics(
        Guid employeeId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = new GetEmployeeFeedbackQuery
        {
            EmployeeId = employeeId,
            FromDate = fromDate,
            ToDate = toDate,
            IncludeDeleted = false,
            PageSize = 1000 // Get all records for statistics
        };

        var result = await queryDispatcher.QueryAsync(query);

        if (!result.Succeeded)
            return CreateResponse(result);

        var feedback = result.Payload;
        var statistics = new
        {
            TotalFeedbackCount = feedback.Count,
            BySourceType = feedback.GroupBy(f => f.SourceType.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            AverageRating = feedback.Where(f => f.AverageRating.HasValue).Select(f => f.AverageRating!.Value).Cast<decimal?>().Average(),
            MostRecentFeedback = feedback.Max(f => (DateTime?)f.FeedbackDate),
            FeedbackWithComments = feedback.Count(f => f.HasUnstructuredFeedback),
            ProjectFeedbackCount = feedback.Count(f => f.SourceType == Domain.EmployeeFeedbackAggregate.FeedbackSourceType.ProjectColleague)
        };

        return Ok(statistics);
    }
}