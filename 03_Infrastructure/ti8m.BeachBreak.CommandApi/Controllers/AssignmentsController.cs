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
}