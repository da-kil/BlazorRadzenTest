using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;
using ti8m.BeachBreak.CommandApi.Dto;
using Npgsql;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/assignments")]
public class AssignmentsController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly ILogger<AssignmentsController> logger;
    private readonly NpgsqlDataSource dataSource;

    public AssignmentsController(
        ICommandDispatcher commandDispatcher,
        ILogger<AssignmentsController> logger,
        NpgsqlDataSource dataSource)
    {
        this.commandDispatcher = commandDispatcher;
        this.logger = logger;
        this.dataSource = dataSource;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAssignments(QuestionnaireAssignmentDto questionnaireAssignment)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (questionnaireAssignment.EmployeeIds == null || !questionnaireAssignment.EmployeeIds.Any())
                return BadRequest("At least one employee ID is required");

            var result = await commandDispatcher.SendAsync(new CreateQuestionnaireAssignmentCommand(
                new QuestionnaireAssignment
                {
                    AssignedBy = questionnaireAssignment.AssignedBy,
                    DueDate = questionnaireAssignment.DueDate,
                    EmployeeIds = questionnaireAssignment.EmployeeIds,
                    Notes = questionnaireAssignment.Notes,
                    TemplateId = questionnaireAssignment.TemplateId
                }));

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating assignments");
            return StatusCode(500, "An error occurred while creating assignments");
        }
    }

    [HttpPost("reminder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendReminder([FromBody] AssignmentReminderDto reminderDto)
    {
        logger.LogInformation("Received SendReminder request for Assignment: {AssignmentId}", reminderDto.AssignmentId);

        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("SendReminder request failed validation");
                return BadRequest(ModelState);
            }

            var command = new SendAssignmentReminderCommand(
                reminderDto.AssignmentId,
                reminderDto.Message,
                reminderDto.SentBy);

            var result = await commandDispatcher.SendAsync(command);

            if (result.Succeeded)
            {
                logger.LogInformation("SendReminder completed successfully for Assignment: {AssignmentId}", reminderDto.AssignmentId);
                return Ok();
            }
            else
            {
                logger.LogWarning("SendReminder failed for Assignment: {AssignmentId}, Error: {ErrorMessage}", reminderDto.AssignmentId, result.Message);
                return BadRequest(result.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending reminder for Assignment: {AssignmentId}", reminderDto.AssignmentId);
            return StatusCode(500, "An error occurred while sending the reminder");
        }
    }

    [HttpPost("bulk-reminder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendBulkReminder([FromBody] BulkAssignmentReminderDto bulkReminderDto)
    {
        var assignmentCount = bulkReminderDto.AssignmentIds?.Count() ?? 0;
        logger.LogInformation("Received SendBulkReminder request for {AssignmentCount} assignments", assignmentCount);

        try
        {
            if (!ModelState.IsValid)
            {
                logger.LogWarning("SendBulkReminder request failed validation");
                return BadRequest(ModelState);
            }

            if (bulkReminderDto.AssignmentIds == null || !bulkReminderDto.AssignmentIds.Any())
            {
                logger.LogWarning("SendBulkReminder request received with no assignment IDs");
                return BadRequest("No assignment IDs provided");
            }

            var command = new SendBulkAssignmentReminderCommand(
                bulkReminderDto.AssignmentIds,
                bulkReminderDto.Message,
                bulkReminderDto.SentBy);

            var result = await commandDispatcher.SendAsync(command);

            if (result.Succeeded)
            {
                logger.LogInformation("SendBulkReminder completed successfully for {AssignmentCount} assignments", assignmentCount);
                return Ok();
            }
            else
            {
                logger.LogWarning("SendBulkReminder failed for {AssignmentCount} assignments, Error: {ErrorMessage}", assignmentCount, result.Message);
                return BadRequest(result.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending bulk reminder for {AssignmentCount} assignments", assignmentCount);
            return StatusCode(500, "An error occurred while sending bulk reminders");
        }
    }
}