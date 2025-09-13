using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class CategoryApiService : ICategoryApiService
{
    private readonly HttpClient httpCommandClient;
    private readonly HttpClient httpQueryClient;

    public CategoryApiService(IHttpClientFactory Factory)
    {
        httpCommandClient = Factory.CreateClient("CommandClient");
        httpQueryClient = Factory.CreateClient("QueryClient");
    }

    public async Task<List<Category>> GetAllCategoriesAsync(bool includeInactive = false)
    {
        try
        {
            var queryString = includeInactive ? "?includeInactive=true" : "";
            var response = await httpQueryClient.GetFromJsonAsync<List<Category>>($"q/api/v1/categories{queryString}");
            return response ?? new List<Category>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching categories: {ex.Message}");
            return new List<Category>();
        }
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        try
        {
            return await httpQueryClient.GetFromJsonAsync<Category>($"q/api/v1/categories/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching category {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        try
        {
            var createRequest = new
            {
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                SortOrder = category.SortOrder
            };

            var response = await httpCommandClient.PostAsJsonAsync("c/api/v1/categories", createRequest);

            if (response.IsSuccessStatusCode)
            {
                // Return the created category with generated ID
                return category;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to create category: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating category: {ex.Message}");
            throw;
        }
    }

    public async Task<Category?> UpdateCategoryAsync(Category category)
    {
        try
        {
            var updateRequest = new
            {
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                SortOrder = category.SortOrder
            };

            var response = await httpCommandClient.PutAsJsonAsync($"c/api/v1/categories/{category.Id}", updateRequest);

            if (response.IsSuccessStatusCode)
            {
                return category;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to update category: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating category {category.Id}: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        try
        {
            var response = await httpCommandClient.DeleteAsync($"c/api/v1/categories/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting category {id}: {ex.Message}");
            return false;
        }
    }

    public async Task<List<string>> GetCategoryNamesAsync()
    {
        try
        {
            var categories = await GetAllCategoriesAsync();
            return categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .Select(c => c.Name)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching category names: {ex.Message}");
            return new List<string>();
        }
    }
}