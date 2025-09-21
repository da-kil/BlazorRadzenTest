using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace ti8m.BeachBreak.Client.Services.Enhanced;

/// <summary>
/// Modernized version of BaseApiService with enhanced error handling and retry logic
/// Maintains backward compatibility while providing improved reliability
/// </summary>
public abstract class ModernBaseApiService : EnhancedApiService
{
    protected ModernBaseApiService(IHttpClientFactory factory, ILogger<EnhancedApiService> logger, ApiServiceOptions? options = null)
        : base(factory, logger, options)
    {
    }

    #region Legacy Compatibility Methods

    /// <summary>
    /// Legacy method that returns data directly (throws on error)
    /// Maintained for backward compatibility
    /// </summary>
    protected new async Task<List<T>> GetAllAsync<T>(string endpoint)
    {
        var result = await base.GetAllAsync<T>(endpoint);
        return result.ThrowIfFailed();
    }

    /// <summary>
    /// Legacy method that returns data directly (throws on error)
    /// </summary>
    protected new async Task<List<T>> GetAllAsync<T>(string endpoint, string queryString)
    {
        var result = await base.GetAllAsync<T>(endpoint, queryString);
        return result.ThrowIfFailed();
    }

    /// <summary>
    /// Legacy method that returns data or null
    /// </summary>
    protected new async Task<T?> GetByIdAsync<T>(string endpoint, Guid id)
    {
        var result = await base.GetByIdAsync<T>(endpoint, id);
        return result.GetDataOrDefault();
    }

    /// <summary>
    /// Legacy create method with fallback result
    /// </summary>
    protected async Task<TResult> CreateAsync<TRequest, TResult>(string endpoint, TRequest request, TResult fallbackResult)
    {
        var result = await base.CreateAsync<TRequest, TResult>(endpoint, request);
        return result.GetDataOr(fallbackResult);
    }

    /// <summary>
    /// Legacy update method with fallback result
    /// </summary>
    protected async Task<TResult?> UpdateAsync<TRequest, TResult>(string endpoint, Guid id, TRequest request, TResult fallbackResult)
    {
        var result = await base.UpdateAsync<TRequest, TResult>(endpoint, id, request);
        return result.IsSuccess ? result.Data : fallbackResult;
    }

    /// <summary>
    /// Legacy delete method returning bool
    /// </summary>
    protected new async Task<bool> DeleteAsync(string endpoint, Guid id)
    {
        var result = await base.DeleteAsync(endpoint, id);
        return result.GetDataOr(false);
    }

    #endregion

    #region Enhanced Methods with ApiResult

    /// <summary>
    /// Enhanced create method that returns ApiResult for better error handling
    /// </summary>
    protected async Task<ApiResult<TResult>> CreateWithResultAsync<TRequest, TResult>(string endpoint, TRequest request)
    {
        return await base.CreateAsync<TRequest, TResult>(endpoint, request);
    }

    /// <summary>
    /// Enhanced update method that returns ApiResult
    /// </summary>
    protected async Task<ApiResult<TResult>> UpdateWithResultAsync<TRequest, TResult>(string endpoint, Guid id, TRequest request)
    {
        return await base.UpdateAsync<TRequest, TResult>(endpoint, id, request);
    }

    /// <summary>
    /// Enhanced delete method that returns ApiResult
    /// </summary>
    protected async Task<ApiResult<bool>> DeleteWithResultAsync(string endpoint, Guid id)
    {
        return await base.DeleteAsync(endpoint, id);
    }

    /// <summary>
    /// Enhanced GET all method that returns ApiResult
    /// </summary>
    protected async Task<ApiResult<List<T>>> GetAllWithResultAsync<T>(string endpoint)
    {
        return await base.GetAllAsync<T>(endpoint);
    }

    /// <summary>
    /// Enhanced GET by ID method that returns ApiResult
    /// </summary>
    protected async Task<ApiResult<T>> GetByIdWithResultAsync<T>(string endpoint, Guid id)
    {
        return await base.GetByIdAsync<T>(endpoint, id);
    }

    #endregion

    #region Search and Specialized Methods

    /// <summary>
    /// Search entities with enhanced error handling
    /// </summary>
    protected async Task<List<T>> SearchAsync<T>(string endpoint, string searchTerm)
    {
        try
        {
            var queryString = $"search={Uri.EscapeDataString(searchTerm)}";
            var result = await base.GetAllAsync<T>(endpoint, queryString);
            return result.ThrowIfFailed();
        }
        catch (Exception ex)
        {
            LogError($"searching {endpoint} with term '{searchTerm}'", ex);
            return new List<T>();
        }
    }

    /// <summary>
    /// Create with response - enhanced version
    /// </summary>
    protected async Task<TResult?> CreateWithResponseAsync<TRequest, TResult>(string endpoint, TRequest request)
    {
        var result = await base.CreateAsync<TRequest, TResult>(endpoint, request);
        if (result.IsSuccess)
        {
            return result.Data;
        }

        // For backward compatibility, throw on error
        throw new HttpRequestException(result.ErrorMessage ?? "Create operation failed");
    }

    /// <summary>
    /// Update with response - enhanced version
    /// </summary>
    protected async Task<TResult?> UpdateWithResponseAsync<TRequest, TResult>(string endpoint, Guid id, TRequest request)
    {
        var result = await base.UpdateAsync<TRequest, TResult>(endpoint, id, request);
        if (result.IsSuccess)
        {
            return result.Data;
        }

        // For backward compatibility, throw on error
        throw new HttpRequestException(result.ErrorMessage ?? "Update operation failed");
    }

    /// <summary>
    /// Create with list response
    /// </summary>
    protected async Task<List<TResult>> CreateWithListResponseAsync<TRequest, TResult>(string endpoint, TRequest request)
    {
        try
        {
            var result = await ExecuteWithRetryWrapper(async () =>
            {
                var response = await HttpCommandClient.PostAsJsonAsync(endpoint, request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<TResult>>();
            }, $"POST {endpoint} (list response)");

            return result ?? new List<TResult>();
        }
        catch (Exception ex)
        {
            LogError($"creating {typeof(TResult).Name} list", ex);
            return new List<TResult>();
        }
    }

    #endregion

    #region Domain-Specific Helper Methods

    /// <summary>
    /// Manager-specific resource access with enhanced error handling
    /// </summary>
    protected async Task<List<T>> GetManagerResourceAsync<T>(string endpoint, string managerId, string resource)
    {
        var fullEndpoint = $"{endpoint}/{managerId}/{resource}";
        var result = await base.GetAllAsync<T>(fullEndpoint);
        return result.GetDataOr(new List<T>());
    }

    /// <summary>
    /// Manager-specific single resource access
    /// </summary>
    protected async Task<T?> GetManagerSingleResourceAsync<T>(string endpoint, string managerId, string resource)
    {
        try
        {
            var fullEndpoint = $"{endpoint}/{managerId}/{resource}";
            var result = await ExecuteWithRetryWrapper(async () =>
            {
                return await HttpQueryClient.GetFromJsonAsync<T>(fullEndpoint);
            }, $"GET {fullEndpoint}");

            return result;
        }
        catch (Exception ex)
        {
            LogError($"fetching {typeof(T).Name} for manager {managerId}/{resource}", ex);
            return default(T);
        }
    }

    /// <summary>
    /// HR resource access with enhanced error handling
    /// </summary>
    protected async Task<List<T>> GetHRResourceAsync<T>(string hrEndpoint, string resource)
    {
        var fullEndpoint = $"{hrEndpoint}/{resource}";
        var result = await base.GetAllAsync<T>(fullEndpoint);
        return result.GetDataOr(new List<T>());
    }

    /// <summary>
    /// Employee-specific resource access
    /// </summary>
    protected async Task<List<T>> GetEmployeeResourceAsync<T>(string endpoint, string employeeId, string resource)
    {
        var fullEndpoint = $"{endpoint}/{employeeId}/{resource}";
        var result = await base.GetAllAsync<T>(fullEndpoint);
        return result.GetDataOr(new List<T>());
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Executes an operation with retry logic (wrapper for simple operations)
    /// </summary>
    private async Task<T?> ExecuteWithRetryWrapper<T>(Func<Task<T?>> operation, string operationName)
    {
        var wrappedOperation = async () =>
        {
            var result = await operation();
            return ApiResult<T>.Success(result!);
        };

        var result = await ExecuteWithRetryAsync(wrappedOperation, operationName);
        return result.GetDataOrDefault();
    }

    /// <summary>
    /// Legacy error logging for backward compatibility
    /// </summary>
    protected void LogError(string message, Exception ex)
    {
        base.LogError(message, ex);
    }

    #endregion
}