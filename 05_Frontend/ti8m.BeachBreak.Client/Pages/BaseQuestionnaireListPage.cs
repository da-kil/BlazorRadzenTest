using Microsoft.AspNetCore.Components;
using Radzen;
using ti8m.BeachBreak.Client.Components.Shared;
using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Services;

namespace ti8m.BeachBreak.Client.Pages;

/// <summary>
/// Generic base component for all questionnaire list pages.
/// Eliminates code duplication across Employee, Manager, and HR views.
/// Follows Template Method pattern with role-specific customization points.
/// </summary>
public abstract class BaseQuestionnaireListPage : OptimizedComponentBase
{
    [Inject] protected NotificationService NotificationService { get; set; } = default!;
    [Inject] protected ICategoryApiService CategoryService { get; set; } = default!;

    protected QuestionnairePageConfiguration? configuration;
    protected IQuestionnaireDataService? dataService;
    protected List<Category> categories = new();
    protected bool isLoading = false;

    protected override async Task OnInitializedAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            await LoadInitialData();
        }, GetInitializationContext());

        SetupConfiguration();
    }

    protected override bool HasStateChanged()
    {
        return HasParameterChanged(nameof(configuration), configuration) ||
               HasParameterChanged(nameof(isLoading), isLoading) ||
               HasAdditionalStateChanged();
    }

    /// <summary>
    /// Main data loading orchestration method.
    /// Loads categories and delegates role-specific loading to derived classes.
    /// </summary>
    private async Task LoadInitialData()
    {
        SetLoading(true);

        try
        {
            // Load categories in parallel with role-specific data
            var categoriesTask = CategoryService.GetAllCategoriesAsync();
            var roleDataTask = LoadRoleSpecificDataAsync();

            await Task.WhenAll(categoriesTask, roleDataTask);

            categories = categoriesTask.Result;
        }
        catch (Exception ex)
        {
            HandleError(ex, "loading data");
        }
        finally
        {
            SetLoading(false);
        }
    }

    /// <summary>
    /// Configuration setup orchestration method.
    /// Creates configuration via factory and wires up action handlers.
    /// </summary>
    private void SetupConfiguration()
    {
        configuration = CreateConfiguration();
        ConfigureActions();
    }

    #region Abstract Methods - Must be implemented by derived classes

    /// <summary>
    /// Load role-specific data (assignments, employees, templates, etc.).
    /// Called in parallel with category loading.
    /// </summary>
    protected abstract Task LoadRoleSpecificDataAsync();

    /// <summary>
    /// Create the page configuration using the appropriate factory method.
    /// </summary>
    protected abstract QuestionnairePageConfiguration CreateConfiguration();

    /// <summary>
    /// Get the context name for error logging/tracking.
    /// </summary>
    protected abstract string GetInitializationContext();

    #endregion

    #region Virtual Methods - Can be overridden for customization

    /// <summary>
    /// Configure action button click handlers.
    /// Override to add role-specific action handlers.
    /// </summary>
    protected virtual void ConfigureActions()
    {
        // Default implementation - derived classes can override
    }

    /// <summary>
    /// Check for additional state changes beyond common properties.
    /// Override to add role-specific state tracking.
    /// </summary>
    protected virtual bool HasAdditionalStateChanged()
    {
        return false;
    }

    /// <summary>
    /// Handle data refresh request.
    /// Override to implement custom refresh logic.
    /// </summary>
    protected virtual async Task RefreshData()
    {
        await LoadInitialData();
        SetupConfiguration();
        NotifyStateChanged();
    }

    #endregion

    #region Protected Utility Methods

    protected void SetLoading(bool loading)
    {
        isLoading = loading;
        NotifyStateChanged();
    }

    protected void HandleError(Exception ex, string context)
    {
        NotificationService.Notify(NotificationSeverity.Error, "Error", $"Failed {context}: {ex.Message}");
    }

    protected void ShowInfo(string message)
    {
        NotificationService.Notify(NotificationSeverity.Info, "Information", message);
    }

    protected void ShowSuccess(string message)
    {
        NotificationService.Notify(NotificationSeverity.Success, "Success", message);
    }

    protected void ShowWarning(string message)
    {
        NotificationService.Notify(NotificationSeverity.Warning, "Warning", message);
    }

    #endregion
}
