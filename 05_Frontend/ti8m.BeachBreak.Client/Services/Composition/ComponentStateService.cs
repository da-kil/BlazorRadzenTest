using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;

namespace ti8m.BeachBreak.Client.Services.Composition;

/// <summary>
/// Implementation of component state management service.
/// Provides composition-based state tracking instead of inheritance-based approach.
/// Thread-safe for concurrent component usage using lock-free ConcurrentDictionary.
/// </summary>
public class ComponentStateService : IComponentStateService
{
    private readonly ConcurrentDictionary<string, object?> lastParameterValues = new();
    private readonly IComponentPerformanceService performanceService;

    public ComponentStateService(IComponentPerformanceService performanceService)
    {
        this.performanceService = performanceService;
    }

    /// <summary>
    /// Captures current parameter values for change detection.
    /// </summary>
    public void CaptureParameterValues(ParameterView parameters)
    {
        lastParameterValues.Clear();

        foreach (var parameter in parameters)
        {
            lastParameterValues[parameter.Name] = parameter.Value;
        }
    }

    /// <summary>
    /// Checks if any parameters have changed since last capture.
    /// </summary>
    public bool HasParametersChanged()
    {
        // If no parameters were captured, assume change for first render
        if (lastParameterValues.IsEmpty)
        {
            return true;
        }

        // For now, this is a simplified implementation
        // In practice, components should provide custom change detection logic
        // This provides a conservative fallback that assumes changes may have occurred
        return true;
    }

    /// <summary>
    /// Checks if a specific parameter has changed and updates its tracked value.
    /// </summary>
    public bool HasParameterChanged<T>(string parameterName, T currentValue)
    {
        bool wasNewParameter = false;
        bool valueChanged = false;

        lastParameterValues.AddOrUpdate(
            parameterName,
            addValueFactory: (key) =>
            {
                wasNewParameter = true;
                return currentValue;
            },
            updateValueFactory: (key, existingValue) =>
            {
                valueChanged = !EqualityComparer<T>.Default.Equals((T)existingValue!, currentValue);
                return currentValue;
            }
        );

        return wasNewParameter || valueChanged;
    }

    /// <summary>
    /// Clears all tracked parameter values (useful for component disposal).
    /// </summary>
    public void ClearParameterTracking()
    {
        lastParameterValues.Clear();
    }

    /// <summary>
    /// Provides a simplified ShouldRender decision based on parameter optimization settings.
    /// </summary>
    public bool ShouldRender(bool enableParameterOptimization, Func<bool> hasStateChanged)
    {
        if (!enableParameterOptimization)
        {
            return true; // Always render if optimization disabled
        }

        return HasParametersChanged() || hasStateChanged();
    }

    /// <summary>
    /// Creates a debounced state change notifier.
    /// </summary>
    public Func<Task> CreateDebouncedNotifier(string componentName, Action stateChanged, int delayMs = 100)
    {
        return () => performanceService.DebounceAsync(
            $"{componentName}.StateChange",
            stateChanged,
            delayMs
        );
    }

    /// <summary>
    /// Creates a throttled state change notifier.
    /// </summary>
    public Func<Task<bool>> CreateThrottledNotifier(string componentName, Action stateChanged, int intervalMs = 50)
    {
        return () => performanceService.TryThrottleAsync(
            $"{componentName}.StateChange",
            stateChanged,
            intervalMs
        );
    }
}