using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.Controllers;

[ApiController]
[Route("q/api/v{version:apiVersion}/employees")]
public class EmployeesController : BaseController
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<EmployeesController> logger;

    public EmployeesController(
        IQueryDispatcher queryDispatcher,
        ILogger<EmployeesController> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllEmployees(
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int? organizationNumber = null,
        [FromQuery] string? role = null,
        [FromQuery] Guid? managerId = null)
    {
        logger.LogInformation("Received GetAllEmployees request - IncludeDeleted: {IncludeDeleted}, OrganizationNumber: {OrganizationNumber}, Role: {Role}, ManagerId: {ManagerId}",
            includeDeleted, organizationNumber, role, managerId);

        try
        {
            var query = new EmployeeListQuery
            {
                IncludeDeleted = includeDeleted,
                OrganizationNumber = organizationNumber,
                Role = role,
                ManagerId = managerId
            };

            var result = await queryDispatcher.QueryAsync(query);

            if (result.Succeeded && result.Payload != null)
            {
                var employeeCount = result.Payload.Count();
                logger.LogInformation("GetAllEmployees completed successfully, returned {EmployeeCount} employees", employeeCount);
            }
            else if (!result.Succeeded)
            {
                logger.LogWarning("GetAllEmployees failed: {ErrorMessage}", result.Message);
            }

            return CreateResponse(result, employees =>
            {
                return employees.Select(employee => new EmployeeDto
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Role = employee.Role,
                    EMail = employee.EMail,
                    StartDate = employee.StartDate,
                    EndDate = employee.EndDate,
                    LastStartDate = employee.LastStartDate,
                    ManagerId = employee.ManagerId,
                    Manager = employee.Manager,
                    LoginName = employee.LoginName,
                    EmployeeNumber = employee.EmployeeNumber,
                    OrganizationNumber = employee.OrganizationNumber,
                    Organization = employee.Organization,
                    IsDeleted = employee.IsDeleted
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving employees");
            return StatusCode(500, "An error occurred while retrieving employees");
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee(Guid id)
    {
        logger.LogInformation("Received GetEmployee request for EmployeeId: {EmployeeId}", id);

        try
        {
            var result = await queryDispatcher.QueryAsync(new EmployeeQuery(id));

            if (result?.Payload == null)
            {
                logger.LogInformation("Employee not found for EmployeeId: {EmployeeId}", id);
                return NotFound($"Employee with ID {id} not found");
            }

            if (result.Succeeded)
            {
                logger.LogInformation("GetEmployee completed successfully for EmployeeId: {EmployeeId}", id);
            }
            else
            {
                logger.LogWarning("GetEmployee failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}", id, result.Message);
            }

            return CreateResponse(result, employee => new EmployeeDto
            {
                Id = id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Role = employee.Role,
                EMail = employee.EMail,
                StartDate = employee.StartDate,
                EndDate = employee.EndDate,
                LastStartDate = employee.LastStartDate,
                ManagerId = employee.ManagerId,
                Manager = employee.Manager,
                LoginName = employee.LoginName,
                EmployeeNumber = employee.EmployeeNumber,
                OrganizationNumber = employee.OrganizationNumber,
                Organization = employee.Organization,
                IsDeleted = employee.IsDeleted
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving employee {EmployeeId}", id);
            return StatusCode(500, "An error occurred while retrieving the employee");
        }
    }

    // Employee-specific assignments endpoints
    [HttpGet("{employeeId:guid}/assignments")]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeAssignments(Guid employeeId)
    {
        logger.LogInformation("Received GetEmployeeAssignments request for EmployeeId: {EmployeeId}", employeeId);

        try
        {
            var result = await queryDispatcher.QueryAsync(new EmployeeAssignmentListQuery(employeeId));

            if (result.Succeeded && result.Payload != null)
            {
                logger.LogInformation("GetEmployeeAssignments completed successfully for EmployeeId: {EmployeeId}, returned {Count} assignments", employeeId, result.Payload.Count());
            }
            else if (!result.Succeeded)
            {
                logger.LogWarning("GetEmployeeAssignments failed for EmployeeId: {EmployeeId}, Error: {ErrorMessage}", employeeId, result.Message);
            }

            return CreateResponse(result, assignments =>
            {
                return assignments.Select(assignment => new QuestionnaireAssignmentDto
                {
                    Id = assignment.Id,
                    TemplateId = assignment.TemplateId,
                    EmployeeId = assignment.EmployeeId,
                    EmployeeName = assignment.EmployeeName,
                    EmployeeEmail = assignment.EmployeeEmail,
                    AssignedDate = assignment.AssignedDate,
                    DueDate = assignment.DueDate,
                    CompletedDate = assignment.CompletedDate,
                    Status = MapAssignmentStatus(assignment.Status),
                    AssignedBy = assignment.AssignedBy,
                    Notes = assignment.Notes
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignments for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving employee assignments");
        }
    }

    [HttpGet("{employeeId:guid}/assignments/{assignmentId:guid}")]
    [ProducesResponseType(typeof(QuestionnaireAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeAssignment(Guid employeeId, Guid assignmentId)
    {
        logger.LogInformation("Received GetEmployeeAssignment request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);

        try
        {
            var result = await queryDispatcher.QueryAsync(new EmployeeAssignmentQuery(employeeId, assignmentId));

            if (result?.Payload == null)
            {
                logger.LogInformation("Assignment not found for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);
                return NotFound($"Assignment with ID {assignmentId} not found for employee {employeeId}");
            }

            if (result.Succeeded)
            {
                logger.LogInformation("GetEmployeeAssignment completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);
            }
            else
            {
                logger.LogWarning("GetEmployeeAssignment failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}", employeeId, assignmentId, result.Message);
            }

            return CreateResponse(result, assignment => new QuestionnaireAssignmentDto
            {
                Id = assignment.Id,
                TemplateId = assignment.TemplateId,
                EmployeeId = assignment.EmployeeId,
                EmployeeName = assignment.EmployeeName,
                EmployeeEmail = assignment.EmployeeEmail,
                AssignedDate = assignment.AssignedDate,
                DueDate = assignment.DueDate,
                CompletedDate = assignment.CompletedDate,
                Status = MapAssignmentStatus(assignment.Status),
                AssignedBy = assignment.AssignedBy,
                Notes = assignment.Notes
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignment {AssignmentId} for employee {EmployeeId}", assignmentId, employeeId);
            return StatusCode(500, "An error occurred while retrieving the employee assignment");
        }
    }

    [HttpGet("{employeeId:guid}/responses/assignment/{assignmentId:guid}")]
    [ProducesResponseType(typeof(QuestionnaireResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeResponse(Guid employeeId, Guid assignmentId)
    {
        logger.LogInformation("Received GetEmployeeResponse request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);

        try
        {
            var result = await queryDispatcher.QueryAsync(new EmployeeResponseQuery(employeeId, assignmentId));

            if (result?.Payload == null)
            {
                logger.LogInformation("Response not found for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);
                return NotFound($"Response not found for assignment {assignmentId} and employee {employeeId}");
            }

            return CreateResponse(result, response => new QuestionnaireResponseDto
            {
                Id = response.Id,
                TemplateId = response.TemplateId,
                AssignmentId = response.AssignmentId,
                EmployeeId = response.EmployeeId.ToString(),
                StartedDate = response.StartedDate,
                CompletedDate = response.CompletedDate,
                Status = MapResponseStatus(response.Status),
                SectionResponses = response.SectionResponses.ToDictionary(kvp => kvp.Key, kvp => new SectionResponseDto { SectionId = kvp.Key }),
                ProgressPercentage = response.ProgressPercentage
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving response for assignment {AssignmentId} and employee {EmployeeId}", assignmentId, employeeId);
            return StatusCode(500, "An error occurred while retrieving the employee response");
        }
    }

    [HttpGet("{employeeId:guid}/assignments/progress")]
    [ProducesResponseType(typeof(IEnumerable<AssignmentProgressDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeAssignmentProgress(Guid employeeId)
    {
        logger.LogInformation("Received GetEmployeeAssignmentProgress request for EmployeeId: {EmployeeId}", employeeId);

        try
        {
            var result = await queryDispatcher.QueryAsync(new EmployeeAssignmentProgressQuery(employeeId));

            return CreateResponse(result, progressList =>
            {
                return progressList.Select(progress => new AssignmentProgressDto
                {
                    AssignmentId = progress.AssignmentId,
                    ProgressPercentage = progress.ProgressPercentage,
                    TotalQuestions = progress.TotalQuestions,
                    AnsweredQuestions = progress.AnsweredQuestions,
                    LastModified = progress.LastModified,
                    IsCompleted = progress.IsCompleted,
                    TimeSpent = progress.TimeSpent
                });
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignment progress for employee {EmployeeId}", employeeId);
            return StatusCode(500, "An error occurred while retrieving employee assignment progress");
        }
    }

    [HttpGet("{employeeId:guid}/assignments/{assignmentId:guid}/progress")]
    [ProducesResponseType(typeof(AssignmentProgressDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeAssignmentProgress(Guid employeeId, Guid assignmentId)
    {
        logger.LogInformation("Received GetEmployeeAssignmentProgress request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", employeeId, assignmentId);

        try
        {
            var result = await queryDispatcher.QueryAsync(new EmployeeProgressQuery(employeeId));

            if (result?.Payload == null || !result.Payload.Any())
            {
                return NotFound($"Progress not found for assignment {assignmentId} and employee {employeeId}");
            }

            var assignmentProgress = result.Payload.FirstOrDefault(p => p.AssignmentId == assignmentId);
            if (assignmentProgress == null)
            {
                return NotFound($"Progress not found for assignment {assignmentId} and employee {employeeId}");
            }

            return Ok(new AssignmentProgressDto
            {
                AssignmentId = assignmentProgress.AssignmentId,
                ProgressPercentage = assignmentProgress.ProgressPercentage,
                TotalQuestions = assignmentProgress.TotalQuestions,
                AnsweredQuestions = assignmentProgress.AnsweredQuestions,
                LastModified = assignmentProgress.LastModified,
                IsCompleted = assignmentProgress.IsCompleted,
                TimeSpent = assignmentProgress.TimeSpent
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving assignment progress for employee {EmployeeId}, assignment {AssignmentId}", employeeId, assignmentId);
            return StatusCode(500, "An error occurred while retrieving the assignment progress");
        }
    }

    // Helper methods for status mapping
    private static AssignmentStatus MapAssignmentStatus(Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus status)
    {
        return status switch
        {
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Assigned => AssignmentStatus.Assigned,
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.InProgress => AssignmentStatus.InProgress,
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Completed => AssignmentStatus.Completed,
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Overdue => AssignmentStatus.Overdue,
            Application.Query.Queries.QuestionnaireAssignmentQueries.AssignmentStatus.Cancelled => AssignmentStatus.Cancelled,
            _ => AssignmentStatus.Assigned
        };
    }

    private static Dto.ResponseStatus MapResponseStatus(Application.Query.Queries.ResponseQueries.ResponseStatus status)
    {
        return status switch
        {
            Application.Query.Queries.ResponseQueries.ResponseStatus.NotStarted => Dto.ResponseStatus.NotStarted,
            Application.Query.Queries.ResponseQueries.ResponseStatus.InProgress => Dto.ResponseStatus.InProgress,
            Application.Query.Queries.ResponseQueries.ResponseStatus.Completed => Dto.ResponseStatus.Completed,
            Application.Query.Queries.ResponseQueries.ResponseStatus.Submitted => Dto.ResponseStatus.Submitted,
            _ => Dto.ResponseStatus.NotStarted
        };
    }
}