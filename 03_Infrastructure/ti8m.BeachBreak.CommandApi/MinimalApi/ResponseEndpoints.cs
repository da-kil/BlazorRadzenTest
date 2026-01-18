using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;
using ti8m.BeachBreak.CommandApi.Authorization;
using ti8m.BeachBreak.CommandApi.DTOs;
using ti8m.BeachBreak.CommandApi.Services;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.CommandApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for questionnaire response management.
/// </summary>
public static class ResponseEndpoints
{
    /// <summary>
    /// Maps questionnaire response management endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapResponseEndpoints(this WebApplication app)
    {
        var responseGroup = app.MapGroup("/c/api/v{version:apiVersion}/responses")
            .WithTags("Responses");

        // Save employee response
        responseGroup.MapPost("/assignment/{assignmentId:guid}", async (
            Guid assignmentId,
            SaveQuestionnaireResponseDto request,
            ICommandDispatcher commandDispatcher,
            UserContext userContext,
            QuestionResponseMappingService mappingService,
            SectionMappingService sectionMappingService,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Get employee ID from authenticated user context
                if (!Guid.TryParse(userContext.Id, out var employeeId))
                {
                    logger.LogWarning("SaveResponse failed: Unable to parse user ID from context");
                    return Results.Problem(
                        title: "User identification failed",
                        detail: "User ID not found in authentication context",
                        statusCode: 401);
                }

                logger.LogInformation("Received SaveResponse request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                    employeeId, assignmentId);

                if (request?.Responses == null)
                {
                    logger.LogWarning("SaveResponse failed: Responses are null");
                    return Results.BadRequest("Responses are required");
                }

                // Convert from strongly-typed DTOs to domain format
                var questionResponses = mappingService.ConvertToTypeSafeFormat(request);

                // Get template structure to organize responses by actual sections
                var sectionResponses = await sectionMappingService.OrganizeResponsesBySectionsAsync(assignmentId, request.TemplateId, questionResponses, CompletionRole.Employee, cancellationToken);

                var command = new SaveEmployeeResponseCommand(
                    employeeId: employeeId,
                    assignmentId: assignmentId,
                    sectionResponses: sectionResponses
                );

                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    logger.LogInformation("SaveResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, ResponseId: {ResponseId}",
                        employeeId, assignmentId, result.Payload);
                    return Results.Ok(result.Payload);
                }
                else
                {
                    logger.LogWarning("SaveResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}",
                        employeeId, assignmentId, result.Message);
                    return Results.Problem(
                        title: "Save response failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while saving the response",
                    statusCode: 500);
            }
        })
        .RequireAuthorization()
        .WithName("SaveEmployeeResponse")
        .WithSummary("Save employee questionnaire response")
        .WithDescription("Saves an employee's response to their assigned questionnaire")
        .Produces<Guid>(200)
        .Produces(400)
        .Produces(401)
        .Produces(500);

        // Save manager response
        responseGroup.MapPost("/manager/assignment/{assignmentId:guid}", async (
            Guid assignmentId,
            SaveQuestionnaireResponseDto request,
            ICommandDispatcher commandDispatcher,
            UserContext userContext,
            IManagerAuthorizationService managerAuthorizationService,
            QuestionResponseMappingService mappingService,
            SectionMappingService sectionMappingService,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Get manager ID from authenticated user context
                if (!Guid.TryParse(userContext.Id, out var managerId))
                {
                    logger.LogWarning("SaveManagerResponse failed: Unable to parse user ID from context");
                    return Results.Problem(
                        title: "User identification failed",
                        detail: "User ID not found in authentication context",
                        statusCode: 401);
                }

                logger.LogInformation("Received SaveManagerResponse request for ManagerId: {ManagerId}, AssignmentId: {AssignmentId}",
                    managerId, assignmentId);

                // Authorization check: Verify manager has access to this assignment
                var canAccess = await managerAuthorizationService.CanAccessAssignmentAsync(managerId, assignmentId);
                if (!canAccess)
                {
                    logger.LogWarning("Manager {ManagerId} attempted to save response for assignment {AssignmentId} without authorization",
                        managerId, assignmentId);
                    return Results.Problem(
                        title: "Access denied",
                        detail: "You are not authorized to save responses for this assignment",
                        statusCode: 403);
                }

                if (request?.Responses == null)
                {
                    logger.LogWarning("SaveManagerResponse failed: Responses are null");
                    return Results.BadRequest("Responses are required");
                }

                // Convert from strongly-typed DTOs to domain format
                var questionResponses = mappingService.ConvertToTypeSafeFormat(request);

                // Get template structure to organize responses by actual sections
                var sectionResponses = await sectionMappingService.OrganizeResponsesBySectionsAsync(assignmentId, request.TemplateId, questionResponses, CompletionRole.Manager, cancellationToken);

                var command = new SaveManagerResponseCommand(
                    managerId: managerId,
                    assignmentId: assignmentId,
                    sectionResponses: sectionResponses
                );

                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    logger.LogInformation("SaveManagerResponse completed successfully for ManagerId: {ManagerId}, AssignmentId: {AssignmentId}, ResponseId: {ResponseId}",
                        managerId, assignmentId, result.Payload);
                    return Results.Ok(result.Payload);
                }
                else
                {
                    logger.LogWarning("SaveManagerResponse failed for ManagerId: {ManagerId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}",
                        managerId, assignmentId, result.Message);
                    return Results.Problem(
                        title: "Save manager response failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while saving the manager response",
                    statusCode: 500);
            }
        })
        .RequireAuthorization()
        .WithName("SaveManagerResponse")
        .WithSummary("Save manager questionnaire response")
        .WithDescription("Saves a manager's response to an assigned questionnaire")
        .Produces<Guid>(200)
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(500);

        // Submit employee response
        responseGroup.MapPost("/assignment/{assignmentId:guid}/submit", async (
            Guid assignmentId,
            ICommandDispatcher commandDispatcher,
            UserContext userContext,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Get employee ID from authenticated user context
                if (!Guid.TryParse(userContext.Id, out var employeeId))
                {
                    logger.LogWarning("SubmitResponse failed: Unable to parse user ID from context");
                    return Results.Problem(
                        title: "User identification failed",
                        detail: "User ID not found in authentication context",
                        statusCode: 401);
                }

                logger.LogInformation("Received SubmitResponse request for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                    employeeId, assignmentId);

                var command = new SubmitEmployeeResponseCommand(
                    employeeId: employeeId,
                    assignmentId: assignmentId
                );

                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    logger.LogInformation("SubmitResponse completed successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}",
                        employeeId, assignmentId);
                    return Results.Ok("Response submitted successfully");
                }
                else
                {
                    logger.LogWarning("SubmitResponse failed for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}, Error: {ErrorMessage}",
                        employeeId, assignmentId, result.Message);
                    return Results.Problem(
                        title: "Submit response failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while submitting the response",
                    statusCode: 500);
            }
        })
        .RequireAuthorization()
        .WithName("SubmitEmployeeResponse")
        .WithSummary("Submit employee questionnaire response")
        .WithDescription("Submits an employee's response for their assigned questionnaire")
        .Produces(200)
        .Produces(400)
        .Produces(401)
        .Produces(500);
    }
}