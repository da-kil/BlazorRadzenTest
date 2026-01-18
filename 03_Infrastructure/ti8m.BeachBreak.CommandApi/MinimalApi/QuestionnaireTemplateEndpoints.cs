using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;
using ti8m.BeachBreak.CommandApi.Dto;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.CommandApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for questionnaire template management.
/// </summary>
public static class QuestionnaireTemplateEndpoints
{
    /// <summary>
    /// Maps questionnaire template management endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapQuestionnaireTemplateEndpoints(this WebApplication app)
    {
        var templateGroup = app.MapGroup("/c/api/v{version:apiVersion}/questionnaire-templates")
            .WithTags("Questionnaire Templates")
            .RequireAuthorization("HROrApp"); // Allows HR users OR service principals with DataSeeder app role

        // Create template
        templateGroup.MapPost("/", async (
            QuestionnaireTemplateDto questionnaireTemplate,
            ICommandDispatcher commandDispatcher,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(questionnaireTemplate.NameEnglish))
                {
                    return Results.BadRequest("Template name is required");
                }

                var commandTemplate = new CommandQuestionnaireTemplate
                {
                    Id = questionnaireTemplate.Id,
                    CategoryId = questionnaireTemplate.CategoryId,
                    DescriptionGerman = questionnaireTemplate.DescriptionGerman,
                    DescriptionEnglish = questionnaireTemplate.DescriptionEnglish,
                    NameGerman = questionnaireTemplate.NameGerman,
                    NameEnglish = questionnaireTemplate.NameEnglish,
                    ProcessType = MapProcessType(questionnaireTemplate.ProcessType),
                    IsCustomizable = questionnaireTemplate.IsCustomizable,
                    AutoInitialize = questionnaireTemplate.AutoInitialize,
                    Sections = questionnaireTemplate.Sections.Select(section => new CommandQuestionSection
                    {
                        DescriptionGerman = section.DescriptionGerman,
                        DescriptionEnglish = section.DescriptionEnglish,
                        Id = section.Id,
                        Order = section.Order,
                        TitleGerman = section.TitleGerman,
                        TitleEnglish = section.TitleEnglish,
                        CompletionRole = section.CompletionRole,
                        Type = section.Type,
                        Configuration = section.Configuration
                    }).ToList()
                };

                Result result = await commandDispatcher.SendAsync(new CreateQuestionnaireTemplateCommand(commandTemplate), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Template created successfully");
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
                logger.LogError(ex, "Error creating questionnaire template");
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while creating the template",
                    statusCode: 500);
            }
        })
        .WithName("CreateQuestionnaireTemplate")
        .WithSummary("Create a new questionnaire template")
        .WithDescription("Creates a new questionnaire template with sections and questions")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(500);

        // Update template
        templateGroup.MapPut("/{id:guid}", async (
            Guid id,
            QuestionnaireTemplateDto questionnaireTemplate,
            ICommandDispatcher commandDispatcher,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var commandTemplate = new CommandQuestionnaireTemplate
                {
                    Id = id,
                    CategoryId = questionnaireTemplate.CategoryId,
                    DescriptionGerman = questionnaireTemplate.DescriptionGerman,
                    DescriptionEnglish = questionnaireTemplate.DescriptionEnglish,
                    NameGerman = questionnaireTemplate.NameGerman,
                    NameEnglish = questionnaireTemplate.NameEnglish,
                    ProcessType = MapProcessType(questionnaireTemplate.ProcessType),
                    IsCustomizable = questionnaireTemplate.IsCustomizable,
                    AutoInitialize = questionnaireTemplate.AutoInitialize,
                    Sections = questionnaireTemplate.Sections.Select(section => new CommandQuestionSection
                    {
                        DescriptionGerman = section.DescriptionGerman,
                        DescriptionEnglish = section.DescriptionEnglish,
                        Id = section.Id,
                        Order = section.Order,
                        TitleGerman = section.TitleGerman,
                        TitleEnglish = section.TitleEnglish,
                        CompletionRole = section.CompletionRole,
                        Type = section.Type,
                        Configuration = section.Configuration
                    }).ToList()
                };

                Result result = await commandDispatcher.SendAsync(new UpdateQuestionnaireTemplateCommand(id, commandTemplate), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Template updated successfully");
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
                logger.LogError(ex, "Error updating template {TemplateId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while updating the template",
                    statusCode: 500);
            }
        })
        .WithName("UpdateQuestionnaireTemplate")
        .WithSummary("Update an existing questionnaire template")
        .WithDescription("Updates an existing questionnaire template")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(500);

        // Delete template
        templateGroup.MapDelete("/{id:guid}", async (
            Guid id,
            ICommandDispatcher commandDispatcher,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                Result result = await commandDispatcher.SendAsync(new DeleteQuestionnaireTemplateCommand(id), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Template deleted successfully");
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
                logger.LogError(ex, "Error deleting template {TemplateId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while deleting the template",
                    statusCode: 500);
            }
        })
        .WithName("DeleteQuestionnaireTemplate")
        .WithSummary("Delete a questionnaire template")
        .WithDescription("Deletes a questionnaire template")
        .Produces(200)
        .Produces(403)
        .Produces(500);

        // Publish template
        templateGroup.MapPost("/{id:guid}/publish", async (
            Guid id,
            ICommandDispatcher commandDispatcher,
            UserContext userContext,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                // Extract publisher employee ID from authenticated user context
                if (!Guid.TryParse(userContext.Id, out var publishedByEmployeeId))
                {
                    logger.LogWarning("Cannot publish template {TemplateId}: unable to parse user ID from context", id);
                    return Results.Problem(
                        title: "User identification failed",
                        detail: "User identity could not be determined",
                        statusCode: 401);
                }

                // Pass employee ID to command (userContext.Id is the Azure AD object ID, which matches employee ID)
                Result result = await commandDispatcher.SendAsync(new PublishQuestionnaireTemplateCommand(id, publishedByEmployeeId), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Template published successfully");
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
                logger.LogError(ex, "Error publishing template {TemplateId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while publishing the template",
                    statusCode: 500);
            }
        })
        .WithName("PublishQuestionnaireTemplate")
        .WithSummary("Publish a questionnaire template")
        .WithDescription("Publishes a template making it available for assignment creation")
        .Produces(200)
        .Produces(401)
        .Produces(500);

        // Unpublish template
        templateGroup.MapPost("/{id:guid}/unpublish", async (
            Guid id,
            ICommandDispatcher commandDispatcher,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                Result result = await commandDispatcher.SendAsync(new UnpublishQuestionnaireTemplateCommand(id), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Template unpublished successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Template unpublish failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error unpublishing template {TemplateId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while unpublishing the template",
                    statusCode: 500);
            }
        })
        .WithName("UnpublishQuestionnaireTemplate")
        .WithSummary("Unpublish a questionnaire template")
        .WithDescription("Unpublishes a template making it unavailable for new assignments")
        .Produces(200)
        .Produces(500);

        // Archive template
        templateGroup.MapPost("/{id:guid}/archive", async (
            Guid id,
            ICommandDispatcher commandDispatcher,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                Result result = await commandDispatcher.SendAsync(new ArchiveQuestionnaireTemplateCommand(id), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Template archived successfully");
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
                logger.LogError(ex, "Error archiving template {TemplateId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while archiving the template",
                    statusCode: 500);
            }
        })
        .WithName("ArchiveQuestionnaireTemplate")
        .WithSummary("Archive a questionnaire template")
        .WithDescription("Archives a template making it inactive")
        .Produces(200)
        .Produces(500);

        // Restore template
        templateGroup.MapPost("/{id:guid}/restore", async (
            Guid id,
            ICommandDispatcher commandDispatcher,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                Result result = await commandDispatcher.SendAsync(new RestoreQuestionnaireTemplateCommand(id), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok("Template restored successfully");
                }
                else
                {
                    return Results.Problem(
                        title: "Template restore failed",
                        detail: result.Message,
                        statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error restoring template {TemplateId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while restoring the template",
                    statusCode: 500);
            }
        })
        .WithName("RestoreQuestionnaireTemplate")
        .WithSummary("Restore an archived questionnaire template")
        .WithDescription("Restores an archived template back to draft status")
        .Produces(200)
        .Produces(500);

        // Clone template
        templateGroup.MapPost("/{id:guid}/clone", async (
            Guid id,
            CloneTemplateRequestDto? request,
            ICommandDispatcher commandDispatcher,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var command = new CloneQuestionnaireTemplateCommand(
                    id,
                    request?.NamePrefix);

                Result<Guid> result = await commandDispatcher.SendAsync(command, cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok(new CloneTemplateResponseDto { NewTemplateId = result.Payload });
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
                logger.LogError(ex, "Error cloning template {TemplateId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while cloning the template",
                    statusCode: 500);
            }
        })
        .WithName("CloneQuestionnaireTemplate")
        .WithSummary("Clone a questionnaire template")
        .WithDescription("Creates a complete copy of an existing template with new IDs in Draft status")
        .Produces<CloneTemplateResponseDto>(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .Produces(500);
    }

    private static Core.Domain.QuestionnaireProcessType MapProcessType(QuestionnaireProcessType dtoProcessType) => dtoProcessType switch
    {
        QuestionnaireProcessType.PerformanceReview => Core.Domain.QuestionnaireProcessType.PerformanceReview,
        QuestionnaireProcessType.Survey => Core.Domain.QuestionnaireProcessType.Survey,
        _ => throw new ArgumentOutOfRangeException(nameof(dtoProcessType), dtoProcessType, "Unknown process type")
    };
}