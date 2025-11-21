using Microsoft.AspNetCore.Components;
using ti8m.BeachBreak.Client.Services.Enhanced;
using System.ComponentModel;

namespace ti8m.BeachBreak.Client.Components.Shared;

/// <summary>
/// Base class for optimized components that reduces unnecessary re-renders
/// and provides performance monitoring capabilities
/// </summary>
public abstract class OptimizedComponentBase : ComponentBase, IDisposable
{
    private bool _disposed;
    private readonly Dictionary<string, object?> _lastParameterValues = new();
    private IDisposable? _performanceTracker;

    /// <summary>
    /// Controls whether the component should perform deep parameter comparison
    /// to avoid unnecessary re-renders. Default is true.
    /// </summary>
    protected virtual bool EnableParameterOptimization => true;

    /// <summary>
    /// Controls whether performance monitoring is enabled for this component.
    /// Default is false for production builds.
    /// </summary>
    protected virtual bool EnablePerformanceMonitoring =>
#if DEBUG
        true;
#else
        false;
#endif

    /// <summary>
    /// The name used for performance tracking. Defaults to component type name.
    /// </summary>
    protected virtual string PerformanceTrackingName => GetType().Name;

    /// <summary>
    /// Override to determine if the component should update based on parameter changes
    /// </summary>
    protected override bool ShouldRender()
    {
        if (!EnableParameterOptimization)
        {
            return base.ShouldRender();
        }

        var shouldRender = HasParametersChanged() || HasStateChanged();

        if (EnablePerformanceMonitoring && !shouldRender)
        {
            Console.WriteLine($"[{PerformanceTrackingName}] Render skipped - no parameter or state changes");
        }

        return shouldRender;
    }

    /// <summary>
    /// Override to track render performance
    /// </summary>
    protected override void OnInitialized()
    {
        if (EnablePerformanceMonitoring)
        {
            _performanceTracker = PerformanceOptimizer.PerformanceMonitor
                .StartOperation($"{PerformanceTrackingName}.Render");
        }

        base.OnInitialized();
    }

    /// <summary>
    /// Override to capture parameter changes for optimization
    /// </summary>
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        if (EnableParameterOptimization)
        {
            CaptureParameterValues(parameters);
        }

        await base.SetParametersAsync(parameters);
    }

    /// <summary>
    /// Override to provide custom state change detection
    /// Protected virtual method that derived classes can override to provide
    /// custom logic for determining if internal state has changed
    /// </summary>
    protected virtual bool HasStateChanged()
    {
        // Default implementation assumes no state change
        // Derived classes should override this to check their specific state
        return false;
    }

    /// <summary>
    /// Method to explicitly trigger a state change notification
    /// Call this when you know the component's internal state has changed
    /// </summary>
    protected void NotifyStateChanged()
    {
        StateHasChanged();
    }

    /// <summary>
    /// Debounced version of StateHasChanged for high-frequency updates
    /// </summary>
    protected async Task NotifyStateChangedDebounced(int delayMs = 100)
    {
        var key = $"{PerformanceTrackingName}.StateChange";
        await PerformanceOptimizer.Debouncer.DebounceAsync(key, () => StateHasChanged(), delayMs);
    }

    /// <summary>
    /// Throttled version of StateHasChanged for high-frequency events
    /// </summary>
    protected async Task<bool> TryNotifyStateChangedThrottled(int intervalMs = 50)
    {
        var key = $"{PerformanceTrackingName}.StateChange";
        return await PerformanceOptimizer.Throttler.TryThrottleAsync(key, () =>
        {
            StateHasChanged();
            return Task.CompletedTask;
        }, intervalMs);
    }

    #region Parameter Change Detection

    /// <summary>
    /// Captures current parameter values for change detection
    /// </summary>
    private void CaptureParameterValues(ParameterView parameters)
    {
        _lastParameterValues.Clear();

        foreach (var parameter in parameters)
        {
            _lastParameterValues[parameter.Name] = parameter.Value;
        }
    }

    /// <summary>
    /// Checks if any parameters have changed since last render
    /// This is now a fallback - prefer overriding HasStateChanged() in derived classes
    /// for more specific change detection
    /// </summary>
    private bool HasParametersChanged()
    {
        // If no parameters were captured, assume change for first render
        if (_lastParameterValues.Count == 0)
        {
            return true;
        }

        // For now, this is a simplified implementation
        // In practice, components should override HasStateChanged() for proper change detection
        // This provides a conservative fallback that assumes changes may have occurred
        return true;
    }

    /// <summary>
    /// Checks if a specific parameter has changed
    /// </summary>
    protected bool HasParameterChanged<T>(string parameterName, T currentValue)
    {
        if (!_lastParameterValues.TryGetValue(parameterName, out var lastValue))
        {
            // Parameter didn't exist before, so it's changed
            _lastParameterValues[parameterName] = currentValue;
            return true;
        }

        var hasChanged = !EqualityComparer<T>.Default.Equals((T)lastValue!, currentValue);

        if (hasChanged)
        {
            _lastParameterValues[parameterName] = currentValue;
        }

        return hasChanged;
    }

    #endregion

    #region Lifecycle Helpers

    /// <summary>
    /// Safe async operation execution with error handling
    /// </summary>
    protected async Task ExecuteSafelyAsync(Func<Task> operation, string? operationName = null)
    {
        try
        {
            using var tracker = EnablePerformanceMonitoring
                ? PerformanceOptimizer.PerformanceMonitor.StartOperation($"{PerformanceTrackingName}.{operationName ?? "AsyncOperation"}")
                : null;

            await operation();
        }
        catch (Exception ex)
        {
            await HandleComponentErrorAsync(ex, operationName);
        }
    }

    /// <summary>
    /// Safe synchronous operation execution with error handling
    /// </summary>
    protected void ExecuteSafely(Action operation, string? operationName = null)
    {
        try
        {
            using var tracker = EnablePerformanceMonitoring
                ? PerformanceOptimizer.PerformanceMonitor.StartOperation($"{PerformanceTrackingName}.{operationName ?? "Operation"}")
                : null;

            operation();
        }
        catch (Exception ex)
        {
            HandleComponentError(ex, operationName);
        }
    }

    /// <summary>
    /// Override this method to provide custom error handling
    /// </summary>
    protected virtual async Task HandleComponentErrorAsync(Exception exception, string? operationName = null)
    {
        Console.WriteLine($"[{PerformanceTrackingName}] Error in {operationName ?? "component operation"}: {exception.Message}");

        // In a real implementation, you might want to:
        // - Log to a proper logging service
        // - Show user-friendly error messages
        // - Report to error tracking service

        await Task.CompletedTask; // Placeholder for async error handling
    }

    /// <summary>
    /// Synchronous version of error handling
    /// Note: This provides true synchronous error handling to avoid deadlocks.
    /// For async error handling, use HandleComponentErrorAsync instead.
    /// </summary>
    protected virtual void HandleComponentError(Exception exception, string? operationName = null)
    {
        Console.WriteLine($"[{PerformanceTrackingName}] Error in {operationName ?? "component operation"}: {exception.Message}");

        // In a real implementation, you might want to:
        // - Log to a proper logging service (synchronously)
        // - Show user-friendly error messages
        // - Report to error tracking service (synchronously)
    }

    #endregion

    #region Memory Management

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public virtual void Dispose()
    {
        if (_disposed) return;

        _performanceTracker?.Dispose();
        _lastParameterValues.Clear();

        _disposed = true;
    }

    /// <summary>
    /// Checks if component is disposed
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    #endregion

    #region Performance Utilities

    /// <summary>
    /// Gets performance metrics for this component
    /// </summary>
    protected PerformanceOptimizer.PerformanceMetrics? GetPerformanceMetrics()
    {
        return PerformanceOptimizer.PerformanceMonitor.GetMetrics($"{PerformanceTrackingName}.Render");
    }

    /// <summary>
    /// Logs current performance metrics to console (debug builds only)
    /// </summary>
    protected void LogPerformanceMetrics()
    {
#if DEBUG
        if (EnablePerformanceMonitoring)
        {
            var metrics = GetPerformanceMetrics();
            if (metrics != null)
            {
                Console.WriteLine($"[{PerformanceTrackingName}] Performance: " +
                    $"Renders: {metrics.ExecutionCount}, " +
                    $"Avg: {metrics.AverageDuration.TotalMilliseconds:F2}ms, " +
                    $"Min: {metrics.MinDuration.TotalMilliseconds:F2}ms, " +
                    $"Max: {metrics.MaxDuration.TotalMilliseconds:F2}ms");
            }
        }
#endif
    }

    #endregion
}