using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class TranslationApiService : BaseApiService, ITranslationApiService
{
    private const string QueryEndpoint = "q/api/v1/translations";
    private const string CommandEndpoint = "c/api/v1/translations";

    public TranslationApiService(IHttpClientFactory factory) : base(factory)
    {
    }

    public async Task<List<UITranslation>> GetAllTranslationsAsync()
    {
        return await GetAllAsync<UITranslation>(QueryEndpoint);
    }

    public async Task<UITranslation?> GetTranslationByKeyAsync(string key)
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<UITranslation>($"{QueryEndpoint}/{key}");
        }
        catch (Exception ex)
        {
            LogError($"Error fetching translation with key {key}", ex);
            return null;
        }
    }

    public async Task<bool> UpsertTranslationAsync(string key, string german, string english, string? category)
    {
        try
        {
            var request = new
            {
                Key = key,
                German = german,
                English = english,
                Category = category
            };

            var response = await HttpCommandClient.PostAsJsonAsync(CommandEndpoint, request, JsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error upserting translation for key {key}", ex);
            return false;
        }
    }

    public async Task<bool> DeleteTranslationAsync(string key)
    {
        try
        {
            var response = await HttpCommandClient.DeleteAsync($"{CommandEndpoint}/{key}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error deleting translation with key {key}", ex);
            return false;
        }
    }

    public async Task<bool> InvalidateCacheAsync()
    {
        try
        {
            var response = await HttpCommandClient.PostAsync($"{CommandEndpoint}/invalidate-cache", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError("Error invalidating translation cache", ex);
            return false;
        }
    }
}
