using ti8m.BeachBreak.CommandApi.Dto;
using ti8m.BeachBreak.CommandApi.Serialization;

namespace ti8m.BeachBreak.CommandApi.MinimalApi;

/// <summary>
/// AOT-compatible minimal API endpoints for health checks and testing.
/// </summary>
public static class HealthEndpoints
{
    /// <summary>
    /// Maps health check endpoints using AOT-compatible minimal APIs.
    /// </summary>
    public static void MapHealthEndpoints(this WebApplication app)
    {
        var healthGroup = app.MapGroup("/api/health")
            .WithTags("Health");

        // Simple health check endpoint
        healthGroup.MapGet("/", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
            .WithName("HealthCheck")
            .WithSummary("Health check endpoint")
            .Produces(200);

        // AOT-compatible error response test
        healthGroup.MapGet("/test-error", () =>
        {
            var errorResponse = new ErrorResponse("Test error for AOT compatibility");
            return Results.Json(errorResponse, CommandApiJsonSerializerContext.Default.ErrorResponse);
        })
            .WithName("TestError")
            .WithSummary("Test AOT-compatible error response")
            .Produces<ErrorResponse>(400);

        // Test permissions response
        healthGroup.MapGet("/test-permissions", () =>
        {
            var permissionsResponse = new InsufficientPermissionsResponse(
                "Test insufficient permissions",
                ["TestPolicy1", "TestPolicy2"],
                ["Admin", "HR"]);
            return Results.Json(permissionsResponse, CommandApiJsonSerializerContext.Default.InsufficientPermissionsResponse);
        })
            .WithName("TestPermissions")
            .WithSummary("Test AOT-compatible permissions response")
            .Produces<InsufficientPermissionsResponse>(403);
    }
}