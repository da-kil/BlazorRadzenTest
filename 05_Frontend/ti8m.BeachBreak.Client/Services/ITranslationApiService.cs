using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public interface ITranslationApiService
{
    Task<List<UITranslation>> GetAllTranslationsAsync();
    Task<UITranslation?> GetTranslationByKeyAsync(string key);
    Task<bool> UpsertTranslationAsync(string key, string german, string english, string? category);
    Task<bool> DeleteTranslationAsync(string key);
    Task<bool> InvalidateCacheAsync();
}
