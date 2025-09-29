using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.QueryApi.Controllers;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/assignments")]
public class AssignmentsController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<AssignmentsController> logger;

    public AssignmentsController(
        IQueryDispatcher queryDispatcher,
        ILogger<AssignmentsController> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAssignments()
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new QuestionnaireAssignmentListQuery());
            return CreateResponse(result, templates =>
            {
                return templates.Select(template => new QuestionnaireAssignmentDto
                {
                    AssignedBy = template.AssignedBy,
                    AssignedDate = template.AssignedDate,
                    CompletedDate = template.CompletedDate,
                    DueDate = template.DueDate,
                    EmployeeEmail = template.EmployeeEmail,
                    EmployeeId = template.EmployeeId.ToString(),
                    EmployeeName = template.EmployeeName,
                    Id = template.Id,
                    Notes = template.Notes,
                    Status = MapAssignmentStatusToDto[template.Status],
                    TemplateId = template.TemplateId
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments");
            return StatusCode(500, "An error occurred while retrieving assignments");
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuestionnaireAssignmentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignment(Guid id)
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new QuestionnaireAssignmentQuery(id));
            if (result == null)
                return NotFound($"Assignment with ID {id} not found");

            return CreateResponse(result, template => new QuestionnaireAssignmentDto
            {
                AssignedBy = template.AssignedBy,
                AssignedDate = template.AssignedDate,
                CompletedDate = template.CompletedDate,
                DueDate = template.DueDate,
                EmployeeEmail = template.EmployeeEmail,
                EmployeeId = template.EmployeeId.ToString(),
                EmployeeName = template.EmployeeName,
                Id = template.Id,
                Notes = template.Notes,
                Status = MapAssignmentStatusToDto[template.Status],
                TemplateId = template.TemplateId
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignment {AssignmentId}", id);
            return StatusCode(500, "An error occurred while retrieving the assignment");
        }
    }

    [HttpGet("employee/{employeeId}")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignmentsByEmployee(Guid employeeId)
    {
        try
        {
            var result = await queryDispatcher.QueryAsync(new QuestionnaireEmployeeAssignmentListQuery(employeeId));
            return CreateResponse(result, templates =>
            {
                return templates.Select(template => new QuestionnaireAssignmentDto
                {
                    AssignedBy = template.AssignedBy,
                    AssignedDate = template.AssignedDate,
                    CompletedDate = template.CompletedDate,
                    DueDate = template.DueDate,
                    EmployeeEmail = template.EmployeeEmail,
                    EmployeeId = template.EmployeeId.ToString(),
                    EmployeeName = template.EmployeeName,
                    Id = template.Id,
                    Notes = template.Notes,
                    Status = MapAssignmentStatusToDto[template.Status],
                    TemplateId = template.TemplateId
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving assignments");
        }
    }

    private static IReadOnlyDictionary<Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus, QueryApi.Dto.AssignmentStatus> MapAssignmentStatusToDto =>
    new Dictionary<Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus, QueryApi.Dto.AssignmentStatus>
    {
        { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Assigned, QueryApi.Dto.AssignmentStatus.Assigned },
        { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Overdue, QueryApi.Dto.AssignmentStatus.Overdue },
        { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Cancelled, QueryApi.Dto.AssignmentStatus.Cancelled },
        { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.InProgress, QueryApi.Dto.AssignmentStatus.InProgress },
        { Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Completed, QueryApi.Dto.AssignmentStatus.Completed },
    };
}