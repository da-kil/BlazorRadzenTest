using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.QueryApi.Controllers;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/questionnaire-templates")]
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
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTemplates()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new QuestionnaireTemplateListQuery());
            return CreateResponse(result, templates =>
            {
                return templates.Select(template => new QuestionnaireTemplateDto
                {

                    Id = template.Id,
                    Name = template.Name,
                    Description = template.Description,
                    Category = template.Category,
                    CreatedDate = template.CreatedDate,
                    LastModified = template.LastModified,
                    IsActive = template.IsActive,
                    IsPublished = template.IsPublished,
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
                        Questions = section.Questions.Select(question => new QuestionItemDto
                        {
                            Id = question.Id,
                            Title = question.Title,
                            Description = question.Description,
                            Type = MapQuestionTypeToDto[question.Type],
                            Order = question.Order,
                            IsRequired = question.IsRequired,
                            Configuration = question.Configuration,
                            Options = question.Options
                        }).ToList()
                    }).ToList(),
                    Settings = new QuestionnaireSettingsDto
                    {
                        AllowSaveProgress = template.Settings.AllowSaveProgress,
                        ShowProgressBar = template.Settings.ShowProgressBar,
                        RequireAllSections = template.Settings.RequireAllSections,
                        SuccessMessage = template.Settings.SuccessMessage,
                        IncompleteMessage = template.Settings.IncompleteMessage,
                        TimeLimit = template.Settings.TimeLimit,
                        AllowReviewBeforeSubmit = template.Settings.AllowReviewBeforeSubmit
                    }
                });
            });
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

            return CreateResponse(result, template => new QuestionnaireTemplateDto
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                Category = template.Category,
                CreatedDate = template.CreatedDate,
                LastModified = template.LastModified,
                IsActive = template.IsActive,
                IsPublished = template.IsPublished,
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
                    Questions = section.Questions.Select(question => new QuestionItemDto
                    {
                        Id = question.Id,
                        Title = question.Title,
                        Description = question.Description,
                        Type = MapQuestionTypeToDto[question.Type],
                        Order = question.Order,
                        IsRequired = question.IsRequired,
                        Configuration = question.Configuration,
                        Options = question.Options
                    }).ToList()
                }).ToList(),
                Settings = new QuestionnaireSettingsDto
                {
                    AllowSaveProgress = template.Settings.AllowSaveProgress,
                    ShowProgressBar = template.Settings.ShowProgressBar,
                    RequireAllSections = template.Settings.RequireAllSections,
                    SuccessMessage = template.Settings.SuccessMessage,
                    IncompleteMessage = template.Settings.IncompleteMessage,
                    TimeLimit = template.Settings.TimeLimit,
                    AllowReviewBeforeSubmit = template.Settings.AllowReviewBeforeSubmit
                }
            });
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
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublishedTemplates()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new PublishedQuestionnaireTemplatesQuery());
            return CreateResponse(result, templates =>
            {
                return templates.Select(template => new QuestionnaireTemplateDto
                {
                    Id = template.Id,
                    Name = template.Name,
                    Description = template.Description,
                    Category = template.Category,
                    CreatedDate = template.CreatedDate,
                    LastModified = template.LastModified,
                    IsActive = template.IsActive,
                    IsPublished = template.IsPublished,
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
                        Questions = section.Questions.Select(question => new QuestionItemDto
                        {
                            Id = question.Id,
                            Title = question.Title,
                            Description = question.Description,
                            Type = MapQuestionTypeToDto[question.Type],
                            Order = question.Order,
                            IsRequired = question.IsRequired,
                            Configuration = question.Configuration,
                            Options = question.Options
                        }).ToList()
                    }).ToList(),
                    Settings = new QuestionnaireSettingsDto
                    {
                        AllowSaveProgress = template.Settings.AllowSaveProgress,
                        ShowProgressBar = template.Settings.ShowProgressBar,
                        RequireAllSections = template.Settings.RequireAllSections,
                        SuccessMessage = template.Settings.SuccessMessage,
                        IncompleteMessage = template.Settings.IncompleteMessage,
                        TimeLimit = template.Settings.TimeLimit,
                        AllowReviewBeforeSubmit = template.Settings.AllowReviewBeforeSubmit
                    }
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving published questionnaire templates");
            return StatusCode(500, "An error occurred while retrieving published templates");
        }
    }

    [HttpGet("drafts")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDraftTemplates()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new DraftQuestionnaireTemplatesQuery());
            return CreateResponse(result, templates =>
            {
                return templates.Select(template => new QuestionnaireTemplateDto
                {
                    Id = template.Id,
                    Name = template.Name,
                    Description = template.Description,
                    Category = template.Category,
                    CreatedDate = template.CreatedDate,
                    LastModified = template.LastModified,
                    IsActive = template.IsActive,
                    IsPublished = template.IsPublished,
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
                        Questions = section.Questions.Select(question => new QuestionItemDto
                        {
                            Id = question.Id,
                            Title = question.Title,
                            Description = question.Description,
                            Type = MapQuestionTypeToDto[question.Type],
                            Order = question.Order,
                            IsRequired = question.IsRequired,
                            Configuration = question.Configuration,
                            Options = question.Options
                        }).ToList()
                    }).ToList(),
                    Settings = new QuestionnaireSettingsDto
                    {
                        AllowSaveProgress = template.Settings.AllowSaveProgress,
                        ShowProgressBar = template.Settings.ShowProgressBar,
                        RequireAllSections = template.Settings.RequireAllSections,
                        SuccessMessage = template.Settings.SuccessMessage,
                        IncompleteMessage = template.Settings.IncompleteMessage,
                        TimeLimit = template.Settings.TimeLimit,
                        AllowReviewBeforeSubmit = template.Settings.AllowReviewBeforeSubmit
                    }
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving draft questionnaire templates");
            return StatusCode(500, "An error occurred while retrieving draft templates");
        }
    }

    [HttpGet("assignable")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignableTemplates()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new AssignableQuestionnaireTemplatesQuery());
            return CreateResponse(result, templates =>
            {
                return templates.Select(template => new QuestionnaireTemplateDto
                {
                    Id = template.Id,
                    Name = template.Name,
                    Description = template.Description,
                    Category = template.Category,
                    CreatedDate = template.CreatedDate,
                    LastModified = template.LastModified,
                    IsActive = template.IsActive,
                    IsPublished = template.IsPublished,
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
                        Questions = section.Questions.Select(question => new QuestionItemDto
                        {
                            Id = question.Id,
                            Title = question.Title,
                            Description = question.Description,
                            Type = MapQuestionTypeToDto[question.Type],
                            Order = question.Order,
                            IsRequired = question.IsRequired,
                            Configuration = question.Configuration,
                            Options = question.Options
                        }).ToList()
                    }).ToList(),
                    Settings = new QuestionnaireSettingsDto
                    {
                        AllowSaveProgress = template.Settings.AllowSaveProgress,
                        ShowProgressBar = template.Settings.ShowProgressBar,
                        RequireAllSections = template.Settings.RequireAllSections,
                        SuccessMessage = template.Settings.SuccessMessage,
                        IncompleteMessage = template.Settings.IncompleteMessage,
                        TimeLimit = template.Settings.TimeLimit,
                        AllowReviewBeforeSubmit = template.Settings.AllowReviewBeforeSubmit
                    }
                });
            });
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

    private static IReadOnlyDictionary<Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType, QueryApi.Dto.QuestionType> MapQuestionTypeToDto =>
        new Dictionary<Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType, QueryApi.Dto.QuestionType>
        {
            { Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType.TextQuestion, QueryApi.Dto.QuestionType.TextQuestion },
            { Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType.GoalAchievement, QueryApi.Dto.QuestionType.GoalAchievement },
            { Application.Query.Queries.QuestionnaireTemplateQueries.QuestionType.SelfAssessment, QueryApi.Dto.QuestionType.SelfAssessment }
        };
}