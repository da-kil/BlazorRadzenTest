using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.AnalyticsQueries;
using ti8m.BeachBreak.Core.Infrastructure;

namespace ti8m.BeachBreak.QueryApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for analytics queries.
/// </summary>
public static class AnalyticsEndpoints
{
    /// <summary>
    /// Maps analytics query endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapAnalyticsEndpoints(this WebApplication app)
    {
        var analyticsGroup = app.MapGroup("/q/api/v{version:apiVersion}/analytics")
            .WithTags("Analytics")
            .RequireAuthorization();

        // Get overall analytics
        analyticsGroup.MapGet("/overview", async (
            IQueryDispatcher queryDispatcher,
            [FromServices] ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var result = await queryDispatcher.QueryAsync(new OverallAnalyticsListQuery(), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok(result.Payload);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogRetrieveOverallAnalyticsError(ex);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving analytics",
                    statusCode: 500);
            }
        })
        .WithName("GetOverallAnalytics")
        .WithSummary("Get overall analytics")
        .WithDescription("Retrieves overall system analytics")
        .Produces<Dictionary<string, object>>(200)
        .Produces(500);

        // Get template analytics
        analyticsGroup.MapGet("/template/{templateId:guid}", async (
            Guid templateId,
            IQueryDispatcher queryDispatcher,
            [FromServices] ILogger logger,
            CancellationToken cancellationToken = default) =>
        {
            try
            {
                var result = await queryDispatcher.QueryAsync(new TemplateAnalyticsListQuery(templateId), cancellationToken);

                if (result.Succeeded)
                {
                    return Results.Ok(result.Payload);
                }
                else
                {
                    return Results.Problem(detail: result.Message, statusCode: result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogRetrieveTemplateAnalyticsError(ex, templateId);
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: "An error occurred while retrieving template analytics",
                    statusCode: 500);
            }
        })
        .WithName("GetTemplateAnalytics")
        .WithSummary("Get template analytics")
        .WithDescription("Retrieves analytics for a specific questionnaire template")
        .Produces<Dictionary<string, object>>(200)
        .Produces(500);
    }
}