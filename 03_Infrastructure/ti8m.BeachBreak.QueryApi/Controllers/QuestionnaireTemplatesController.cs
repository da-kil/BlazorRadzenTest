using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionnaireTemplatesController : ControllerBase
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<QuestionnaireTemplatesController> logger;

    public QuestionnaireTemplatesController(
        IQueryDispatcher queryDispatcher,
        ILogger<QuestionnaireTemplatesController> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestionnaireTemplateDto>>> GetAllTemplates()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new QuestionnaireTemplateListQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving questionnaire templates");
            return StatusCode(500, "An error occurred while retrieving templates");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestionnaireTemplateDto>> GetTemplate(Guid id)
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new QuestionnaireTemplateQuery(id));
            if (result == null)
                return NotFound($"Template with ID {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving template {TemplateId}", id);
            return StatusCode(500, "An error occurred while retrieving the template");
        }
    }

    //[HttpGet("category/{category}")]
    //public async Task<ActionResult<List<QuestionnaireTemplateDto>>> GetTemplatesByCategory(string category)
    //{
    //    try
    //    {
    //        var templates = await _questionnaireService.GetTemplatesByCategoryAsync(category);
    //        return Ok(templates);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error retrieving templates by category {Category}", category);
    //        return StatusCode(500, "An error occurred while retrieving templates");
    //    }
    //}

    //[HttpGet("{id:guid}/analytics")]
    //public async Task<ActionResult<Dictionary<string, object>>> GetTemplateAnalytics(Guid id)
    //{
    //    try
    //    {
    //        var template = await _questionnaireService.GetTemplateByIdAsync(id);
    //        if (template == null)
    //            return NotFound($"Template with ID {id} not found");

    //        var analytics = await _questionnaireService.GetTemplateAnalyticsAsync(id);
    //        return Ok(analytics);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error retrieving analytics for template {TemplateId}", id);
    //        return StatusCode(500, "An error occurred while retrieving analytics");
    //    }
    //}
}