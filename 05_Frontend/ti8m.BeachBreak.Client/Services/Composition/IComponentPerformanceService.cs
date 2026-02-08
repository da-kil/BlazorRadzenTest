using ti8m.BeachBreak.Client.Services.Enhanced;

namespace ti8m.BeachBreak.Client.Services.Composition;

/// <summary>
/// Service for managing component performance monitoring, metrics tracking, and optimization.
/// Extracted from the complex component inheritance hierarchy for composition-based design.
/// </summary>
public interface IComponentPerformanceService
{
    /// <summary>
    /// Starts performance tracking for a component operation.
    /// Returns a disposable tracker that automatically stops when disposed.
    /// </summary>
    IDisposable? StartTracking(string operationName);

    /// <summary>
    /// Gets performance metrics for a specific operation.
    /// </summary>
    PerformanceOptimizer.PerformanceMetrics? GetMetrics(string operationName);

    /// <summary>
    /// Logs performance metrics to console (debug builds only).
    /// </summary>
    void LogMetrics(string componentName, string operationName);

    /// <summary>
    /// Debounced execution of an operation to prevent excessive calls.
    /// </summary>
    Task DebounceAsync(string key, Func<Task> operation, int delayMs = 100);

    /// <summary>
    /// Synchronous debounced execution.
    /// </summary>
    Task DebounceAsync(string key, Action operation, int delayMs = 100);

    /// <summary>
    /// Throttled execution of an operation to limit frequency.
    /// Returns true if operation was executed, false if throttled.
    /// </summary>
    Task<bool> TryThrottleAsync(string key, Func<Task> operation, int intervalMs = 50);

    /// <summary>
    /// Synchronous throttled execution.
    /// </summary>
    Task<bool> TryThrottleAsync(string key, Action operation, int intervalMs = 50);
}