using System.Net.Http.Json;
using System.Text.Json;
using ti8m.BeachBreak.Client.Converters;

namespace ti8m.BeachBreak.Client.Services;

public abstract class BaseApiService
{
    protected readonly HttpClient HttpCommandClient;
    protected readonly HttpClient HttpQueryClient;

    /// <summary>
    /// JSON serialization options configured for PascalCase consistency with backend
    /// </summary>
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null, // PascalCase
        PropertyNameCaseInsensitive = false, // Strict enforcement
        Converters = { new QuestionResponseDataDtoJsonConverter() }
    };

    protected BaseApiService(IHttpClientFactory factory)
    {
        HttpCommandClient = factory.CreateClient("CommandClient");
        HttpQueryClient = factory.CreateClient("QueryClient");
    }

    protected async Task<List<T>> GetAllAsync<T>(string endpoint)
    {
        try
        {
            var response = await HttpQueryClient.GetFromJsonAsync<List<T>>(endpoint);
            return response ?? new List<T>();
        }
        catch (Exception ex)
        {
            LogError($"Error fetching from {endpoint}", ex);
            return new List<T>();
        }
    }

    protected async Task<List<T>> GetAllAsync<T>(string endpoint, string queryString)
    {
        try
        {
            var fullEndpoint = string.IsNullOrEmpty(queryString) ? endpoint : $"{endpoint}?{queryString}";
            var response = await HttpQueryClient.GetFromJsonAsync<List<T>>(fullEndpoint);
            return response ?? new List<T>();
        }
        catch (Exception ex)
        {
            LogError($"Error fetching from {endpoint}", ex);
            return new List<T>();
        }
    }

    protected async Task<T?> GetByIdAsync<T>(string endpoint, Guid id)
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<T>($"{endpoint}/{id}");
        }
        catch (Exception ex)
        {
            LogError($"Error fetching {typeof(T).Name} with id {id}", ex);
            return default(T);
        }
    }

    protected async Task<TResult> CreateAsync<TRequest, TResult>(string endpoint, TRequest request, TResult fallbackResult)
    {
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync(endpoint, request, JsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return fallbackResult;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to create {typeof(TResult).Name}: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            LogError($"Error creating {typeof(TResult).Name}", ex);
            throw;
        }
    }

    protected async Task<TResult?> UpdateAsync<TRequest, TResult>(string endpoint, Guid id, TRequest request, TResult fallbackResult)
    {
        try
        {
            var response = await HttpCommandClient.PutAsJsonAsync($"{endpoint}/{id}", request, JsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return fallbackResult;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to update {typeof(TResult).Name}: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            LogError($"Error updating {typeof(TResult).Name} with id {id}", ex);
            return default(TResult);
        }
    }

    protected async Task<bool> DeleteAsync(string endpoint, Guid id)
    {
        try
        {
            var response = await HttpCommandClient.DeleteAsync($"{endpoint}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error deleting from {endpoint} with id {id}", ex);
            return false;
        }
    }

    protected async Task<List<T>> SearchAsync<T>(string endpoint, string searchTerm)
    {
        try
        {
            var queryString = $"search={Uri.EscapeDataString(searchTerm)}";
            return await GetAllAsync<T>(endpoint, queryString);
        }
        catch (Exception ex)
        {
            LogError($"Error searching {endpoint} with term '{searchTerm}'", ex);
            return new List<T>();
        }
    }




    // Patch method for partial updates
    protected async Task<TResult?> PatchAsync<TRequest, TResult>(string endpoint, Guid id, string subPath, TRequest request)
    {
        try
        {
            var response = await HttpCommandClient.PatchAsJsonAsync($"{endpoint}/{id}/{subPath}", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TResult>();
        }
        catch (Exception ex)
        {
            LogError($"Error patching {typeof(TResult).Name} with id {id}", ex);
            return default(TResult);
        }
    }

    // GET with sub-path (e.g., /api/resources/assignment/123)
    protected async Task<T?> GetBySubPathAsync<T>(string endpoint, string subPath, Guid id)
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<T>($"{endpoint}/{subPath}/{id}");
        }
        catch (Exception ex)
        {
            LogError($"Error fetching {typeof(T).Name} with {subPath} {id}", ex);
            return default(T);
        }
    }

    // POST with sub-path and return response (e.g., /api/responses/assignment/123)
    protected async Task<TResult?> PostToSubPathAsync<TRequest, TResult>(string endpoint, string subPath, Guid id, TRequest request)
    {
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync($"{endpoint}/{subPath}/{id}", request, JsonOptions);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TResult>();
            return result ?? throw new Exception($"Failed to deserialize {typeof(TResult).Name}");
        }
        catch (Exception ex)
        {
            LogError($"Error posting to {endpoint}/{subPath}/{id}", ex);
            throw;
        }
    }

    // POST with sub-path and action (e.g., /api/responses/assignment/123/submit)
    protected async Task<TResult?> PostActionAsync<TResult>(string endpoint, string subPath, Guid id, string action)
    {
        try
        {
            var response = await HttpCommandClient.PostAsync($"{endpoint}/{subPath}/{id}/{action}", null);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TResult>();
        }
        catch (Exception ex)
        {
            LogError($"Error executing {action} for {endpoint}/{subPath}/{id}", ex);
            return default(TResult);
        }
    }

    // POST action and refetch entity (e.g., /api/templates/123/publish then GET /api/templates/123)
    protected async Task<TResult?> PostActionAndRefetchAsync<TRequest, TResult>(string commandEndpoint, Guid id, string action, TRequest? requestBody, string queryEndpoint)
    {
        try
        {
            HttpResponseMessage response;
            if (requestBody != null)
            {
                response = await HttpCommandClient.PostAsJsonAsync($"{commandEndpoint}/{id}/{action}", requestBody, JsonOptions);
            }
            else
            {
                response = await HttpCommandClient.PostAsync($"{commandEndpoint}/{id}/{action}", null);
            }

            if (response.IsSuccessStatusCode)
            {
                return await GetByIdAsync<TResult>(queryEndpoint, id);
            }
            return default(TResult);
        }
        catch (Exception ex)
        {
            LogError($"Error executing {action} for {typeof(TResult).Name} {id}", ex);
            return default(TResult);
        }
    }




    // HR/Organization-wide endpoints (e.g., /api/hr/employees)
    protected async Task<List<T>> GetHRResourceAsync<T>(string hrEndpoint, string resource)
    {
        try
        {
            var response = await HttpQueryClient.GetFromJsonAsync<List<T>>($"{hrEndpoint}/{resource}");
            return response ?? new List<T>();
        }
        catch (Exception ex)
        {
            LogError($"Error fetching {typeof(T).Name} from HR/{resource}", ex);
            return new List<T>();
        }
    }

    // HR single resource (e.g., /api/hr/analytics/organization)
    protected async Task<T?> GetHRSingleResourceAsync<T>(string hrEndpoint, string resource)
    {
        try
        {
            var response = await HttpQueryClient.GetFromJsonAsync<T>($"{hrEndpoint}/{resource}");
            return response ?? default(T);
        }
        catch (Exception ex)
        {
            LogError($"Error fetching {typeof(T).Name} from HR/{resource}", ex);
            return default(T);
        }
    }

    // HR with sub-path (e.g., /api/hr/assignments/department/IT)
    protected async Task<List<T>> GetHRResourceWithSubPathAsync<T>(string hrEndpoint, string resource, string subPath, string identifier)
    {
        try
        {
            var response = await HttpQueryClient.GetFromJsonAsync<List<T>>($"{hrEndpoint}/{resource}/{subPath}/{Uri.EscapeDataString(identifier)}");
            return response ?? new List<T>();
        }
        catch (Exception ex)
        {
            LogError($"Error fetching {typeof(T).Name} from HR/{resource}/{subPath}/{identifier}", ex);
            return new List<T>();
        }
    }

    // Employee-specific endpoints (e.g., /api/employees/123/assignments)
    protected async Task<List<T>> GetEmployeeResourceAsync<T>(string endpoint, string employeeId, string resource)
    {
        try
        {
            var response = await HttpQueryClient.GetFromJsonAsync<List<T>>($"{endpoint}/{employeeId}/{resource}");
            return response ?? new List<T>();
        }
        catch (Exception ex)
        {
            LogError($"Error fetching {typeof(T).Name} for employee {employeeId}/{resource}", ex);
            return new List<T>();
        }
    }

    // Employee-specific single resource with path (e.g., /api/employees/123/assignments/456)
    protected async Task<T?> GetEmployeeSubResourceAsync<T>(string endpoint, string employeeId, string resource, Guid subResourceId)
    {
        try
        {
            var response = await HttpQueryClient.GetFromJsonAsync<T>($"{endpoint}/{employeeId}/{resource}/{subResourceId}");
            return response ?? default(T);
        }
        catch (Exception ex)
        {
            LogError($"Error fetching {typeof(T).Name} for employee {employeeId}/{resource}/{subResourceId}", ex);
            return default(T);
        }
    }



    protected void LogError(string message, Exception? ex)
    {
        if (ex != null)
        {
            Console.WriteLine($"{message}: {ex.Message}");
        }
        else
        {
            Console.WriteLine(message);
        }
    }
}