using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ti8m.BeachBreak.Client.Services.Enhanced;

/// <summary>
/// Enhanced API service with retry logic, structured error handling, and improved logging
/// </summary>
public abstract class EnhancedApiService
{
    protected readonly HttpClient HttpCommandClient;
    protected readonly HttpClient HttpQueryClient;
    protected readonly ILogger<EnhancedApiService> Logger;
    protected readonly ApiServiceOptions Options;

    protected EnhancedApiService(IHttpClientFactory factory, ILogger<EnhancedApiService> logger, ApiServiceOptions? options = null)
    {
        HttpCommandClient = factory.CreateClient("CommandClient");
        HttpQueryClient = factory.CreateClient("QueryClient");
        Logger = logger;
        Options = options ?? new ApiServiceOptions();

        ConfigureHttpClients();
    }

    private void ConfigureHttpClients()
    {
        var timeout = TimeSpan.FromSeconds(Options.RequestTimeoutSeconds);
        HttpCommandClient.Timeout = timeout;
        HttpQueryClient.Timeout = timeout;
    }

    #region Enhanced HTTP Methods with Retry Logic

    /// <summary>
    /// GET all entities with retry logic and structured error handling
    /// </summary>
    protected async Task<ApiResult<List<T>>> GetAllAsync<T>(string endpoint)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await HttpQueryClient.GetFromJsonAsync<List<T>>(endpoint);
            return ApiResult<List<T>>.Success(response ?? new List<T>());
        }, $"GET {endpoint}");
    }

    /// <summary>
    /// GET all entities with query parameters
    /// </summary>
    protected async Task<ApiResult<List<T>>> GetAllAsync<T>(string endpoint, string queryString)
    {
        var fullEndpoint = string.IsNullOrEmpty(queryString) ? endpoint : $"{endpoint}?{queryString}";
        return await GetAllAsync<T>(fullEndpoint);
    }

    /// <summary>
    /// GET entity by ID with retry logic
    /// </summary>
    protected async Task<ApiResult<T>> GetByIdAsync<T>(string endpoint, Guid id)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await HttpQueryClient.GetFromJsonAsync<T>($"{endpoint}/{id}");
            return response != null
                ? ApiResult<T>.Success(response)
                : ApiResult<T>.Failure($"{typeof(T).Name} with ID {id} not found", "NOT_FOUND");
        }, $"GET {endpoint}/{id}");
    }

    /// <summary>
    /// POST create entity with enhanced error handling
    /// </summary>
    protected async Task<ApiResult<TResult>> CreateAsync<TRequest, TResult>(string endpoint, TRequest request)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await HttpCommandClient.PostAsJsonAsync(endpoint, request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return ApiResult<TResult>.HttpFailure((int)response.StatusCode, response.ReasonPhrase ?? "Unknown error", errorContent);
            }

            var result = await response.Content.ReadFromJsonAsync<TResult>();
            return result != null
                ? ApiResult<TResult>.Success(result)
                : ApiResult<TResult>.Failure($"Failed to deserialize created {typeof(TResult).Name}");
        }, $"POST {endpoint}");
    }

    /// <summary>
    /// PUT update entity with enhanced error handling
    /// </summary>
    protected async Task<ApiResult<TResult>> UpdateAsync<TRequest, TResult>(string endpoint, Guid id, TRequest request)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await HttpCommandClient.PutAsJsonAsync($"{endpoint}/{id}", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return ApiResult<TResult>.HttpFailure((int)response.StatusCode, response.ReasonPhrase ?? "Unknown error", errorContent);
            }

            var result = await response.Content.ReadFromJsonAsync<TResult>();
            return result != null
                ? ApiResult<TResult>.Success(result)
                : ApiResult<TResult>.Failure($"Failed to deserialize updated {typeof(TResult).Name}");
        }, $"PUT {endpoint}/{id}");
    }

    /// <summary>
    /// DELETE entity with enhanced error handling
    /// </summary>
    protected async Task<ApiResult<bool>> DeleteAsync(string endpoint, Guid id)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await HttpCommandClient.DeleteAsync($"{endpoint}/{id}");

            if (response.IsSuccessStatusCode)
            {
                return ApiResult<bool>.Success(true);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return ApiResult<bool>.HttpFailure((int)response.StatusCode, response.ReasonPhrase ?? "Unknown error", errorContent);
        }, $"DELETE {endpoint}/{id}");
    }

    /// <summary>
    /// PATCH entity with enhanced error handling
    /// </summary>
    protected async Task<ApiResult<TResult>> PatchAsync<TRequest, TResult>(string endpoint, Guid id, string subPath, TRequest request)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var response = await HttpCommandClient.PatchAsJsonAsync($"{endpoint}/{id}/{subPath}", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return ApiResult<TResult>.HttpFailure((int)response.StatusCode, response.ReasonPhrase ?? "Unknown error", errorContent);
            }

            var result = await response.Content.ReadFromJsonAsync<TResult>();
            return result != null
                ? ApiResult<TResult>.Success(result)
                : ApiResult<TResult>.Failure($"Failed to deserialize patched {typeof(TResult).Name}");
        }, $"PATCH {endpoint}/{id}/{subPath}");
    }

    #endregion

    #region Retry Logic Implementation

    /// <summary>
    /// Executes an API operation with retry logic
    /// </summary>
    protected async Task<ApiResult<T>> ExecuteWithRetryAsync<T>(Func<Task<ApiResult<T>>> operation, string operationName)
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt <= Options.MaxRetryAttempts)
        {
            try
            {
                if (Options.EnableDetailedLogging && attempt > 0)
                {
                    Logger.LogInformation("Retry attempt {Attempt} for {Operation}", attempt, operationName);
                }

                var result = await operation();

                // If successful or non-retryable error, return immediately
                if (result.IsSuccess || !ShouldRetry(result.HttpStatusCode))
                {
                    if (Options.EnableDetailedLogging && attempt > 0)
                    {
                        Logger.LogInformation("Operation {Operation} succeeded after {Attempt} retries", operationName, attempt);
                    }
                    return result;
                }

                // If this was the last attempt, return the failed result
                if (attempt >= Options.MaxRetryAttempts)
                {
                    Logger.LogWarning("Operation {Operation} failed after {MaxAttempts} attempts. Final error: {Error}",
                        operationName, Options.MaxRetryAttempts, result.ErrorMessage);
                    return result;
                }
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                Logger.LogWarning(ex, "HTTP request exception on attempt {Attempt} for {Operation}", attempt + 1, operationName);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                lastException = ex;
                Logger.LogWarning(ex, "Request timeout on attempt {Attempt} for {Operation}", attempt + 1, operationName);
            }
            catch (Exception ex)
            {
                // For unexpected exceptions, don't retry
                Logger.LogError(ex, "Unexpected exception during {Operation}", operationName);
                return ApiResult<T>.ExceptionFailure(ex);
            }

            attempt++;

            // Wait before retrying
            if (attempt <= Options.MaxRetryAttempts)
            {
                var delay = CalculateRetryDelay(attempt);
                Logger.LogInformation("Waiting {Delay}ms before retry attempt {Attempt} for {Operation}", delay, attempt, operationName);
                await Task.Delay(delay);
            }
        }

        // If we get here, all retries failed with exceptions
        var message = $"Operation {operationName} failed after {Options.MaxRetryAttempts} retry attempts";
        Logger.LogError(lastException, message);
        return ApiResult<T>.ExceptionFailure(lastException ?? new InvalidOperationException(message), message);
    }

    /// <summary>
    /// Determines if a request should be retried based on HTTP status code
    /// </summary>
    private bool ShouldRetry(int? httpStatusCode)
    {
        if (httpStatusCode == null) return true; // Retry on network errors
        return Options.RetryableStatusCodes.Contains((HttpStatusCode)httpStatusCode.Value);
    }

    /// <summary>
    /// Calculates retry delay with optional exponential backoff
    /// </summary>
    private int CalculateRetryDelay(int attemptNumber)
    {
        if (!Options.UseExponentialBackoff)
        {
            return Options.RetryDelayMs;
        }

        // Exponential backoff: delay * (2 ^ (attempt - 1))
        var delay = Options.RetryDelayMs * Math.Pow(2, attemptNumber - 1);
        return (int)Math.Min(delay, 30000); // Cap at 30 seconds
    }

    #endregion

    #region Logging Helpers

    /// <summary>
    /// Logs successful operations when detailed logging is enabled
    /// </summary>
    protected void LogSuccess(string operation, object? data = null)
    {
        if (Options.EnableDetailedLogging)
        {
            Logger.LogDebug("Successfully completed {Operation}", operation);
        }
    }

    /// <summary>
    /// Logs operation failures
    /// </summary>
    protected void LogError(string operation, Exception exception)
    {
        Logger.LogError(exception, "Failed to complete {Operation}: {Error}", operation, exception.Message);
    }

    /// <summary>
    /// Logs operation warnings
    /// </summary>
    protected void LogWarning(string operation, string message)
    {
        Logger.LogWarning("Warning during {Operation}: {Message}", operation, message);
    }

    #endregion
}