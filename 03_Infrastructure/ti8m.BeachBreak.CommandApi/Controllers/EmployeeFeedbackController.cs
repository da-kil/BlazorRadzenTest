using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;
using ti8m.BeachBreak.CommandApi.Dto;

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
    /// Creates a new feedback template.
    /// </summary>
    /// <param name="dto">Template creation data</param>
    /// <returns>Success/failure result</returns>
    [HttpPost("templates")]
    public async Task<IActionResult> CreateFeedbackTemplate([FromBody] CreateFeedbackTemplateDto dto)
    {
        var command = dto.ToCommand();
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Updates an existing feedback template (draft only).
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="dto">Updated template data</param>
    /// <returns>Success/failure result</returns>
    [HttpPut("templates/{id}")]
    public async Task<IActionResult> UpdateFeedbackTemplate(Guid id, [FromBody] UpdateFeedbackTemplateDto dto)
    {
        var command = dto.ToCommand(id);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Publishes a feedback template (makes it available for use).
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <returns>Success/failure result</returns>
    [HttpPost("templates/{id}/publish")]
    public async Task<IActionResult> PublishFeedbackTemplate(Guid id)
    {
        var command = new PublishFeedbackTemplateCommand(id);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Archives a feedback template (hides from active list).
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <returns>Success/failure result</returns>
    [HttpPost("templates/{id}/archive")]
    public async Task<IActionResult> ArchiveFeedbackTemplate(Guid id)
    {
        var command = new ArchiveFeedbackTemplateCommand(id);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Soft deletes a feedback template.
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <returns>Success/failure result</returns>
    [HttpDelete("templates/{id}")]
    public async Task<IActionResult> DeleteFeedbackTemplate(Guid id)
    {
        var command = new DeleteFeedbackTemplateCommand(id);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }

    /// <summary>
    /// Clones an existing feedback template with new ownership.
    /// </summary>
    /// <param name="id">Source template ID</param>
    /// <param name="dto">Clone configuration</param>
    /// <returns>Success/failure result</returns>
    [HttpPost("templates/{id}/clone")]
    public async Task<IActionResult> CloneFeedbackTemplate(Guid id, [FromBody] CloneFeedbackTemplateDto dto)
    {
        var command = dto.ToCommand(id);
        var result = await commandDispatcher.SendAsync(command);
        return CreateResponse(result);
    }
}