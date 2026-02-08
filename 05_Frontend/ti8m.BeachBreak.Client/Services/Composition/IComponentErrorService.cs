namespace ti8m.BeachBreak.Client.Services.Composition;

/// <summary>
/// Service for managing component error handling and safe execution patterns.
/// Extracted from the complex component inheritance hierarchy for composition-based design.
/// </summary>
public interface IComponentErrorService
{
    /// <summary>
    /// Executes an async operation safely with error handling and performance tracking.
    /// </summary>
    Task ExecuteSafelyAsync(string componentName, Func<Task> operation, string? operationName = null, bool enablePerformanceMonitoring = false);

    /// <summary>
    /// Executes an async operation safely with error handling, performance tracking, and return value.
    /// </summary>
    Task<T> ExecuteSafelyAsync<T>(string componentName, Func<Task<T>> operation, T defaultValue, string? operationName = null, bool enablePerformanceMonitoring = false);

    /// <summary>
    /// Executes a synchronous operation safely with error handling and performance tracking.
    /// </summary>
    void ExecuteSafely(string componentName, Action operation, string? operationName = null, bool enablePerformanceMonitoring = false);

    /// <summary>
    /// Executes a synchronous operation safely with error handling, performance tracking, and return value.
    /// </summary>
    T ExecuteSafely<T>(string componentName, Func<T> operation, T defaultValue, string? operationName = null, bool enablePerformanceMonitoring = false);

    /// <summary>
    /// Handles component errors with configurable error handling strategy.
    /// </summary>
    Task HandleErrorAsync(string componentName, Exception exception, string? operationName = null);

    /// <summary>
    /// Synchronous error handling for cases where async is not appropriate.
    /// </summary>
    void HandleError(string componentName, Exception exception, string? operationName = null);

    /// <summary>
    /// Sets a custom error handler for specific components or operations.
    /// </summary>
    void SetCustomErrorHandler(string componentName, Func<Exception, string?, Task> errorHandler);

    /// <summary>
    /// Sets a custom synchronous error handler for specific components or operations.
    /// </summary>
    void SetCustomErrorHandler(string componentName, Action<Exception, string?> errorHandler);
}