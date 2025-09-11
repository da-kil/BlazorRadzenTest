using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;
using ti8m.BeachBreak.CommandApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/questionnaire-templates")]
public class QuestionnaireTemplatesController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly ILogger<QuestionnaireTemplatesController> logger;

    public QuestionnaireTemplatesController(
        ICommandDispatcher commandDispatcher,
        ILogger<QuestionnaireTemplatesController> logger)
    {
        this.commandDispatcher = commandDispatcher;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTemplate(QuestionnaireTemplateDto questionnaireTemplate)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(questionnaireTemplate.Name))
                return BadRequest("Template name is required");

            Result result = await commandDispatcher.SendAsync(new CreateQuestionnaireTemplateCommand(new QuestionnaireTemplate
            {
                Category = questionnaireTemplate.Category,
                Description = questionnaireTemplate.Description,
                Name = questionnaireTemplate.Name,
                Sections = questionnaireTemplate.Sections.Select(section => new QuestionSection
                {
                    Description = section.Description,
                    Id = section.Id,
                    IsRequired = section.IsRequired,
                    Order = section.Order,
                    Questions = section.Questions.Select(question => new QuestionItem
                    {
                        Configuration = question.Configuration,
                        Description = question.Description,
                        Id = question.Id,
                        IsRequired = question.IsRequired,
                        Options = question.Options,
                        Order = question.Order,
                        Title = question.Title,
                        Type = MapQuestionType[question.Type]
                    }).ToList(),
                    Title = section.Title
                }).ToList(),
                Settings = new QuestionnaireSettings
                {
                    AllowReviewBeforeSubmit = questionnaireTemplate.Settings.AllowReviewBeforeSubmit,
                    AllowSaveProgress = questionnaireTemplate.Settings.AllowSaveProgress,
                    IncompleteMessage = questionnaireTemplate.Settings.IncompleteMessage,
                    RequireAllSections = questionnaireTemplate.Settings.RequireAllSections,
                    ShowProgressBar = questionnaireTemplate.Settings.ShowProgressBar,
                    SuccessMessage = questionnaireTemplate.Settings.SuccessMessage,
                    TimeLimit = questionnaireTemplate.Settings.TimeLimit
                }
            }));

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating questionnaire template");
            return StatusCode(500, "An error occurred while creating the template");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTemplate(Guid id, QuestionnaireTemplateDto questionnaireTemplate)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Result result = await commandDispatcher.SendAsync(new UpdateQuestionnaireTemplateCommand(id, new QuestionnaireTemplate
            {
                Category = questionnaireTemplate.Category,
                Description = questionnaireTemplate.Description,
                Name = questionnaireTemplate.Name,
                Sections = questionnaireTemplate.Sections.Select(section => new QuestionSection
                {
                    Description = section.Description,
                    Id = section.Id,
                    IsRequired = section.IsRequired,
                    Order = section.Order,
                    Questions = section.Questions.Select(question => new QuestionItem
                    {
                        Configuration = question.Configuration,
                        Description = question.Description,
                        Id = question.Id,
                        IsRequired = question.IsRequired,
                        Options = question.Options,
                        Order = question.Order,
                        Title = question.Title,
                        Type = MapQuestionType[question.Type]
                    }).ToList(),
                    Title = section.Title
                }).ToList(),
                Settings = new QuestionnaireSettings
                {
                    AllowReviewBeforeSubmit = questionnaireTemplate.Settings.AllowReviewBeforeSubmit,
                    AllowSaveProgress = questionnaireTemplate.Settings.AllowSaveProgress,
                    IncompleteMessage = questionnaireTemplate.Settings.IncompleteMessage,
                    RequireAllSections = questionnaireTemplate.Settings.RequireAllSections,
                    ShowProgressBar = questionnaireTemplate.Settings.ShowProgressBar,
                    SuccessMessage = questionnaireTemplate.Settings.SuccessMessage,
                    TimeLimit = questionnaireTemplate.Settings.TimeLimit
                }
            }));

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating template {TemplateId}", id);
            return StatusCode(500, "An error occurred while updating the template");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        try
        {
            Result result = await commandDispatcher.SendAsync(new DeleteQuestionnaireTemplateCommand(id));

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting template {TemplateId}", id);
            return StatusCode(500, "An error occurred while deleting the template");
        }
    }

    private static IReadOnlyDictionary<QuestionTypeDto, QuestionType> MapQuestionType =>
    new Dictionary<QuestionTypeDto, QuestionType>
    {
        { QuestionTypeDto.TextQuestion, QuestionType.TextQuestion },
        { QuestionTypeDto.SelfAssessment, QuestionType.SelfAssessment },
        { QuestionTypeDto.GoalAchievement, QuestionType.GoalAchievement }
    };
}