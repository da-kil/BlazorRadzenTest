using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.EmployeeFeedbackCommands;
using ti8m.BeachBreak.CommandApi.Dto;

namespace ti8m.BeachBreak.CommandApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for employee feedback management.
/// </summary>
public static class EmployeeFeedbackEndpoints
{
    /// <summary>
    /// Maps employee feedback management endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapEmployeeFeedbackEndpoints(this WebApplication app)
    {
        var feedbackGroup = app.MapGroup("/c/api/v{version:apiVersion}/employee-feedbacks")
            .WithTags("Employee Feedback")
            .RequireAuthorization("TeamLeadOrApp"); // TeamLead, HR and above can access feedback operations

        // Record feedback
        feedbackGroup.MapPost("/", async (
            RecordEmployeeFeedbackDto dto,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = dto.ToCommand();
                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Employee feedback recorded successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Feedback recording failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while recording the feedback",
                    statusCode: 500);
            }
        })
        .WithName("RecordEmployeeFeedback")
        .WithSummary("Record new employee feedback")
        .WithDescription("Records new employee feedback from external sources")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Update feedback
        feedbackGroup.MapPut("/{id:guid}", async (
            Guid id,
            UpdateEmployeeFeedbackDto dto,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = dto.ToCommand(id);
                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Employee feedback updated successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Feedback update failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while updating the feedback",
                    statusCode: 500);
            }
        })
        .WithName("UpdateEmployeeFeedback")
        .WithSummary("Update existing employee feedback")
        .WithDescription("Updates existing employee feedback")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Delete feedback
        feedbackGroup.MapDelete("/{id:guid}", async (
            Guid id,
            ICommandDispatcher commandDispatcher,
            string? deleteReason,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new DeleteEmployeeFeedbackCommand(id, deleteReason);
                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Employee feedback deleted successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Feedback deletion failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while deleting the feedback",
                    statusCode: 500);
            }
        })
        .WithName("DeleteEmployeeFeedback")
        .WithSummary("Delete employee feedback")
        .WithDescription("Soft deletes employee feedback")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Create feedback template
        feedbackGroup.MapPost("/templates", async (
            CreateFeedbackTemplateDto dto,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = dto.ToCommand();
                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Feedback template created successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Template creation failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while creating the template",
                    statusCode: 500);
            }
        })
        .WithName("CreateFeedbackTemplate")
        .WithSummary("Create a new feedback template")
        .WithDescription("Creates a new feedback template")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Update feedback template
        feedbackGroup.MapPut("/templates/{id:guid}", async (
            Guid id,
            UpdateFeedbackTemplateDto dto,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = dto.ToCommand(id);
                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Feedback template updated successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Template update failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while updating the template",
                    statusCode: 500);
            }
        })
        .WithName("UpdateFeedbackTemplate")
        .WithSummary("Update an existing feedback template")
        .WithDescription("Updates an existing feedback template (draft only)")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Publish feedback template
        feedbackGroup.MapPost("/templates/{id:guid}/publish", async (
            Guid id,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new PublishFeedbackTemplateCommand(id);
                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Feedback template published successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Template publish failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while publishing the template",
                    statusCode: 500);
            }
        })
        .WithName("PublishFeedbackTemplate")
        .WithSummary("Publish a feedback template")
        .WithDescription("Publishes a feedback template (makes it available for use)")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Archive feedback template
        feedbackGroup.MapPost("/templates/{id:guid}/archive", async (
            Guid id,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new ArchiveFeedbackTemplateCommand(id);
                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Feedback template archived successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Template archive failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while archiving the template",
                    statusCode: 500);
            }
        })
        .WithName("ArchiveFeedbackTemplate")
        .WithSummary("Archive a feedback template")
        .WithDescription("Archives a feedback template (hides from active list)")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Delete feedback template
        feedbackGroup.MapDelete("/templates/{id:guid}", async (
            Guid id,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new DeleteFeedbackTemplateCommand(id);
                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Feedback template deleted successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Template deletion failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while deleting the template",
                    statusCode: 500);
            }
        })
        .WithName("DeleteFeedbackTemplate")
        .WithSummary("Delete a feedback template")
        .WithDescription("Soft deletes a feedback template")
        .Produces(200)
        .Produces(400)
        .Produces(500);

        // Clone feedback template
        feedbackGroup.MapPost("/templates/{id:guid}/clone", async (
            Guid id,
            CloneFeedbackTemplateDto dto,
            ICommandDispatcher commandDispatcher,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = dto.ToCommand(id);
                var result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Feedback template cloned successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Template clone failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while cloning the template",
                    statusCode: 500);
            }
        })
        .WithName("CloneFeedbackTemplate")
        .WithSummary("Clone a feedback template")
        .WithDescription("Clones an existing feedback template with new ownership")
        .Produces(200)
        .Produces(400)
        .Produces(500);
    }
}