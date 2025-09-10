using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.AnalyticsQueries;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<AnalyticsController> logger;

    public AnalyticsController(
        IQueryDispatcher queryDispatcher,
        ILogger<AnalyticsController> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<Dictionary<string, object>>> GetOverallAnalytics()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new OverallAnalyticsListQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving overall analytics");
            return StatusCode(500, "An error occurred while retrieving analytics");
        }
    }

    [HttpGet("template/{templateId:guid}")]
    public async Task<ActionResult<Dictionary<string, object>>> GetTemplateAnalytics(Guid templateId)
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new TemplateAnalyticsListQuery(templateId));
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving analytics for template {TemplateId}", templateId);
            return StatusCode(500, "An error occurred while retrieving template analytics");
        }
    }
}