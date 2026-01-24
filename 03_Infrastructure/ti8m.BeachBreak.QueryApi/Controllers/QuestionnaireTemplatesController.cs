using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.QueryApi.Controllers;
using ti8m.BeachBreak.QueryApi.Dto;
using ti8m.BeachBreak.QueryApi.Mappers;

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
    [Authorize(Policy = "HR")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTemplates()
    {
        var result = await queryDispatcher.QueryAsync(new QuestionnaireTemplateListQuery());
        return CreateResponse(result, templates => templates.Select(MapToDto));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuestionnaireTemplateDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplate(Guid id)
    {
        var result = await queryDispatcher.QueryAsync(new QuestionnaireTemplateQuery(id));
        if (result == null)
            return CreateResponse(Result<QuestionnaireTemplateDto>.Fail($"Template with ID {id} not found", 404));

        return CreateResponse(result, MapToDto);
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
    [Authorize(Policy = "HR")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublishedTemplates()
    {
        var result = await queryDispatcher.QueryAsync(new PublishedQuestionnaireTemplatesQuery());
        return CreateResponse(result, templates => templates.Select(MapToDto));
    }

    [HttpGet("drafts")]
    [Authorize(Policy = "HR")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDraftTemplates()
    {
        var result = await queryDispatcher.QueryAsync(new DraftQuestionnaireTemplatesQuery());
        return CreateResponse(result, templates => templates.Select(MapToDto));
    }

    [HttpGet("archived")]
    [Authorize(Policy = "HR")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetArchivedTemplates()
    {
        var result = await queryDispatcher.QueryAsync(new ArchivedQuestionnaireTemplatesQuery());
        return CreateResponse(result, templates => templates.Select(MapToDto));
    }

    [HttpGet("assignable")]
    [Authorize(Policy = "HR")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignableTemplates()
    {
        var result = await queryDispatcher.QueryAsync(new AssignableQuestionnaireTemplatesQuery());
        return CreateResponse(result, templates => templates.Select(MapToDto));
    }


    private static QuestionnaireTemplateDto MapToDto(Application.Query.Queries.QuestionnaireTemplateQueries.QuestionnaireTemplate template)
    {
        return new QuestionnaireTemplateDto
        {
            Id = template.Id,
            NameGerman = template.NameGerman,
            NameEnglish = template.NameEnglish,
            DescriptionGerman = template.DescriptionGerman,
            DescriptionEnglish = template.DescriptionEnglish,
            CategoryId = template.CategoryId,
            ProcessType = EnumConverter.MapToProcessType(template.ProcessType),
            IsCustomizable = template.IsCustomizable,
            AutoInitialize = template.AutoInitialize,
            CreatedDate = template.CreatedDate,
            Status = MapToStatusDto(template.Status),
            PublishedDate = template.PublishedDate,
            LastPublishedDate = template.LastPublishedDate,
            PublishedByEmployeeId = template.PublishedByEmployeeId,
            PublishedByEmployeeName = template.PublishedByEmployeeName,
            Sections = template.Sections.Select(section => new QuestionSectionDto
            {
                Id = section.Id,
                TitleGerman = section.TitleGerman,
                TitleEnglish = section.TitleEnglish,
                DescriptionGerman = section.DescriptionGerman,
                DescriptionEnglish = section.DescriptionEnglish,
                Order = section.Order,
                CompletionRole = EnumConverter.MapToCompletionRole(section.CompletionRole),
                Type = EnumConverter.MapToQuestionType(section.Type),
                Configuration = section.Configuration
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

    private static IReadOnlyDictionary<Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType, QueryApi.Dto.QuestionType> MapQuestionTypeToDto =>
        new Dictionary<Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType, QueryApi.Dto.QuestionType>
        {
            { Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType.TextQuestion, QueryApi.Dto.QuestionType.TextQuestion },
            { Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType.Goal, QueryApi.Dto.QuestionType.Goal },
            { Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType.Assessment, QueryApi.Dto.QuestionType.Assessment }
        };
}