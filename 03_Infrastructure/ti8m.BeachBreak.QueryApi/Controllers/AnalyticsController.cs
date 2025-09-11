using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.AnalyticsQueries;
using ti8m.BeachBreak.QueryApi.Controllers;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/analytics")]
public class AnalyticsController : BaseController
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
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverallAnalytics()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new OverallAnalyticsListQuery());
            return CreateResponse(result, templates => 
            {
                return result;
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving overall analytics");
            return StatusCode(500, "An error occurred while retrieving analytics");
        }
    }

    [HttpGet("template/{templateId:guid}")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplateAnalytics(Guid templateId)
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new TemplateAnalyticsListQuery(templateId));
            return CreateResponse(result, templates =>
            {
                return result;
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving analytics for template {TemplateId}", templateId);
            return StatusCode(500, "An error occurred while retrieving template analytics");
        }
    }
}