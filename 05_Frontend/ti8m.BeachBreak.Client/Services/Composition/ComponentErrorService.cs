using System.Collections.Concurrent;

namespace ti8m.BeachBreak.Client.Services.Composition;

/// <summary>
/// Implementation of component error handling service.
/// Provides composition-based error handling instead of inheritance-based approach.
/// Thread-safe for concurrent component usage.
/// </summary>
public class ComponentErrorService : IComponentErrorService
{
    private readonly IComponentPerformanceService performanceService;
    private readonly ConcurrentDictionary<string, Func<Exception, string?, Task>> asyncErrorHandlers = new();
    private readonly ConcurrentDictionary<string, Action<Exception, string?>> syncErrorHandlers = new();

    public ComponentErrorService(IComponentPerformanceService performanceService)
    {
        this.performanceService = performanceService;
    }

    /// <summary>
    /// Executes an async operation safely with error handling and performance tracking.
    /// </summary>
    public async Task ExecuteSafelyAsync(string componentName, Func<Task> operation, string? operationName = null, bool enablePerformanceMonitoring = false)
    {
        try
        {
            using var tracker = enablePerformanceMonitoring
                ? performanceService.StartTracking($"{componentName}.{operationName ?? "AsyncOperation"}")
                : null;

            await operation();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(componentName, ex, operationName);
        }
    }

    /// <summary>
    /// Executes an async operation safely with error handling, performance tracking, and return value.
    /// </summary>
    public async Task<T> ExecuteSafelyAsync<T>(string componentName, Func<Task<T>> operation, T defaultValue, string? operationName = null, bool enablePerformanceMonitoring = false)
    {
        try
        {
            using var tracker = enablePerformanceMonitoring
                ? performanceService.StartTracking($"{componentName}.{operationName ?? "AsyncOperation"}")
                : null;

            return await operation();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(componentName, ex, operationName);
            return defaultValue;
        }
    }

    /// <summary>
    /// Executes a synchronous operation safely with error handling and performance tracking.
    /// </summary>
    public void ExecuteSafely(string componentName, Action operation, string? operationName = null, bool enablePerformanceMonitoring = false)
    {
        try
        {
            using var tracker = enablePerformanceMonitoring
                ? performanceService.StartTracking($"{componentName}.{operationName ?? "Operation"}")
                : null;

            operation();
        }
        catch (Exception ex)
        {
            HandleError(componentName, ex, operationName);
        }
    }

    /// <summary>
    /// Executes a synchronous operation safely with error handling, performance tracking, and return value.
    /// </summary>
    public T ExecuteSafely<T>(string componentName, Func<T> operation, T defaultValue, string? operationName = null, bool enablePerformanceMonitoring = false)
    {
        try
        {
            using var tracker = enablePerformanceMonitoring
                ? performanceService.StartTracking($"{componentName}.{operationName ?? "Operation"}")
                : null;

            return operation();
        }
        catch (Exception ex)
        {
            HandleError(componentName, ex, operationName);
            return defaultValue;
        }
    }

    /// <summary>
    /// Handles component errors with configurable error handling strategy.
    /// </summary>
    public async Task HandleErrorAsync(string componentName, Exception exception, string? operationName = null)
    {
        // Check for custom error handler first
        if (asyncErrorHandlers.TryGetValue(componentName, out var customHandler))
        {
            try
            {
                await customHandler(exception, operationName);
                return;
            }
            catch (Exception handlerEx)
            {
                // Custom handler failed, fall back to default handling
                Console.WriteLine($"[{componentName}] Custom error handler failed: {handlerEx.Message}");
            }
        }

        // Default error handling
        await DefaultErrorHandlingAsync(componentName, exception, operationName);
    }

    /// <summary>
    /// Synchronous error handling for cases where async is not appropriate.
    /// </summary>
    public void HandleError(string componentName, Exception exception, string? operationName = null)
    {
        // Check for custom error handler first
        if (syncErrorHandlers.TryGetValue(componentName, out var customHandler))
        {
            try
            {
                customHandler(exception, operationName);
                return;
            }
            catch (Exception handlerEx)
            {
                // Custom handler failed, fall back to default handling
                Console.WriteLine($"[{componentName}] Custom error handler failed: {handlerEx.Message}");
            }
        }

        // Default error handling
        DefaultErrorHandling(componentName, exception, operationName);
    }

    /// <summary>
    /// Sets a custom async error handler for specific components or operations.
    /// </summary>
    public void SetCustomErrorHandler(string componentName, Func<Exception, string?, Task> errorHandler)
    {
        asyncErrorHandlers[componentName] = errorHandler;
    }

    /// <summary>
    /// Sets a custom synchronous error handler for specific components or operations.
    /// </summary>
    public void SetCustomErrorHandler(string componentName, Action<Exception, string?> errorHandler)
    {
        syncErrorHandlers[componentName] = errorHandler;
    }

    /// <summary>
    /// Default async error handling implementation.
    /// </summary>
    private async Task DefaultErrorHandlingAsync(string componentName, Exception exception, string? operationName)
    {
        Console.WriteLine($"[{componentName}] Error in {operationName ?? "component operation"}: {exception.Message}");

        // In a real implementation, you might want to:
        // - Log to a proper logging service
        // - Show user-friendly error messages
        // - Report to error tracking service
        // - Send telemetry data

        await Task.CompletedTask; // Placeholder for async error handling
    }

    /// <summary>
    /// Default synchronous error handling implementation.
    /// </summary>
    private void DefaultErrorHandling(string componentName, Exception exception, string? operationName)
    {
        Console.WriteLine($"[{componentName}] Error in {operationName ?? "component operation"}: {exception.Message}");

        // In a real implementation, you might want to:
        // - Log to a proper logging service (synchronously)
        // - Show user-friendly error messages
        // - Report to error tracking service (synchronously)
    }
}