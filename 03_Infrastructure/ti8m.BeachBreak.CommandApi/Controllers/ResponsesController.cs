using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;
using ti8m.BeachBreak.CommandApi.Authorization;
using ti8m.BeachBreak.CommandApi.Models;
using ti8m.BeachBreak.CommandApi.Services;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/responses")]
public class ResponsesController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly UserContext userContext;
    private readonly IManagerAuthorizationService managerAuthorizationService;
    private readonly QuestionResponseMappingService mappingService;
    private readonly ILogger<ResponsesController> logger;

    public ResponsesController(
        ICommandDispatcher commandDispatcher,
        UserContext userContext,
        IManagerAuthorizationService managerAuthorizationService,
        QuestionResponseMappingService mappingService,
        ILogger<ResponsesController> logger)
    {
        this.commandDispatcher = commandDispatcher;
        this.userContext = userContext;
        this.managerAuthorizationService = managerAuthorizationService;
        this.mappingService = mappingService;
        this.logger = logger;
    }

    [HttpPost("assignment/{assignmentId:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveResponse(
        Guid assignmentId,
        [FromBody] Dictionary<Guid, SectionResponse> sectionResponses)
    {
        // Get employee ID from authenticated user context
        if (!Guid.TryParse(userContext.Id, out var employeeId))
        {
            logger.LogWarning("SaveResponse failed: Unable to parse user ID from context");
            return CreateResponse(Result<Guid>.Fail("User ID not found in authentication context", StatusCodes.Status401Unauthorized));
        }

        logger.LogInformation("Received SaveResponse request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
            employeeId, assignmentId);

        if (sectionResponses == null)
        {
            logger.LogWarning("SaveResponse failed: Section responses are null");
            return CreateResponse(Result<Guid>.Fail("Section responses are required", StatusCodes.Status400BadRequest));
        }

        // Convert from API DTOs to type-safe domain format
        var typeSafeSectionResponses = mappingService.ConvertToTypeSafeFormat(sectionResponses);

        var command = new SaveEmployeeResponseCommand(
            employeeId: employeeId,
            assignmentId: assignmentId,
            sectionResponses: typeSafeSectionResponses
        );

        var result = await commandDispatcher.SendAsync(command);

        if (result.Succeeded)
        {
            logger.LogInformation("SaveResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, ResponseId: {ResponseId}",
                employeeId, assignmentId, result.Payload);
        }
        else
        {
            logger.LogWarning("SaveResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}",
                employeeId, assignmentId, result.Message);
        }

        return CreateResponse(result);
    }

    [HttpPost("manager/assignment/{assignmentId:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SaveManagerResponse(
        Guid assignmentId,
        [FromBody] Dictionary<Guid, SectionResponse> sectionResponses)
    {
        // Get manager ID from authenticated user context
        if (!Guid.TryParse(userContext.Id, out var managerId))
        {
            logger.LogWarning("SaveManagerResponse failed: Unable to parse user ID from context");
            return CreateResponse(Result<Guid>.Fail("User ID not found in authentication context", StatusCodes.Status401Unauthorized));
        }

        logger.LogInformation("Received SaveManagerResponse request for ManagerId: {ManagerId}, AssignmentId: {AssignmentId}",
            managerId, assignmentId);

        // Authorization check: Verify manager has access to this assignment
        var canAccess = await managerAuthorizationService.CanAccessAssignmentAsync(managerId, assignmentId);
        if (!canAccess)
        {
            logger.LogWarning("Manager {ManagerId} attempted to save response for assignment {AssignmentId} without authorization",
                managerId, assignmentId);
            return CreateResponse(Result<Guid>.Fail("You are not authorized to save responses for this assignment", StatusCodes.Status403Forbidden));
        }

        if (sectionResponses == null)
        {
            logger.LogWarning("SaveManagerResponse failed: Section responses are null");
            return CreateResponse(Result<Guid>.Fail("Section responses are required", StatusCodes.Status400BadRequest));
        }

        // Convert from API DTOs to type-safe domain format
        var typeSafeSectionResponses = mappingService.ConvertToTypeSafeFormat(sectionResponses);

        var command = new SaveManagerResponseCommand(
            managerId: managerId,
            assignmentId: assignmentId,
            sectionResponses: typeSafeSectionResponses
        );

        var result = await commandDispatcher.SendAsync(command);

        if (result.Succeeded)
        {
            logger.LogInformation("SaveManagerResponse completed successfully for ManagerId: {ManagerId}, AssignmentId: {AssignmentId}, ResponseId: {ResponseId}",
                managerId, assignmentId, result.Payload);
        }
        else
        {
            logger.LogWarning("SaveManagerResponse failed for ManagerId: {ManagerId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}",
                managerId, assignmentId, result.Message);
        }

        return CreateResponse(result);
    }

    [HttpPost("assignment/{assignmentId:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitResponse(Guid assignmentId)
    {
        // Get employee ID from authenticated user context
        if (!Guid.TryParse(userContext.Id, out var employeeId))
        {
            logger.LogWarning("SubmitResponse failed: Unable to parse user ID from context");
            return CreateResponse(Result.Fail("User ID not found in authentication context", StatusCodes.Status401Unauthorized));
        }

        logger.LogInformation("Received SubmitResponse request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
            employeeId, assignmentId);

        var command = new SubmitEmployeeResponseCommand(
            employeeId: employeeId,
            assignmentId: assignmentId
        );

        var result = await commandDispatcher.SendAsync(command);

        if (result.Succeeded)
        {
            logger.LogInformation("SubmitResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                employeeId, assignmentId);
        }
        else
        {
            logger.LogWarning("SubmitResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}",
                employeeId, assignmentId, result.Message);
        }

        return CreateResponse(result);
    }
}