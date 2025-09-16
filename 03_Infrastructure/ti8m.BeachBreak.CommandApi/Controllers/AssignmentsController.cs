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

            if (result.Succeeded)
            {
                // Get the actual created assignments from database
                var createdAssignments = await GetRecentAssignmentsByTemplateAsync(questionnaireAssignment.TemplateId, questionnaireAssignment.EmployeeIds);

                logger.LogInformation("Successfully created {AssignmentCount} assignments", createdAssignments.Count);
                return Ok(createdAssignments);
            }
            else
            {
                logger.LogWarning("Failed to create assignments: {ErrorMessage}", result.Message);
                return BadRequest(result.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating assignments");
            return StatusCode(500, "An error occurred while creating assignments");
        }
    }

    //[HttpPatch("{id:guid}/status")]
    //public async Task<ActionResult<QuestionnaireAssignment>> UpdateAssignmentStatus(Guid id, [FromBody] AssignmentStatus status)
    //{
    //    try
    //    {
    //        var assignment = await _questionnaireService.UpdateAssignmentStatusAsync(id, status);
    //        if (assignment == null)
    //            return NotFound($"Assignment with ID {id} not found");

    //        return Ok(assignment);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error updating assignment status {AssignmentId}", id);
    //        return StatusCode(500, "An error occurred while updating assignment status");
    //    }
    //}

    //[HttpDelete("{id:guid}")]
    //public async Task<IActionResult> DeleteAssignment(Guid id)
    //{
    //    try
    //    {
    //        var success = await _questionnaireService.DeleteAssignmentAsync(id);
    //        if (!success)
    //            return NotFound($"Assignment with ID {id} not found");

    //        return NoContent();
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error deleting assignment {AssignmentId}", id);
    //        return StatusCode(500, "An error occurred while deleting the assignment");
    //    }
    //}

    private async Task<List<object>> GetRecentAssignmentsByTemplateAsync(Guid templateId, List<string> employeeIds)
    {
        const string sql = """
            SELECT id, template_id, employee_id, employee_name, employee_email,
                   assigned_date, due_date, completed_date, status, assigned_by, notes
            FROM questionnaire_assignments
            WHERE template_id = @templateId
            AND employee_id = ANY(@employeeIds)
            AND assigned_date >= NOW() - INTERVAL '5 minutes'
            ORDER BY assigned_date DESC
            """;

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        command.Parameters.AddWithValue("@templateId", templateId);
        command.Parameters.AddWithValue("@employeeIds", employeeIds.Select(Guid.Parse).ToArray());

        var assignments = new List<object>();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            assignments.Add(new
            {
                Id = reader.GetGuid(0),
                TemplateId = reader.GetGuid(1),
                EmployeeId = reader.GetGuid(2).ToString(),
                EmployeeName = reader.GetString(3),
                EmployeeEmail = reader.GetString(4),
                AssignedDate = reader.GetDateTime(5),
                DueDate = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                CompletedDate = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                Status = reader.GetString(8),
                AssignedBy = reader.IsDBNull(9) ? null : reader.GetString(9),
                Notes = reader.IsDBNull(10) ? null : reader.GetString(10)
            });
        }

        return assignments;
    }
}