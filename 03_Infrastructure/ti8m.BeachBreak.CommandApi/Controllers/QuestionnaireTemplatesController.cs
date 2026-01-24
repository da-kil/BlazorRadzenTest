using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;
using ti8m.BeachBreak.CommandApi.Dto;
using ti8m.BeachBreak.CommandApi.Mappers;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/questionnaire-templates")]
[Authorize(Policy = "HROrApp")] // Allows HR users OR service principals with DataSeeder app role
public class QuestionnaireTemplatesController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly UserContext userContext;
    private readonly ILogger<QuestionnaireTemplatesController> logger;
    private readonly IQuestionSectionMapper questionSectionMapper;

    public QuestionnaireTemplatesController(
        ICommandDispatcher commandDispatcher,
        UserContext userContext,
        ILogger<QuestionnaireTemplatesController> logger,
        IQuestionSectionMapper questionSectionMapper)
    {
        this.commandDispatcher = commandDispatcher;
        this.userContext = userContext;
        this.logger = logger;
        this.questionSectionMapper = questionSectionMapper;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTemplate(QuestionnaireTemplateDto questionnaireTemplate)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        if (string.IsNullOrWhiteSpace(questionnaireTemplate.NameEnglish))
            return CreateResponse(Result.Fail("Template name is required", 400));

        var commandTemplate = new CommandQuestionnaireTemplate
        {
            Id = questionnaireTemplate.Id,
            CategoryId = questionnaireTemplate.CategoryId,
            DescriptionGerman = questionnaireTemplate.DescriptionGerman,
            DescriptionEnglish = questionnaireTemplate.DescriptionEnglish,
            NameGerman = questionnaireTemplate.NameGerman,
            NameEnglish = questionnaireTemplate.NameEnglish,
            ProcessType = MapProcessType(questionnaireTemplate.ProcessType),
            IsCustomizable = questionnaireTemplate.IsCustomizable,
            AutoInitialize = questionnaireTemplate.AutoInitialize,
            Sections = questionSectionMapper.MapToCommandList(questionnaireTemplate.Sections)
        };

        Result result = await commandDispatcher.SendAsync(new CreateQuestionnaireTemplateCommand(commandTemplate));

        return CreateResponse(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTemplate(Guid id, QuestionnaireTemplateDto questionnaireTemplate)
    {
        if (!ModelState.IsValid)
            return CreateResponse(Result.Fail("Invalid model state", 400));

        var commandTemplate = new CommandQuestionnaireTemplate
        {
            Id = id,
            CategoryId = questionnaireTemplate.CategoryId,
            DescriptionGerman = questionnaireTemplate.DescriptionGerman,
            DescriptionEnglish = questionnaireTemplate.DescriptionEnglish,
            NameGerman = questionnaireTemplate.NameGerman,
            NameEnglish = questionnaireTemplate.NameEnglish,
            ProcessType = MapProcessType(questionnaireTemplate.ProcessType),
            IsCustomizable = questionnaireTemplate.IsCustomizable,
            AutoInitialize = questionnaireTemplate.AutoInitialize,
            Sections = questionSectionMapper.MapToCommandList(questionnaireTemplate.Sections)
        };

        Result result = await commandDispatcher.SendAsync(new UpdateQuestionnaireTemplateCommand(id, commandTemplate));

        return CreateResponse(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        Result result = await commandDispatcher.SendAsync(new DeleteQuestionnaireTemplateCommand(id));

        return CreateResponse(result);
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> PublishTemplate(Guid id)
    {
        // Extract publisher employee ID from authenticated user context
        if (!Guid.TryParse(userContext.Id, out var publishedByEmployeeId))
        {
            logger.LogWarning("Cannot publish template {TemplateId}: unable to parse user ID from context", id);
            return CreateResponse(Result.Fail("User identity could not be determined", 401));
        }

        // Pass employee ID to command (userContext.Id is the Azure AD object ID, which matches employee ID)
        Result result = await commandDispatcher.SendAsync(new PublishQuestionnaireTemplateCommand(id, publishedByEmployeeId));

        return CreateResponse(result);
    }

    [HttpPost("{id:guid}/unpublish")]
    public async Task<IActionResult> UnpublishTemplate(Guid id)
    {
        Result result = await commandDispatcher.SendAsync(new UnpublishQuestionnaireTemplateCommand(id));

        return CreateResponse(result);
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> ArchiveTemplate(Guid id)
    {
        Result result = await commandDispatcher.SendAsync(new ArchiveQuestionnaireTemplateCommand(id));

        return CreateResponse(result);
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> RestoreTemplate(Guid id)
    {
        Result result = await commandDispatcher.SendAsync(new RestoreQuestionnaireTemplateCommand(id));

        return CreateResponse(result);
    }

    /// <summary>
    /// Clone an existing questionnaire template.
    /// Creates a complete copy with new IDs in Draft status.
    /// </summary>
    /// <param name="id">Source template ID to clone</param>
    /// <param name="request">Optional clone request with name prefix</param>
    /// <returns>The ID of the newly created cloned template</returns>
    [HttpPost("{id:guid}/clone")]
    [ProducesResponseType(typeof(CloneTemplateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloneTemplate(Guid id, [FromBody] CloneTemplateRequestDto? request = null)
    {
        var command = new CloneQuestionnaireTemplateCommand(
            id,
            request?.NamePrefix);

        Result<Guid> result = await commandDispatcher.SendAsync(command);

        if (!result.Succeeded)
            return CreateResponse(result);

        return CreateResponse(Result<CloneTemplateResponseDto>.Success(new CloneTemplateResponseDto { NewTemplateId = result.Payload }));
    }

    private static QuestionType MapQuestionType(QuestionTypeDto dtoType) => dtoType switch
    {
        QuestionTypeDto.TextQuestion => QuestionType.TextQuestion,
        QuestionTypeDto.Assessment => QuestionType.Assessment,
        QuestionTypeDto.Goal => QuestionType.Goal,
        _ => throw new ArgumentOutOfRangeException(nameof(dtoType), dtoType, "Unknown question type")
    };

    private static QuestionTypeDto MapQuestionTypeToDto(QuestionType questionType) => questionType switch
    {
        QuestionType.TextQuestion => QuestionTypeDto.TextQuestion,
        QuestionType.Assessment => QuestionTypeDto.Assessment,
        QuestionType.Goal => QuestionTypeDto.Goal,
        _ => throw new ArgumentOutOfRangeException(nameof(questionType), questionType, "Unknown question type")
    };

    private static Core.Domain.QuestionnaireProcessType MapProcessType(QuestionnaireProcessType dtoProcessType) => dtoProcessType switch
    {
        QuestionnaireProcessType.PerformanceReview => Core.Domain.QuestionnaireProcessType.PerformanceReview,
        QuestionnaireProcessType.Survey => Core.Domain.QuestionnaireProcessType.Survey,
        _ => throw new ArgumentOutOfRangeException(nameof(dtoProcessType), dtoProcessType, "Unknown process type")
    };
}