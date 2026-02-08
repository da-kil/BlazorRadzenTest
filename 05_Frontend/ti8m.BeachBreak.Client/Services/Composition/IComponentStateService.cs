using Microsoft.AspNetCore.Components;

namespace ti8m.BeachBreak.Client.Services.Composition;

/// <summary>
/// Service for managing component state changes and parameter optimization.
/// Extracted from the complex component inheritance hierarchy for composition-based design.
/// </summary>
public interface IComponentStateService
{
    /// <summary>
    /// Captures current parameter values for change detection.
    /// </summary>
    void CaptureParameterValues(ParameterView parameters);

    /// <summary>
    /// Checks if any parameters have changed since last capture.
    /// </summary>
    bool HasParametersChanged();

    /// <summary>
    /// Checks if a specific parameter has changed and updates its tracked value.
    /// </summary>
    bool HasParameterChanged<T>(string parameterName, T currentValue);

    /// <summary>
    /// Clears all tracked parameter values (useful for component disposal).
    /// </summary>
    void ClearParameterTracking();

    /// <summary>
    /// Provides a simplified ShouldRender decision based on parameter optimization settings.
    /// </summary>
    bool ShouldRender(bool enableParameterOptimization, Func<bool> hasStateChanged);

    /// <summary>
    /// Creates a debounced state change notifier.
    /// </summary>
    Func<Task> CreateDebouncedNotifier(string componentName, Action stateChanged, int delayMs = 100);

    /// <summary>
    /// Creates a throttled state change notifier.
    /// </summary>
    Func<Task<bool>> CreateThrottledNotifier(string componentName, Action stateChanged, int intervalMs = 50);
}