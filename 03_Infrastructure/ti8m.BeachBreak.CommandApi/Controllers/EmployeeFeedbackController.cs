using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.CommandApi.Dto;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.CommandApi.Controllers;

/// <summary>
/// API controller for employee feedback command operations.
/// Supports recording, updating, and deleting feedback from multiple sources.
/// </summary>
[ApiController]
[Route("c/api/v{version:apiVersion}/employee-feedbacks")]
[Authorize(Policy = "TeamLead")] // TeamLead, HR and above can access feedback operations
public class EmployeeFeedbackController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;

    public EmployeeFeedbackController(ICommandDispatcher commandDispatcher)
    {
        this.commandDispatcher = commandDispatcher;
    }

    /// <summary>
    /// Records new employee feedback from external sources.
    /// </summary>
    /// <param name="dto">Feedback data including source type, provider info, and ratings/comments</param>
    /// <returns>ID of the created feedback record</returns>
    [HttpPost]
    public async Task<IActionResult> RecordFeedback([FromBody] RecordEmployeeFeedbackDto dto)
    {
        var command = dto.ToCommand();
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Updates existing employee feedback.
    /// </summary>
    /// <param name="id">ID of the feedback to update</param>
    /// <param name="dto">Updated feedback data</param>
    /// <returns>Success/failure result</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFeedback(Guid id, [FromBody] UpdateEmployeeFeedbackDto dto)
    {
        var command = dto.ToCommand(id);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Soft deletes employee feedback.
    /// </summary>
    /// <param name="id">ID of the feedback to delete</param>
    /// <param name="deleteReason">Optional reason for deletion</param>
    /// <returns>Success/failure result</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFeedback(Guid id, [FromQuery] string? deleteReason = null)
    {
        var command = new DeleteEmployeeFeedbackCommand(id, deleteReason);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Creates a new custom feedback template.
    /// </summary>
    /// <param name="dto">Template creation data</param>
    /// <returns>Created template ID</returns>
    [HttpPost("templates")]
    public async Task<IActionResult> CreateFeedbackTemplate([FromBody] CreateFeedbackTemplateDto dto)
    {
        var command = new CreateFeedbackTemplateCommand
        {
            TemplateName = dto.TemplateName,
            Description = dto.Description ?? string.Empty,
            SourceType = dto.SourceType,
            EvaluationCriteria = dto.EvaluationCriteria ?? new List<EvaluationItem>(),
            TextSections = dto.TextSections ?? new List<TextSectionDefinition>(),
            RatingScale = dto.RatingScale,
            ScaleLowLabel = dto.ScaleLowLabel ?? "Poor",
            ScaleHighLabel = dto.ScaleHighLabel ?? "Excellent",
            IsActive = dto.IsActive,
            IsDefault = dto.IsDefault
        };

        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }
}