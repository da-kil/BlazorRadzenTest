using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.CommandApi.Models;
using ti8m.BeachBreak.CommandApi.Services;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionnaireTemplatesController : ControllerBase
{
    private readonly IQuestionnaireService _questionnaireService;
    private readonly ILogger<QuestionnaireTemplatesController> _logger;

    public QuestionnaireTemplatesController(
        IQuestionnaireService questionnaireService,
        ILogger<QuestionnaireTemplatesController> logger)
    {
        _questionnaireService = questionnaireService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestionnaireTemplate>>> GetAllTemplates()
    {
        try
        {
            var templates = await _questionnaireService.GetAllTemplatesAsync();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving questionnaire templates");
            return StatusCode(500, "An error occurred while retrieving templates");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestionnaireTemplate>> GetTemplate(Guid id)
    {
        try
        {
            var template = await _questionnaireService.GetTemplateByIdAsync(id);
            if (template == null)
                return NotFound($"Template with ID {id} not found");

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template {TemplateId}", id);
            return StatusCode(500, "An error occurred while retrieving the template");
        }
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<List<QuestionnaireTemplate>>> GetTemplatesByCategory(string category)
    {
        try
        {
            var templates = await _questionnaireService.GetTemplatesByCategoryAsync(category);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving templates by category {Category}", category);
            return StatusCode(500, "An error occurred while retrieving templates");
        }
    }

    [HttpPost]
    public async Task<ActionResult<QuestionnaireTemplate>> CreateTemplate(CreateQuestionnaireTemplateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Template name is required");

            var template = await _questionnaireService.CreateTemplateAsync(request);
            return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating questionnaire template");
            return StatusCode(500, "An error occurred while creating the template");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<QuestionnaireTemplate>> UpdateTemplate(Guid id, UpdateQuestionnaireTemplateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.Id != id)
                return BadRequest("ID in URL does not match ID in request body");

            var template = await _questionnaireService.UpdateTemplateAsync(id, request);
            if (template == null)
                return NotFound($"Template with ID {id} not found");

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template {TemplateId}", id);
            return StatusCode(500, "An error occurred while updating the template");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        try
        {
            var success = await _questionnaireService.DeleteTemplateAsync(id);
            if (!success)
                return NotFound($"Template with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template {TemplateId}", id);
            return StatusCode(500, "An error occurred while deleting the template");
        }
    }

    [HttpGet("{id:guid}/analytics")]
    public async Task<ActionResult<Dictionary<string, object>>> GetTemplateAnalytics(Guid id)
    {
        try
        {
            var template = await _questionnaireService.GetTemplateByIdAsync(id);
            if (template == null)
                return NotFound($"Template with ID {id} not found");

            var analytics = await _questionnaireService.GetTemplateAnalyticsAsync(id);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics for template {TemplateId}", id);
            return StatusCode(500, "An error occurred while retrieving analytics");
        }
    }
}