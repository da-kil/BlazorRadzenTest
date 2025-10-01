using Microsoft.AspNetCore.Components;
using Radzen;
using ti8m.BeachBreak.Client.Services;

namespace ti8m.BeachBreak.Client.Components.Shared;

public abstract class BasePageComponent : ComponentBase
{
    [Inject] protected IEmployeeQuestionnaireService EmployeeQuestionnaireService { get; set; } = default!;
    [Inject] protected IManagerQuestionnaireService ManagerQuestionnaireService { get; set; } = default!;
    [Inject] protected IHRQuestionnaireService HRQuestionnaireService { get; set; } = default!;
    [Inject] protected ICategoryApiService CategoryApiService { get; set; } = default!;
    [Inject] protected IEmployeeApiService EmployeeApiService { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected DialogService DialogService { get; set; } = default!;
    [Inject] protected NotificationService NotificationService { get; set; } = default!;

    protected bool isLoading = false;
    protected string errorMessage = "";

    protected virtual void ShowError(string message)
    {
        errorMessage = message;
        NotificationService.Notify(NotificationSeverity.Error, "Error", message);
        StateHasChanged();
    }

    protected virtual void ShowSuccess(string message)
    {
        NotificationService.Notify(NotificationSeverity.Success, "Success", message);
    }

    protected virtual void ShowInfo(string message)
    {
        NotificationService.Notify(NotificationSeverity.Info, "Information", message);
    }

    protected virtual void ShowWarning(string message)
    {
        NotificationService.Notify(NotificationSeverity.Warning, "Warning", message);
    }

    protected virtual async Task HandleError(Exception ex, string operation)
    {
        isLoading = false;
        var message = $"Error during {operation}: {ex.Message}";
        ShowError(message);
        Console.WriteLine($"[{GetType().Name}] {message}");
        await Task.CompletedTask;
    }

    protected virtual void SetLoading(bool loading)
    {
        isLoading = loading;
        StateHasChanged();
    }

    protected virtual void NavigateToPage(string url)
    {
        NavigationManager.NavigateTo(url);
    }

    protected virtual async Task<bool> ShowConfirmDialog(string title, string message, string confirmText = "Confirm", string cancelText = "Cancel")
    {
        var result = await DialogService.Confirm(message, title, new ConfirmOptions
        {
            OkButtonText = confirmText,
            CancelButtonText = cancelText
        });

        return result ?? false;
    }
}