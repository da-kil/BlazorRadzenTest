namespace ti8m.BeachBreak.Client.Services.Enhanced;

/// <summary>
/// Represents the result of an API operation with success/error states
/// </summary>
public class ApiResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public Exception? Exception { get; init; }
    public int? HttpStatusCode { get; init; }

    protected ApiResult() { }

    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    public static ApiResult<T> Success(T data)
    {
        return new ApiResult<T>
        {
            IsSuccess = true,
            Data = data
        };
    }

    /// <summary>
    /// Creates a successful result with no data
    /// </summary>
    public static ApiResult<T> Success()
    {
        return new ApiResult<T>
        {
            IsSuccess = true,
            Data = default(T)
        };
    }

    /// <summary>
    /// Creates a failed result with error information
    /// </summary>
    public static ApiResult<T> Failure(string errorMessage, string? errorCode = null, Exception? exception = null, int? httpStatusCode = null)
    {
        return new ApiResult<T>
        {
            IsSuccess = false,
            Data = default(T),
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            Exception = exception,
            HttpStatusCode = httpStatusCode
        };
    }

    /// <summary>
    /// Creates a failed result from an HTTP response
    /// </summary>
    public static ApiResult<T> HttpFailure(int statusCode, string reasonPhrase, string? errorContent = null)
    {
        var message = $"HTTP {statusCode}: {reasonPhrase}";
        if (!string.IsNullOrEmpty(errorContent))
        {
            message += $" - {errorContent}";
        }

        return new ApiResult<T>
        {
            IsSuccess = false,
            Data = default(T),
            ErrorMessage = message,
            ErrorCode = statusCode.ToString(),
            HttpStatusCode = statusCode
        };
    }

    /// <summary>
    /// Creates a failed result from an exception
    /// </summary>
    public static ApiResult<T> ExceptionFailure(Exception exception, string? customMessage = null)
    {
        return new ApiResult<T>
        {
            IsSuccess = false,
            Data = default(T),
            ErrorMessage = customMessage ?? exception.Message,
            Exception = exception
        };
    }

    /// <summary>
    /// Returns the data if successful, otherwise throws an exception
    /// </summary>
    public T ThrowIfFailed()
    {
        if (!IsSuccess)
        {
            var message = ErrorMessage ?? "API operation failed";
            throw Exception ?? new InvalidOperationException(message);
        }
        return Data!;
    }

    /// <summary>
    /// Returns the data if successful, otherwise returns the default value
    /// </summary>
    public T? GetDataOrDefault()
    {
        return IsSuccess ? Data : default(T);
    }

    /// <summary>
    /// Returns the data if successful, otherwise returns the fallback value
    /// </summary>
    public T GetDataOr(T fallback)
    {
        return IsSuccess && Data != null ? Data : fallback;
    }
}

/// <summary>
/// Non-generic version for operations that don't return data
/// </summary>
public class ApiResult : ApiResult<object>
{
    /// <summary>
    /// Creates a successful result with no data
    /// </summary>
    public static new ApiResult Success()
    {
        return new ApiResult
        {
            IsSuccess = true
        };
    }

    /// <summary>
    /// Creates a failed result with error information
    /// </summary>
    public static new ApiResult Failure(string errorMessage, string? errorCode = null, Exception? exception = null, int? httpStatusCode = null)
    {
        return new ApiResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            Exception = exception,
            HttpStatusCode = httpStatusCode
        };
    }
}