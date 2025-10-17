using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.QueryApi.Controllers;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/questionnaire-templates")]
[Authorize] // All endpoints require authentication - all roles can view templates/responses
public class QuestionnaireTemplatesController : BaseController
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
    [Authorize(Roles = "HR")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTemplates()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new QuestionnaireTemplateListQuery());
            return CreateResponse(result, templates => templates.Select(MapToDto));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving questionnaire templates");
            return StatusCode(500, "An error occurred while retrieving templates");
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuestionnaireTemplateDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplate(Guid id)
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new QuestionnaireTemplateQuery(id));
            if (result == null)
                return NotFound($"Template with ID {id} not found");

            return CreateResponse(result, MapToDto);
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

    [HttpGet("published")]
    [Authorize(Roles = "HR")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublishedTemplates()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new PublishedQuestionnaireTemplatesQuery());
            return CreateResponse(result, templates => templates.Select(MapToDto));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving published questionnaire templates");
            return StatusCode(500, "An error occurred while retrieving published templates");
        }
    }

    [HttpGet("drafts")]
    [Authorize(Roles = "HR")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDraftTemplates()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new DraftQuestionnaireTemplatesQuery());
            return CreateResponse(result, templates => templates.Select(MapToDto));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving draft questionnaire templates");
            return StatusCode(500, "An error occurred while retrieving draft templates");
        }
    }

    [HttpGet("archived")]
    [Authorize(Roles = "HR")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetArchivedTemplates()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new ArchivedQuestionnaireTemplatesQuery());
            return CreateResponse(result, templates => templates.Select(MapToDto));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving archived questionnaire templates");
            return StatusCode(500, "An error occurred while retrieving archived templates");
        }
    }

    [HttpGet("assignable")]
    [Authorize(Roles = "HR")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignableTemplates()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new AssignableQuestionnaireTemplatesQuery());
            return CreateResponse(result, templates => templates.Select(MapToDto));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignable questionnaire templates");
            return StatusCode(500, "An error occurred while retrieving assignable templates");
        }
    }

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

    private static QuestionnaireTemplateDto MapToDto(Application.Query.Queries.QuestionnaireTemplateQueries.QuestionnaireTemplate template)
    {
        return new QuestionnaireTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            CategoryId = template.CategoryId,
            RequiresManagerReview = template.RequiresManagerReview,
            CreatedDate = template.CreatedDate,
            Status = MapToStatusDto(template.Status),
            PublishedDate = template.PublishedDate,
            LastPublishedDate = template.LastPublishedDate,
            PublishedBy = template.PublishedBy,
            Sections = template.Sections.Select(section => new QuestionSectionDto
            {
                Id = section.Id,
                Title = section.Title,
                Description = section.Description,
                Order = section.Order,
                IsRequired = section.IsRequired,
                CompletionRole = MapToCompletionRoleEnum(section.CompletionRole),
                Questions = section.Questions.Select(question => new QuestionItemDto
                {
                    Id = question.Id,
                    Title = question.Title,
                    Description = question.Description,
                    Type = MapQuestionTypeToDto[question.Type],
                    Order = question.Order,
                    IsRequired = question.IsRequired,
                    Configuration = question.Configuration
                }).ToList()
            }).ToList()
        };
    }

    private static QueryApi.Dto.TemplateStatus MapToStatusDto(Application.Query.Queries.QuestionnaireTemplateQueries.TemplateStatus queryStatus)
    {
        return queryStatus switch
        {
            Application.Query.Queries.QuestionnaireTemplateQueries.TemplateStatus.Draft => QueryApi.Dto.TemplateStatus.Draft,
            Application.Query.Queries.QuestionnaireTemplateQueries.TemplateStatus.Published => QueryApi.Dto.TemplateStatus.Published,
            Application.Query.Queries.QuestionnaireTemplateQueries.TemplateStatus.Archived => QueryApi.Dto.TemplateStatus.Archived,
            _ => QueryApi.Dto.TemplateStatus.Draft
        };
    }

    private static Domain.QuestionnaireTemplateAggregate.CompletionRole MapToCompletionRoleEnum(string completionRole)
    {
        return completionRole?.ToLower() switch
        {
            "manager" => Domain.QuestionnaireTemplateAggregate.CompletionRole.Manager,
            "both" => Domain.QuestionnaireTemplateAggregate.CompletionRole.Both,
            _ => Domain.QuestionnaireTemplateAggregate.CompletionRole.Employee
        };
    }

    private static IReadOnlyDictionary<Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType, QueryApi.Dto.QuestionType> MapQuestionTypeToDto =>
        new Dictionary<Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType, QueryApi.Dto.QuestionType>
        {
            { Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType.TextQuestion, QueryApi.Dto.QuestionType.TextQuestion },
            { Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType.GoalAchievement, QueryApi.Dto.QuestionType.GoalAchievement },
            { Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType.Assessment, QueryApi.Dto.QuestionType.Assessment }
        };
}