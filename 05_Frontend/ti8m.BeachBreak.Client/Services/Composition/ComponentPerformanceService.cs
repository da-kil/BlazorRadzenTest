using ti8m.BeachBreak.Client.Services.Enhanced;

namespace ti8m.BeachBreak.Client.Services.Composition;

/// <summary>
/// Implementation of component performance monitoring service.
/// Provides composition-based performance tracking instead of inheritance-based approach.
/// </summary>
public class ComponentPerformanceService : IComponentPerformanceService
{
    /// <summary>
    /// Starts performance tracking for a component operation.
    /// Returns a disposable tracker that automatically stops when disposed.
    /// </summary>
    public IDisposable? StartTracking(string operationName)
    {
        return PerformanceOptimizer.PerformanceMonitor.StartOperation(operationName);
    }

    /// <summary>
    /// Gets performance metrics for a specific operation.
    /// </summary>
    public PerformanceOptimizer.PerformanceMetrics? GetMetrics(string operationName)
    {
        return PerformanceOptimizer.PerformanceMonitor.GetMetrics(operationName);
    }

    /// <summary>
    /// Logs performance metrics to console (debug builds only).
    /// </summary>
    public void LogMetrics(string componentName, string operationName)
    {
#if DEBUG
        var metrics = GetMetrics(operationName);
        if (metrics != null)
        {
            Console.WriteLine($"[{componentName}] Performance: " +
                $"Renders: {metrics.ExecutionCount}, " +
                $"Avg: {metrics.AverageDuration.TotalMilliseconds:F2}ms, " +
                $"Min: {metrics.MinDuration.TotalMilliseconds:F2}ms, " +
                $"Max: {metrics.MaxDuration.TotalMilliseconds:F2}ms");
        }
#endif
    }

    /// <summary>
    /// Debounced execution of an operation to prevent excessive calls.
    /// </summary>
    public async Task DebounceAsync(string key, Func<Task> operation, int delayMs = 100)
    {
        await PerformanceOptimizer.Debouncer.DebounceAsync(key, operation, delayMs);
    }

    /// <summary>
    /// Synchronous debounced execution.
    /// </summary>
    public async Task DebounceAsync(string key, Action operation, int delayMs = 100)
    {
        await PerformanceOptimizer.Debouncer.DebounceAsync(key, () =>
        {
            operation();
            return Task.CompletedTask;
        }, delayMs);
    }

    /// <summary>
    /// Throttled execution of an operation to limit frequency.
    /// Returns true if operation was executed, false if throttled.
    /// </summary>
    public async Task<bool> TryThrottleAsync(string key, Func<Task> operation, int intervalMs = 50)
    {
        return await PerformanceOptimizer.Throttler.TryThrottleAsync(key, operation, intervalMs);
    }

    /// <summary>
    /// Synchronous throttled execution.
    /// </summary>
    public async Task<bool> TryThrottleAsync(string key, Action operation, int intervalMs = 50)
    {
        return await PerformanceOptimizer.Throttler.TryThrottleAsync(key, () =>
        {
            operation();
            return Task.CompletedTask;
        }, intervalMs);
    }
}