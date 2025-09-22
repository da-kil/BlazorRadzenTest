using Microsoft.Extensions.Logging;
using Radzen;

namespace ti8m.BeachBreak.Client.Services.Enhanced;

/// <summary>
/// Global error handler for consistent user feedback and error processing
/// </summary>
public class GlobalErrorHandler
{
    private readonly ILogger<GlobalErrorHandler> _logger;
    private readonly NotificationService _notificationService;
    private readonly DialogService _dialogService;

    public GlobalErrorHandler(
        ILogger<GlobalErrorHandler> logger,
        NotificationService notificationService,
        DialogService dialogService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _dialogService = dialogService;
    }

    /// <summary>
    /// Handles API errors and provides appropriate user feedback
    /// </summary>
    public async Task HandleApiErrorAsync<T>(ApiResult<T> result, string operation)
    {
        if (result.IsSuccess) return;

        var errorInfo = ProcessError(result, operation);

        // Log the error
        _logger.LogError("API Error in {Operation}: {Error} (Code: {Code}, Status: {Status})",
            operation, errorInfo.Message, errorInfo.Code, errorInfo.HttpStatus);

        // Show user notification based on error type
        await ShowUserNotificationAsync(errorInfo);
    }

    /// <summary>
    /// Handles exceptions and provides appropriate user feedback
    /// </summary>
    public async Task HandleExceptionAsync(Exception exception, string operation)
    {
        var errorInfo = ProcessException(exception, operation);

        // Log the error
        _logger.LogError(exception, "Exception in {Operation}: {Error}", operation, errorInfo.Message);

        // Show user notification
        await ShowUserNotificationAsync(errorInfo);
    }

    /// <summary>
    /// Shows a confirmation dialog for destructive operations
    /// </summary>
    public async Task<bool> ConfirmDestructiveActionAsync(string title, string message)
    {
        try
        {
            var result = await _dialogService.Confirm(
                message,
                title,
                new ConfirmOptions
                {
                    OkButtonText = "Yes",
                    CancelButtonText = "No",
                    AutoFocusFirstElement = true
                });

            return result == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing confirmation dialog");
            return false;
        }
    }

    #region Private Methods

    private ErrorInfo ProcessError<T>(ApiResult<T> result, string operation)
    {
        return new ErrorInfo
        {
            Operation = operation,
            Message = result.ErrorMessage ?? "An unknown error occurred",
            Code = result.ErrorCode,
            HttpStatus = result.HttpStatusCode,
            Severity = DetermineErrorSeverity(result.HttpStatusCode),
            IsUserFacing = ShouldShowToUser(result.HttpStatusCode),
            Exception = result.Exception
        };
    }

    private ErrorInfo ProcessException(Exception exception, string operation)
    {
        var severity = exception switch
        {
            ArgumentException => ErrorSeverity.Warning,
            InvalidOperationException => ErrorSeverity.Warning,
            UnauthorizedAccessException => ErrorSeverity.Error,
            HttpRequestException => ErrorSeverity.Error,
            TaskCanceledException => ErrorSeverity.Information,
            _ => ErrorSeverity.Error
        };

        return new ErrorInfo
        {
            Operation = operation,
            Message = GetUserFriendlyMessage(exception),
            Code = exception.GetType().Name,
            Severity = severity,
            IsUserFacing = true,
            Exception = exception
        };
    }

    private ErrorSeverity DetermineErrorSeverity(int? httpStatusCode)
    {
        return httpStatusCode switch
        {
            400 => ErrorSeverity.Warning, // Bad Request
            401 => ErrorSeverity.Error,   // Unauthorized
            403 => ErrorSeverity.Error,   // Forbidden
            404 => ErrorSeverity.Information, // Not Found
            409 => ErrorSeverity.Warning, // Conflict
            422 => ErrorSeverity.Warning, // Unprocessable Entity
            429 => ErrorSeverity.Information, // Too Many Requests
            >= 500 => ErrorSeverity.Error, // Server Errors
            _ => ErrorSeverity.Warning
        };
    }

    private bool ShouldShowToUser(int? httpStatusCode)
    {
        // Don't show 404s for optional resources, or 401s (handled by auth system)
        return httpStatusCode switch
        {
            401 => false, // Handled by authentication system
            404 => false, // Usually handled contextually
            _ => true
        };
    }

    private string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentException => "Invalid input provided. Please check your data and try again.",
            InvalidOperationException => "This operation cannot be completed at this time.",
            UnauthorizedAccessException => "You don't have permission to perform this action.",
            HttpRequestException => "Unable to connect to the server. Please check your connection.",
            TaskCanceledException => "The operation timed out. Please try again.",
            _ => "An unexpected error occurred. Please try again or contact support."
        };
    }

    private async Task ShowUserNotificationAsync(ErrorInfo errorInfo)
    {
        if (!errorInfo.IsUserFacing) return;

        try
        {
            var notificationMessage = new NotificationMessage
            {
                Severity = MapToNotificationSeverity(errorInfo.Severity),
                Summary = GetNotificationTitle(errorInfo.Severity),
                Detail = errorInfo.Message,
                Duration = GetNotificationDuration(errorInfo.Severity)
            };

            _notificationService.Notify(notificationMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing user notification");
            // Fallback: try to show a simple console message
            Console.WriteLine($"Error: {errorInfo.Message}");
        }
    }

    private NotificationSeverity MapToNotificationSeverity(ErrorSeverity severity)
    {
        return severity switch
        {
            ErrorSeverity.Information => NotificationSeverity.Info,
            ErrorSeverity.Warning => NotificationSeverity.Warning,
            ErrorSeverity.Error => NotificationSeverity.Error,
            _ => NotificationSeverity.Warning
        };
    }

    private string GetNotificationTitle(ErrorSeverity severity)
    {
        return severity switch
        {
            ErrorSeverity.Information => "Information",
            ErrorSeverity.Warning => "Warning",
            ErrorSeverity.Error => "Error",
            _ => "Notification"
        };
    }

    private double GetNotificationDuration(ErrorSeverity severity)
    {
        return severity switch
        {
            ErrorSeverity.Information => 3000, // 3 seconds
            ErrorSeverity.Warning => 5000,     // 5 seconds
            ErrorSeverity.Error => 8000,       // 8 seconds
            _ => 4000
        };
    }

    #endregion

    #region Supporting Types

    private class ErrorInfo
    {
        public string Operation { get; set; } = "";
        public string Message { get; set; } = "";
        public string? Code { get; set; }
        public int? HttpStatus { get; set; }
        public ErrorSeverity Severity { get; set; }
        public bool IsUserFacing { get; set; }
        public Exception? Exception { get; set; }
    }

    private enum ErrorSeverity
    {
        Information,
        Warning,
        Error
    }

    #endregion
}