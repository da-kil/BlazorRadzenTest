using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;
using ti8m.BeachBreak.Application.Command.Models;
using ti8m.BeachBreak.Application.Command.Services;
using ti8m.BeachBreak.CommandApi.Dto;
using ti8m.BeachBreak.CommandApi.DTOs;
using ti8m.BeachBreak.CommandApi.Mappers;
using ti8m.BeachBreak.CommandApi.Services;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using CommandResult = ti8m.BeachBreak.Application.Command.Commands.Result;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/employees")]
public class EmployeesController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly IEmployeeRoleService employeeRoleService;
    private readonly UserContext userContext;
    private readonly QuestionResponseMappingService mappingService;
    private readonly SectionMappingService sectionMappingService;
    private readonly ILogger<EmployeesController> logger;

    public EmployeesController(
        ICommandDispatcher commandDispatcher,
        IEmployeeRoleService employeeRoleService,
        UserContext userContext,
        QuestionResponseMappingService mappingService,
        SectionMappingService sectionMappingService,
        ILogger<EmployeesController> logger)
    {
        this.commandDispatcher = commandDispatcher;
        this.employeeRoleService = employeeRoleService;
        this.userContext = userContext;
        this.mappingService = mappingService;
        this.sectionMappingService = sectionMappingService;
        this.logger = logger;
    }

    [HttpPost("bulk-insert")]
    [Authorize(Policy = "AdminOrApp")] // Allows Admin users OR service principals with DataSeeder app role
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkInsertEmployees([FromBody] IEnumerable<EmployeeDto> employees)
    {
        CommandResult result = await commandDispatcher.SendAsync(new BulkInsertEmployeesCommand(
            employees.Select(dto => new SyncEmployee
            {
                Id = dto.Id,
                EmployeeId = dto.EmployeeId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = dto.Role,
                EMail = dto.EMail,
                StartDate = dto.StartDate,
                LastStartDate = dto.LastStartDate,
                EndDate = dto.EndDate,
                ManagerId = dto.ManagerId,
                LoginName = dto.LoginName,
                OrganizationNumber = dto.OrganizationNumber

            })));

        return CreateResponse(result);
    }

    [HttpPut("bulk-update")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkUpdateEmployees([FromBody] IEnumerable<EmployeeDto> employees)
    {
        CommandResult result = await commandDispatcher.SendAsync(new BulkUpdateEmployeesCommand(
            employees.Select(dto => new SyncEmployee
            {
                Id = dto.Id,
                EmployeeId = dto.EmployeeId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = dto.Role,
                EMail = dto.EMail,
                StartDate = dto.StartDate,
                LastStartDate = dto.LastStartDate,
                EndDate = dto.EndDate,
                ManagerId = dto.ManagerId,
                LoginName = dto.LoginName,
                OrganizationNumber = dto.OrganizationNumber

            })));

        return CreateResponse(result);
    }

    [HttpDelete("bulk-delete")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkDeleteEmployees([FromBody] IEnumerable<Guid> employeeIds)
    {
        CommandResult result = await commandDispatcher.SendAsync(new BulkDeleteEmployeesCommand(employeeIds));

        return CreateResponse(result);
    }

    /// <summary>
    /// Changes the application role of an employee.
    /// Only Admin, HRLead, HR, or service principals with DataSeeder app role can access this endpoint.
    /// Service principals with DataSeeder role act as Admin for this operation.
    /// For employee users, controller fetches requester's role from database; business rules enforced in domain layer.
    /// </summary>
    [HttpPut("{employeeId:guid}/application-role")]
    [Authorize(Policy = "AdminOrApp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeApplicationRole(
        Guid employeeId,
        [FromBody] ChangeApplicationRoleDto dto)
    {
        // Infrastructure responsibility: Fetch requester's role using EmployeeRoleService
        // If user is not an employee (e.g., service principal), default to Admin role
        ApplicationRole commandRequesterRole = ApplicationRole.Admin;

        if (Guid.TryParse(userContext.Id, out var userId))
        {
            var requesterRole = await employeeRoleService.GetEmployeeRoleAsync(userId, HttpContext.RequestAborted);

            if (requesterRole != null)
            {
                commandRequesterRole = (ApplicationRole)requesterRole.ApplicationRoleValue;
            }
            else
            {
                logger.LogInformation("User {UserId} not found in employee database, treating as Admin (likely service principal)", userId);
            }
        }
        else
        {
            logger.LogInformation("No user ID in context, treating as Admin (likely service principal)");
        }

        var domainRequesterRole = ApplicationRoleMapper.MapToDomain(commandRequesterRole);
        var domainNewRole = (Domain.EmployeeAggregate.ApplicationRole)dto.NewRole;

        // Dispatch command with requester's role - business rules validated in domain
        CommandResult result = await commandDispatcher.SendAsync(
            new ChangeEmployeeApplicationRoleCommand(
                employeeId,
                ApplicationRoleMapper.MapFromDomain(domainNewRole),
                ApplicationRoleMapper.MapFromDomain(domainRequesterRole)));

        return CreateResponse(result);
    }

    /// <summary>
    /// Saves the currently authenticated employee's response to their assigned questionnaire.
    /// Uses UserContext to get the employee ID - more secure than accepting it as a parameter.
    /// This is the employee self-service endpoint that follows the "me" pattern for implicit authorization.
    /// Stores responses with CompletionRole.Employee in the domain model.
    /// </summary>
    [HttpPost("me/responses/assignment/{assignmentId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveMyResponse(
        Guid assignmentId,
        [FromBody] SaveQuestionnaireResponseDto request)
    {
        // Get employee ID from authenticated user context
        if (!userContext.TryGetUserId(out var employeeId, out var errorMessage))
        {
            logger.LogWarning("SaveMyResponse failed: {ErrorMessage}", errorMessage);
            return CreateResponse(Application.Command.Commands.Result<Guid>.Fail(errorMessage, StatusCodes.Status401Unauthorized));
        }

        logger.LogInformation("Received SaveMyResponse request for authenticated EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
            employeeId, assignmentId);

        if (request?.Responses == null)
        {
            logger.LogWarning("SaveMyResponse failed: Responses are null");
            return CreateResponse(Application.Command.Commands.Result<Guid>.Fail("Responses are required", StatusCodes.Status400BadRequest));
        }

        // Convert from strongly-typed DTOs to domain format
        var questionResponses = mappingService.ConvertToTypeSafeFormat(request);

        // Get template structure to organize responses by actual sections
        var typeSafeSectionResponses = await sectionMappingService.OrganizeResponsesBySectionsAsync(assignmentId, request.TemplateId, questionResponses, CompletionRole.Employee, HttpContext.RequestAborted);

        var command = new SaveEmployeeResponseCommand(
            employeeId: employeeId,
            assignmentId: assignmentId,
            sectionResponses: typeSafeSectionResponses
        );

        var result = await commandDispatcher.SendAsync(command);

        if (result.Succeeded)
        {
            logger.LogInformation("SaveMyResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, ResponseId: {ResponseId}",
                employeeId, assignmentId, result.Payload);
        }
        else
        {
            logger.LogWarning("SaveMyResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}",
                employeeId, assignmentId, result.Message);
        }

        return CreateResponse(result);
    }

    /// <summary>
    /// Submits the currently authenticated employee's response for their assigned questionnaire.
    /// Uses UserContext to get the employee ID - more secure than accepting it as a parameter.
    /// This is the employee self-service endpoint that follows the "me" pattern for implicit authorization.
    /// </summary>
    [HttpPost("me/responses/assignment/{assignmentId:guid}/submit")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitMyResponse(Guid assignmentId)
    {
        // Get employee ID from authenticated user context
        if (!userContext.TryGetUserId(out var employeeId, out var errorMessage))
        {
            logger.LogWarning("SubmitMyResponse failed: {ErrorMessage}", errorMessage);
            return CreateResponse(CommandResult.Fail(errorMessage, StatusCodes.Status401Unauthorized));
        }

        logger.LogInformation("Received SubmitMyResponse request for authenticated EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
            employeeId, assignmentId);

        var command = new SubmitEmployeeResponseCommand(
            employeeId: employeeId,
            assignmentId: assignmentId
        );

        var result = await commandDispatcher.SendAsync(command);

        if (result.Succeeded)
        {
            logger.LogInformation("SubmitMyResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                employeeId, assignmentId);
        }
        else
        {
            logger.LogWarning("SubmitMyResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}",
                employeeId, assignmentId, result.Message);
        }

        return CreateResponse(result);
    }

    /// <summary>
    /// Changes the preferred language for a specific employee.
    /// Users can only change their own language preference unless they have elevated roles.
    /// </summary>
    [HttpPost("{id:guid}/language")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangeEmployeeLanguage(Guid id, [FromBody] ChangeEmployeeLanguageRequest request)
    {
        logger.LogInformation("Received ChangeEmployeeLanguage request for EmployeeId: {EmployeeId}, Language: {Language}",
            id, request.Language);

        try
        {
            // Get current user ID for authorization
            if (!userContext.TryGetUserId(out var userId, out var errorMessage))
            {
                logger.LogWarning("ChangeEmployeeLanguage failed: {ErrorMessage}", errorMessage);
                return CreateResponse(CommandResult.Fail(errorMessage, StatusCodes.Status401Unauthorized));
            }

            // For now, users can only change their own language
            // TODO: Add elevated role authorization for HR/Admin to change other employees' languages
            if (userId != id)
            {
                logger.LogWarning("User {UserId} attempted to change language for employee {EmployeeId} without authorization",
                    userId, id);
                return CreateResponse(CommandResult.Fail("You can only change your own language preference", StatusCodes.Status403Forbidden));
            }

            // Validate language parameter (DTO enum validation is handled by model binding)
            if (!Enum.IsDefined(typeof(LanguageDto), request.Language))
            {
                return CreateResponse(CommandResult.Fail($"Invalid language value: {request.Language}. Valid values are: {string.Join(", ", Enum.GetValues<LanguageDto>())}", 400));
            }

            // Map from API DTO to Application layer enum
            var applicationLanguage = LanguageMapper.MapToApplication(request.Language);

            var command = new ChangeEmployeePreferredLanguageCommand(
                id,
                applicationLanguage,
                userId);

            var result = await commandDispatcher.SendAsync(command);

            logger.LogInformation("ChangeEmployeeLanguage command result for EmployeeId {EmployeeId}: Success={Success}",
                id, result.Succeeded);

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error changing language for employee {EmployeeId}", id);
            return CreateResponse(CommandResult.Fail("An error occurred while changing the employee language", 500));
        }
    }
}

/// <summary>
/// Request model for changing employee language preference
/// </summary>
public record ChangeEmployeeLanguageRequest(LanguageDto Language);