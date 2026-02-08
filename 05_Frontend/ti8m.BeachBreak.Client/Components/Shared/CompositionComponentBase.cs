using Microsoft.AspNetCore.Components;
using ti8m.BeachBreak.Client.Services.Composition;

namespace ti8m.BeachBreak.Client.Components.Shared;

/// <summary>
/// Lightweight component base class using composition instead of complex inheritance.
/// Provides essential functionality through injected services rather than bloated base class methods.
///
/// Performance Benefits:
/// - Reduced inheritance complexity (from 522 lines to ~80 lines)
/// - Easier to test and debug
/// - More flexible - components can choose which services to use
/// - Better separation of concerns
/// </summary>
public abstract class CompositionComponentBase : ComponentBase, IDisposable
{
    [Inject] protected IComponentPerformanceService PerformanceService { get; set; } = default!;
    [Inject] protected IComponentStateService StateService { get; set; } = default!;
    [Inject] protected IComponentErrorService ErrorService { get; set; } = default!;

    private bool disposed;

    /// <summary>
    /// Controls whether the component should perform parameter optimization.
    /// Default is true.
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
    /// The name used for performance tracking and error handling.
    /// Defaults to component type name.
    /// </summary>
    protected virtual string ComponentName => GetType().Name;

    /// <summary>
    /// Optimized ShouldRender implementation using composition-based state service.
    /// </summary>
    protected override bool ShouldRender()
    {
        var shouldRender = StateService.ShouldRender(EnableParameterOptimization, HasStateChanged);

        if (EnablePerformanceMonitoring && !shouldRender)
        {
            Console.WriteLine($"[{ComponentName}] Render skipped - no parameter or state changes");
        }

        return shouldRender;
    }

    /// <summary>
    /// Override to track parameter changes for optimization.
    /// </summary>
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        if (EnableParameterOptimization)
        {
            StateService.CaptureParameterValues(parameters);
        }

        await base.SetParametersAsync(parameters);
    }

    /// <summary>
    /// Override this method to provide custom state change detection.
    /// </summary>
    protected virtual bool HasStateChanged()
    {
        // Default implementation assumes no state change
        // Derived classes should override this to check their specific state
        return false;
    }

    /// <summary>
    /// Method to explicitly trigger a state change notification.
    /// </summary>
    protected void NotifyStateChanged()
    {
        StateHasChanged();
    }

    /// <summary>
    /// Cleanup resources.
    /// </summary>
    public virtual void Dispose()
    {
        if (disposed) return;

        StateService.ClearParameterTracking();
        disposed = true;
    }

    /// <summary>
    /// Checks if component is disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }
}