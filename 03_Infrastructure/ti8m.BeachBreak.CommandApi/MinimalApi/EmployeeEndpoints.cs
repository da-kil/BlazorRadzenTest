using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;
using ti8m.BeachBreak.Application.Command.Models;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.CommandApi.Dto;
using ti8m.BeachBreak.CommandApi.DTOs;
using ti8m.BeachBreak.CommandApi.Mappers;
using ti8m.BeachBreak.CommandApi.Services;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Core.Infrastructure;
using CommandResult = ti8m.BeachBreak.Application.Command.Commands.Result;

namespace ti8m.BeachBreak.CommandApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for employee management.
/// </summary>
public static class EmployeeEndpoints
{
    /// <summary>
    /// Maps employee management endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapEmployeeEndpoints(this WebApplication app)
    {
        var employeeGroup = app.MapGroup("/c/api/v{version:apiVersion}/employees")
            .WithTags("Employees");

        // Bulk insert employees
        employeeGroup.MapPost("/bulk-insert", async (
            IEnumerable<EmployeeDto> employees,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
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
                    })), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Employees inserted successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Employee bulk insert failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while inserting employees",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("AdminOrApp") // Allows Admin users OR service principals with DataSeeder app role
        .WithName("BulkInsertEmployees")
        .WithSummary("Bulk insert employees")
        .WithDescription("Imports multiple employees from external source")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(500);

        // Bulk update employees
        employeeGroup.MapPut("/bulk-update", async (
            IEnumerable<EmployeeDto> employees,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
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
                    })), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Employees updated successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Employee bulk update failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while updating employees",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("Admin")
        .WithName("BulkUpdateEmployees")
        .WithSummary("Bulk update employees")
        .WithDescription("Updates multiple employees")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(500);

        // Bulk delete employees
        employeeGroup.MapDelete("/bulk-delete", async (
            IEnumerable<Guid> employeeIds,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                CommandResult result = await commandDispatcher.SendAsync(new BulkDeleteEmployeesCommand(employeeIds), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Employees deleted successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Employee bulk delete failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while deleting employees",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("Admin")
        .WithName("BulkDeleteEmployees")
        .WithSummary("Bulk delete employees")
        .WithDescription("Deletes multiple employees")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(500);

        // Change application role
        employeeGroup.MapPut("/{employeeId:guid}/application-role", async (
            Guid employeeId,
            ChangeApplicationRoleDto dto,
            ICommandDispatcher commandDispatcher,
            IQueryDispatcher queryDispatcher,
            UserContext userContext,
            [FromServices] ILogger logger,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Infrastructure responsibility: Fetch requester's role from database using UserContext
                // If user is not an employee (e.g., service principal), default to Admin role
                ApplicationRole commandRequesterRole = ApplicationRole.Admin;

                if (Guid.TryParse(userContext.Id, out var userId))
                {
                    var requesterRoleResult = await queryDispatcher.QueryAsync(
                        new GetEmployeeRoleByIdQuery(userId),
                        cancellationToken);

                    if (requesterRoleResult != null)
                    {
                        commandRequesterRole = ApplicationRoleMapper.MapFromQuery(requesterRoleResult.ApplicationRole);
                    }
                    else
                    {
                        logger.LogUserNotFoundTreatedAsAdmin(userId);
                    }
                }
                else
                {
                    logger.LogNoUserIdTreatedAsAdmin();
                }

                var domainRequesterRole = ApplicationRoleMapper.MapToDomain(commandRequesterRole);
                var domainNewRole = (Domain.EmployeeAggregate.ApplicationRole)dto.NewRole;

                // Dispatch command with requester's role - business rules validated in domain
                CommandResult result = await commandDispatcher.SendAsync(
                    new ChangeEmployeeApplicationRoleCommand(
                        employeeId,
                        ApplicationRoleMapper.MapFromDomain(domainNewRole),
                        ApplicationRoleMapper.MapFromDomain(domainRequesterRole)), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Employee application role changed successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Role change failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while changing the application role",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("AdminOrApp")
        .WithName("ChangeEmployeeApplicationRole")
        .WithSummary("Change employee application role")
        .WithDescription("Changes the application role of an employee")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .Produces(500);

        // Save employee's own response
        employeeGroup.MapPost("/me/responses/assignment/{assignmentId:guid}", async (
            Guid assignmentId,
            SaveQuestionnaireResponseDto request,
            ICommandDispatcher commandDispatcher,
            UserContext userContext,
            QuestionResponseMappingService mappingService,
            SectionMappingService sectionMappingService,
            [FromServices] ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Get employee ID from authenticated user context
                if (!Guid.TryParse(userContext.Id, out var employeeId))
                {
                    logger.LogSaveResponseFailedNoUserId();
                    return Results.Problem(
                        title: "User identification failed",
                        detail: "User ID not found in authentication context",
                        statusCode: 401);
                }

                logger.LogSaveResponseReceived(employeeId, assignmentId);

                if (request?.Responses == null)
                {
                    logger.LogSaveResponseFailedNullResponses();
                    return Results.BadRequest("Responses are required");
                }

                // Convert from strongly-typed DTOs to domain format
                var questionResponses = mappingService.ConvertToTypeSafeFormat(request);

                // Get template structure to organize responses by actual sections
                var typeSafeSectionResponses = await sectionMappingService.OrganizeResponsesBySectionsAsync(assignmentId, request.TemplateId, questionResponses, CompletionRole.Employee, cancellationToken);

                var command = new SaveEmployeeResponseCommand(
                    employeeId: employeeId,
                    assignmentId: assignmentId,
                    sectionResponses: typeSafeSectionResponses
                );

                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    logger.LogSaveResponseCompleted(employeeId, assignmentId, result.Payload);
                    return Results.Ok(result.Payload);
                }
                else
                {
                    logger.LogSaveResponseFailed(employeeId, assignmentId, result.Message);
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
        .WithName("SaveMyResponse")
        .WithSummary("Save employee's own questionnaire response")
        .WithDescription("Saves the currently authenticated employee's response to their assigned questionnaire")
        .Produces<Guid>(200)
        .Produces(400)
        .Produces(401)
        .Produces(500);

        // Submit employee's own response
        employeeGroup.MapPost("/me/responses/assignment/{assignmentId:guid}/submit", async (
            Guid assignmentId,
            ICommandDispatcher commandDispatcher,
            UserContext userContext,
            [FromServices] ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Get employee ID from authenticated user context
                if (!Guid.TryParse(userContext.Id, out var employeeId))
                {
                    logger.LogSubmitResponseFailedNoUserId();
                    return Results.Problem(
                        title: "User identification failed",
                        detail: "User ID not found in authentication context",
                        statusCode: 401);
                }

                logger.LogSubmitResponseReceived(employeeId, assignmentId);

                var command = new SubmitEmployeeResponseCommand(
                    employeeId: employeeId,
                    assignmentId: assignmentId
                );

                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    logger.LogSubmitResponseCompleted(employeeId, assignmentId);
                    return Results.Ok("Response submitted successfully");
                }
                else
                {
                    logger.LogSubmitResponseFailed(employeeId, assignmentId, result.Message);
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
        .WithName("SubmitMyResponse")
        .WithSummary("Submit employee's own questionnaire response")
        .WithDescription("Submits the currently authenticated employee's response for their assigned questionnaire")
        .Produces(200)
        .Produces(400)
        .Produces(401)
        .Produces(500);

        // Change employee language
        employeeGroup.MapPost("/{id:guid}/language", async (
            Guid id,
            ChangeEmployeeLanguageRequest request,
            ICommandDispatcher commandDispatcher,
            UserContext userContext,
            [FromServices] ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                logger.LogChangeLanguageReceived(id, request.Language.ToString());

                // Get current user ID for authorization
                if (!Guid.TryParse(userContext.Id, out var userId))
                {
                    logger.LogChangeLanguageFailedNoUserId();
                    return Results.Problem(
                        title: "User identification failed",
                        detail: "User ID not found in authentication context",
                        statusCode: 401);
                }

                // For now, users can only change their own language
                // TODO: Add elevated role authorization for HR/Admin to change other employees' languages
                if (userId != id)
                {
                    logger.LogChangeLanguageUnauthorized(userId, id);
                    return Results.Problem(
                        title: "Authorization failed",
                        detail: "You can only change your own language preference",
                        statusCode: 403);
                }

                // Validate language parameter (DTO enum validation is handled by model binding)
                if (!Enum.IsDefined(typeof(LanguageDto), request.Language))
                {
                    return Results.BadRequest($"Invalid language value: {request.Language}. Valid values are: {string.Join(", ", Enum.GetValues<LanguageDto>())}");
                }

                // Map from API DTO to Application layer enum
                var applicationLanguage = LanguageMapper.MapToApplication(request.Language);

                var command = new ChangeEmployeePreferredLanguageCommand(
                    id,
                    applicationLanguage,
                    userId);

                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                logger.LogChangeLanguageResult(id, result.Succeeded);

                if (result.Succeeded)
                {
                    return Results.Ok("Employee language changed successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Language change failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogChangeLanguageError(id, ex);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while changing the employee language",
                    statusCode: 500);
            }
        })
        .RequireAuthorization()
        .WithName("ChangeEmployeeLanguage")
        .WithSummary("Change employee preferred language")
        .WithDescription("Changes the preferred language for a specific employee")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .Produces(500);
    }
}

/// <summary>
/// Request model for changing employee language preference
/// </summary>
public record ChangeEmployeeLanguageRequest(LanguageDto Language);