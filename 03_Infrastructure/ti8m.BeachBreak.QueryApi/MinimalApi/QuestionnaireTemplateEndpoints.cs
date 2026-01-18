using Microsoft.AspNetCore.Authorization;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.QueryApi.Dto;

namespace ti8m.BeachBreak.QueryApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for questionnaire template queries.
/// </summary>
public static class QuestionnaireTemplateEndpoints
{
    /// <summary>
    /// Maps questionnaire template query endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapQuestionnaireTemplateEndpoints(this WebApplication app)
    {
        var templateGroup = app.MapGroup("/q/api/v{version:apiVersion}/questionnaire-templates")
            .WithTags("Questionnaire Templates")
            .RequireAuthorization(); // All endpoints require authentication - all roles can view templates/responses

        // Get all templates (HR only)
        templateGroup.MapGet("/", async (
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var result = await queryDispatcher.QueryAsync(new QuestionnaireTemplateListQuery(), cancellationToken);

                if (result.Succeeded)
                {
                    var templates = result.Payload.Select(MapToDto);
                    return Results.Ok(templates);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving questionnaire templates");
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving templates",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("HR")
        .WithName("GetAllTemplates")
        .WithSummary("Get all questionnaire templates")
        .WithDescription("Gets all questionnaire templates - HR only")
        .Produces<IEnumerable<QuestionnaireTemplateDto>>(200)
        .Produces(500);

        // Get template by ID
        templateGroup.MapGet("/{id:guid}", async (
            Guid id,
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var result = await queryDispatcher.QueryAsync(new QuestionnaireTemplateQuery(id), cancellationToken);
                if (result == null)
                {
                    return Results.NotFound($"Template with ID {id} not found");
                }

                if (result.Succeeded)
                {
                    var dto = MapToDto(result.Payload);
                    return Results.Ok(dto);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving template {TemplateId}", id);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving the template",
                    statusCode: 500);
            }
        })
        .WithName("GetTemplate")
        .WithSummary("Get template by ID")
        .WithDescription("Gets a specific questionnaire template by ID")
        .Produces<QuestionnaireTemplateDto>(200)
        .Produces(404)
        .Produces(500);

        // Get published templates (HR only)
        templateGroup.MapGet("/published", async (
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var result = await queryDispatcher.QueryAsync(new PublishedQuestionnaireTemplatesQuery(), cancellationToken);

                if (result.Succeeded)
                {
                    var templates = result.Payload.Select(MapToDto);
                    return Results.Ok(templates);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving published questionnaire templates");
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving published templates",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("HR")
        .WithName("GetPublishedTemplates")
        .WithSummary("Get published templates")
        .WithDescription("Gets published questionnaire templates - HR only")
        .Produces<IEnumerable<QuestionnaireTemplateDto>>(200)
        .Produces(500);

        // Get draft templates (HR only)
        templateGroup.MapGet("/drafts", async (
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var result = await queryDispatcher.QueryAsync(new DraftQuestionnaireTemplatesQuery(), cancellationToken);

                if (result.Succeeded)
                {
                    var templates = result.Payload.Select(MapToDto);
                    return Results.Ok(templates);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving draft questionnaire templates");
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving draft templates",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("HR")
        .WithName("GetDraftTemplates")
        .WithSummary("Get draft templates")
        .WithDescription("Gets draft questionnaire templates - HR only")
        .Produces<IEnumerable<QuestionnaireTemplateDto>>(200)
        .Produces(500);

        // Get archived templates (HR only)
        templateGroup.MapGet("/archived", async (
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var result = await queryDispatcher.QueryAsync(new ArchivedQuestionnaireTemplatesQuery(), cancellationToken);

                if (result.Succeeded)
                {
                    var templates = result.Payload.Select(MapToDto);
                    return Results.Ok(templates);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving archived questionnaire templates");
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving archived templates",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("HR")
        .WithName("GetArchivedTemplates")
        .WithSummary("Get archived templates")
        .WithDescription("Gets archived questionnaire templates - HR only")
        .Produces<IEnumerable<QuestionnaireTemplateDto>>(200)
        .Produces(500);

        // Get assignable templates (HR only)
        templateGroup.MapGet("/assignable", async (
            IQueryDispatcher queryDispatcher,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var result = await queryDispatcher.QueryAsync(new AssignableQuestionnaireTemplatesQuery(), cancellationToken);

                if (result.Succeeded)
                {
                    var templates = result.Payload.Select(MapToDto);
                    return Results.Ok(templates);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving assignable questionnaire templates");
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving assignable templates",
                    statusCode: 500);
            }
        })
        .RequireAuthorization("HR")
        .WithName("GetAssignableTemplates")
        .WithSummary("Get assignable templates")
        .WithDescription("Gets assignable questionnaire templates - HR only")
        .Produces<IEnumerable<QuestionnaireTemplateDto>>(200)
        .Produces(500);
    }

    private static QuestionnaireTemplateDto MapToDto(Application.Query.Queries.QuestionnaireTemplateQueries.QuestionnaireTemplate template)
    {
        return new QuestionnaireTemplateDto
        {
            Id = template.Id,
            NameGerman = template.NameGerman,
            NameEnglish = template.NameEnglish,
            DescriptionGerman = template.DescriptionGerman,
            DescriptionEnglish = template.DescriptionEnglish,
            CategoryId = template.CategoryId,
            ProcessType = MapProcessType(template.ProcessType),
            IsCustomizable = template.IsCustomizable,
            AutoInitialize = template.AutoInitialize,
            CreatedDate = template.CreatedDate,
            Status = MapToStatusDto(template.Status),
            PublishedDate = template.PublishedDate,
            LastPublishedDate = template.LastPublishedDate,
            PublishedByEmployeeId = template.PublishedByEmployeeId,
            PublishedByEmployeeName = template.PublishedByEmployeeName,
            Sections = template.Sections.Select(section => new QuestionSectionDto
            {
                Id = section.Id,
                TitleGerman = section.TitleGerman,
                TitleEnglish = section.TitleEnglish,
                DescriptionGerman = section.DescriptionGerman,
                DescriptionEnglish = section.DescriptionEnglish,
                Order = section.Order,
                CompletionRole = MapToCompletionRoleEnum(section.CompletionRole),
                Type = MapQuestionTypeFromString(section.Type),
                Configuration = section.Configuration
            }).ToList()
        };
    }

    private static QueryApi.Dto.TemplateStatus MapToStatusDto(Application.Query.Queries.QuestionnaireTemplateQueries.TemplateStatus queryStatus)
    {
        return queryStatus switch
        {
            Application.Query.Queries.QuestionnaireTemplateQueries.TemplateStatus.Draft => QueryApi.Dto.TemplateStatus.Draft,
            Application.Query.Queries.QuestionnaireTemplateQueries.TemplateStatus.Published => QueryApi.Dto.TemplateStatus.Published,
            Application.Query.Queries.QuestionnaireTemplateQueries.TemplateStatus.Archived => QueryApi.Dto.TemplateStatus.Archived,
            _ => QueryApi.Dto.TemplateStatus.Draft
        };
    }

    private static CompletionRole MapToCompletionRoleEnum(string completionRole)
    {
        return completionRole?.ToLower() switch
        {
            "manager" => CompletionRole.Manager,
            "both" => CompletionRole.Both,
            _ => CompletionRole.Employee
        };
    }

    private static QueryApi.Dto.QuestionType MapQuestionTypeFromString(string type)
    {
        return type switch
        {
            "Assessment" => QueryApi.Dto.QuestionType.Assessment,
            "TextQuestion" => QueryApi.Dto.QuestionType.TextQuestion,
            "Goal" => QueryApi.Dto.QuestionType.Goal,
            _ => QueryApi.Dto.QuestionType.Assessment // Default fallback
        };
    }

    private static QueryApi.Dto.QuestionnaireProcessType MapProcessType(Core.Domain.QuestionnaireProcessType domainProcessType)
    {
        return domainProcessType switch
        {
            Core.Domain.QuestionnaireProcessType.PerformanceReview => QueryApi.Dto.QuestionnaireProcessType.PerformanceReview,
            Core.Domain.QuestionnaireProcessType.Survey => QueryApi.Dto.QuestionnaireProcessType.Survey,
            _ => throw new ArgumentOutOfRangeException(nameof(domainProcessType), domainProcessType, "Unknown process type")
        };
    }
}