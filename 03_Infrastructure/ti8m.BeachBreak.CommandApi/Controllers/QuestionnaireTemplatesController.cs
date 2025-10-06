using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;
using ti8m.BeachBreak.CommandApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/questionnaire-templates")]
[Authorize(Roles = "HR")]
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
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTemplate(QuestionnaireTemplateDto questionnaireTemplate)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(questionnaireTemplate.Name))
                return BadRequest("Template name is required");

            var commandTemplate = new CommandQuestionnaireTemplate
            {
                Id = Guid.NewGuid(),
                CategoryId = questionnaireTemplate.CategoryId,
                Description = questionnaireTemplate.Description,
                Name = questionnaireTemplate.Name,
                Sections = questionnaireTemplate.Sections.Select(section => new CommandQuestionSection
                {
                    Description = section.Description,
                    Id = section.Id,
                    IsRequired = section.IsRequired,
                    Order = section.Order,
                    Title = section.Title,
                    Questions = section.Questions.Select(question => new CommandQuestionItem
                    {
                        Configuration = question.Configuration,
                        Description = question.Description,
                        Id = question.Id,
                        IsRequired = question.IsRequired,
                        Options = question.Options,
                        Order = question.Order,
                        Title = question.Title,
                        Type = MapQuestionType(question.Type)
                    }).ToList()
                }).ToList(),
                Settings = new CommandQuestionnaireSettings
                {
                    AllowReviewBeforeSubmit = questionnaireTemplate.Settings.AllowReviewBeforeSubmit,
                    AllowSaveProgress = questionnaireTemplate.Settings.AllowSaveProgress,
                    IncompleteMessage = questionnaireTemplate.Settings.IncompleteMessage,
                    RequireAllSections = questionnaireTemplate.Settings.RequireAllSections,
                    ShowProgressBar = questionnaireTemplate.Settings.ShowProgressBar,
                    SuccessMessage = questionnaireTemplate.Settings.SuccessMessage,
                    TimeLimit = questionnaireTemplate.Settings.TimeLimit
                }
            };

            Result result = await commandDispatcher.SendAsync(new CreateQuestionnaireTemplateCommand(commandTemplate));

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating questionnaire template");
            return StatusCode(500, "An error occurred while creating the template");
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTemplate(Guid id, QuestionnaireTemplateDto questionnaireTemplate)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var commandTemplate = new CommandQuestionnaireTemplate
            {
                Id = id,
                CategoryId = questionnaireTemplate.CategoryId,
                Description = questionnaireTemplate.Description,
                Name = questionnaireTemplate.Name,
                Sections = questionnaireTemplate.Sections.Select(section => new CommandQuestionSection
                {
                    Description = section.Description,
                    Id = section.Id,
                    IsRequired = section.IsRequired,
                    Order = section.Order,
                    Title = section.Title,
                    Questions = section.Questions.Select(question => new CommandQuestionItem
                    {
                        Configuration = question.Configuration,
                        Description = question.Description,
                        Id = question.Id,
                        IsRequired = question.IsRequired,
                        Options = question.Options,
                        Order = question.Order,
                        Title = question.Title,
                        Type = MapQuestionType(question.Type)
                    }).ToList()
                }).ToList(),
                Settings = new CommandQuestionnaireSettings
                {
                    AllowReviewBeforeSubmit = questionnaireTemplate.Settings.AllowReviewBeforeSubmit,
                    AllowSaveProgress = questionnaireTemplate.Settings.AllowSaveProgress,
                    IncompleteMessage = questionnaireTemplate.Settings.IncompleteMessage,
                    RequireAllSections = questionnaireTemplate.Settings.RequireAllSections,
                    ShowProgressBar = questionnaireTemplate.Settings.ShowProgressBar,
                    SuccessMessage = questionnaireTemplate.Settings.SuccessMessage,
                    TimeLimit = questionnaireTemplate.Settings.TimeLimit
                }
            };

            Result result = await commandDispatcher.SendAsync(new UpdateQuestionnaireTemplateCommand(id, commandTemplate));

            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return Problem(detail: result.Message, statusCode: result.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating template {TemplateId}", id);
            return StatusCode(500, "An error occurred while updating the template");
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> PublishTemplate(Guid id, [FromBody] string publishedBy)
    {
        try
        {
            //Todo: replace
            if (string.IsNullOrWhiteSpace(publishedBy))
            {
                publishedBy = "Test";
            }

            Result result = await commandDispatcher.SendAsync(new PublishQuestionnaireTemplateCommand(id, publishedBy));

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing template {TemplateId}", id);
            return StatusCode(500, "An error occurred while publishing the template");
        }
    }

    [HttpPost("{id:guid}/unpublish")]
    public async Task<IActionResult> UnpublishTemplate(Guid id)
    {
        try
        {
            Result result = await commandDispatcher.SendAsync(new UnpublishQuestionnaireTemplateCommand(id));

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error unpublishing template {TemplateId}", id);
            return StatusCode(500, "An error occurred while unpublishing the template");
        }
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> ActivateTemplate(Guid id)
    {
        try
        {
            Result result = await commandDispatcher.SendAsync(new ActivateQuestionnaireTemplateCommand(id));

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error activating template {TemplateId}", id);
            return StatusCode(500, "An error occurred while activating the template");
        }
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateTemplate(Guid id)
    {
        try
        {
            Result result = await commandDispatcher.SendAsync(new DeactivateQuestionnaireTemplateCommand(id));

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating template {TemplateId}", id);
            return StatusCode(500, "An error occurred while deactivating the template");
        }
    }

    private static QuestionType MapQuestionType(QuestionTypeDto dtoType) => dtoType switch
    {
        QuestionTypeDto.TextQuestion => QuestionType.TextQuestion,
        QuestionTypeDto.SelfAssessment => QuestionType.SelfAssessment,
        QuestionTypeDto.GoalAchievement => QuestionType.GoalAchievement,
        _ => throw new ArgumentOutOfRangeException(nameof(dtoType), dtoType, "Unknown question type")
    };
}