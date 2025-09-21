using Microsoft.AspNetCore.Components;
using System.Timers;

namespace ti8m.BeachBreak.Client.Components.Shared
{
    /// <summary>
    /// Base class for components that need auto-save functionality
    /// Provides common auto-save behavior that can be inherited by any component
    /// </summary>
    public abstract class AutoSaveComponentBase : ComponentBase, IDisposable
    {
        [Parameter] public int AutoSaveDelayMs { get; set; } = 2000;

        protected bool ShowAutoSave = false;
        private System.Threading.Timer? autoSaveTimer;

        /// <summary>
        /// Shows the auto-save indicator and hides it after the specified delay
        /// </summary>
        protected void ShowAutoSaveIndicator()
        {
            ShowAutoSave = true;
            StateHasChanged();

            // Clear existing timer
            autoSaveTimer?.Dispose();

            // Set new timer to hide indicator after specified delay
            autoSaveTimer = new System.Threading.Timer(_ => {
                ShowAutoSave = false;
                InvokeAsync(StateHasChanged);
            }, null, AutoSaveDelayMs, Timeout.Infinite);
        }

        /// <summary>
        /// Updates a value with an EventCallback and triggers auto-save indicator
        /// </summary>
        protected async Task UpdateWithAutoSave<T>(T value, EventCallback<T> callback)
        {
            await callback.InvokeAsync(value);
            ShowAutoSaveIndicator();
        }

        /// <summary>
        /// Updates multiple values and triggers auto-save indicator once
        /// </summary>
        protected async Task UpdateMultipleWithAutoSave(params Func<Task>[] updates)
        {
            foreach (var update in updates)
            {
                await update();
            }
            ShowAutoSaveIndicator();
        }

        public virtual void Dispose()
        {
            autoSaveTimer?.Dispose();
        }
    }
}