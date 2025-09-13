using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface ICategoryApiService
{
    Task<List<Category>> GetAllCategoriesAsync(bool includeInactive = false);
    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category?> UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(Guid id);
    Task<List<string>> GetCategoryNamesAsync();
}