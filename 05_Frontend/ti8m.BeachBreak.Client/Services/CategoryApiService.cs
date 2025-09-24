using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class CategoryApiService : BaseApiService, ICategoryApiService
{
    private const string QueryEndpoint = "q/api/v1/categories";
    private const string CommandEndpoint = "c/api/v1/categories";

    public CategoryApiService(IHttpClientFactory factory) : base(factory)
    {
    }

    public async Task<List<Category>> GetAllCategoriesAsync(bool includeInactive = false)
    {
        var queryString = includeInactive ? "includeInactive=true" : "";
        return await GetAllAsync<Category>(QueryEndpoint, queryString);
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await GetByIdAsync<Category>(QueryEndpoint, id);
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        return await CreateAsync<object, Category>(CommandEndpoint, category, category);
    }

    public async Task<Category?> UpdateCategoryAsync(Category category)
    {
        var updateRequest = new
        {
            NameEn = category.NameEn,
            NameDe = category.NameDe,
            DescriptionEn = category.DescriptionEn,
            DescriptionDe = category.DescriptionDe,
            IsActive = category.IsActive,
            SortOrder = category.SortOrder
        };

        return await UpdateAsync<object, Category>(CommandEndpoint, category.Id, updateRequest, category);
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        return await DeleteAsync(CommandEndpoint, id);
    }

    public async Task<List<string>> GetCategoryNamesAsync()
    {
        return await GetCategoryNamesAsync("en");
    }

    public async Task<List<string>> GetCategoryNamesAsync(string language = "en")
    {
        try
        {
            var categories = await GetAllCategoriesAsync();
            return categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.NameEn)
                .Select(c => language.ToLower() == "de" ? c.NameDe : c.NameEn)
                .ToList();
        }
        catch (Exception ex)
        {
            LogError("Error fetching category names", ex);
            return new List<string>();
        }
    }
}