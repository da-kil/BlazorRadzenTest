using Microsoft.AspNetCore.Mvc;
using System.Security;
using System.Text.Json;

namespace ti8m.BeachBreak.CommandApi.Middleware;

/// <summary>
/// Global exception handling middleware for CommandApi.
/// Provides consistent error responses, correlation tracking, and structured logging.
/// Follows CLAUDE.md pattern: "NEVER wrap controller actions in try-catch blocks".
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionMiddleware> logger;
    private readonly IWebHostEnvironment environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment environment)
    {
        this.next = next;
        this.logger = logger;
        this.environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                          ?? context.TraceIdentifier;

        // Log the exception with structured information
        logger.LogError(exception,
            "Unhandled exception occurred in CommandApi. " +
            "RequestId: {RequestId}, CorrelationId: {CorrelationId}, " +
            "Method: {Method}, Path: {Path}, User: {User}",
            context.TraceIdentifier,
            correlationId,
            context.Request.Method,
            context.Request.Path,
            context.User?.Identity?.Name ?? "Anonymous");

        // Determine response details based on exception type
        var (statusCode, message, details) = GetErrorResponse(exception);

        // Create ProblemDetails response consistent with API format
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetErrorTitle(statusCode),
            Detail = message,
            Instance = $"{context.Request.Method} {context.Request.Path}",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        // Add correlation tracking
        problemDetails.Extensions.Add("requestId", context.TraceIdentifier);
        problemDetails.Extensions.Add("correlationId", correlationId);
        problemDetails.Extensions.Add("timestamp", DateTimeOffset.UtcNow);

        // Add additional details for development environment
        if (environment.IsDevelopment())
        {
            problemDetails.Extensions.Add("exceptionType", exception.GetType().Name);
            problemDetails.Extensions.Add("stackTrace", exception.StackTrace);
        }

        // Set response headers and content type
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        // Serialize and write response
        var jsonResponse = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private (int statusCode, string message, string? details) GetErrorResponse(Exception exception)
    {
        return exception switch
        {
            // Handle validation exceptions (more specific first)
            ArgumentNullException => (400, "Required data is missing", exception.Message),
            ArgumentException => (400, "Invalid request data", exception.Message),

            // Handle authorization exceptions
            UnauthorizedAccessException => (401, "Unauthorized access", "Authentication is required"),
            SecurityException => (403, "Access forbidden", "Insufficient permissions"),

            // Handle business logic exceptions (if any custom exceptions are thrown)
            InvalidOperationException when exception.Message.Contains("business", StringComparison.OrdinalIgnoreCase)
                => (409, "Business rule violation", exception.Message),

            // Handle timeout exceptions
            TaskCanceledException => (408, "Request timeout", "The operation timed out"),
            TimeoutException => (408, "Request timeout", "The operation timed out"),

            // Handle resource not found
            KeyNotFoundException => (404, "Resource not found", exception.Message),
            FileNotFoundException => (404, "Resource not found", exception.Message),

            // Default: Internal server error for unhandled exceptions
            _ => (500, "An internal error occurred", "The server encountered an unexpected condition")
        };
    }

    private static string GetErrorTitle(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            408 => "Request Timeout",
            409 => "Conflict",
            500 => "Internal Server Error",
            _ => "Error"
        };
    }
}

/// <summary>
/// Extension method to register GlobalExceptionMiddleware.
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}