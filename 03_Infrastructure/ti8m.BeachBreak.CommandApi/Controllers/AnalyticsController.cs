using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.CommandApi.Services;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IQuestionnaireService _questionnaireService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IQuestionnaireService questionnaireService,
        ILogger<AnalyticsController> logger)
    {
        _questionnaireService = questionnaireService;
        _logger = logger;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<Dictionary<string, object>>> GetOverallAnalytics()
    {
        try
        {
            var analytics = await _questionnaireService.GetOverallAnalyticsAsync();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overall analytics");
            return StatusCode(500, "An error occurred while retrieving analytics");
        }
    }

    [HttpGet("template/{templateId:guid}")]
    public async Task<ActionResult<Dictionary<string, object>>> GetTemplateAnalytics(Guid templateId)
    {
        try
        {
            var analytics = await _questionnaireService.GetTemplateAnalyticsAsync(templateId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics for template {TemplateId}", templateId);
            return StatusCode(500, "An error occurred while retrieving template analytics");
        }
    }
}