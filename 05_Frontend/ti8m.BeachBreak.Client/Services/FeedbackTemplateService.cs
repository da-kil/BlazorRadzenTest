using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service implementation for managing feedback templates
/// Follows CQRS pattern with separate command and query endpoints
/// </summary>
public class FeedbackTemplateService : BaseApiService, IFeedbackTemplateService
{
    private const string TemplateCommandEndpoint = "c/api/v1/employee-feedbacks/templates";
    private const string TemplateQueryEndpoint = "q/api/v1/employee-feedbacks/templates";

    public FeedbackTemplateService(IHttpClientFactory factory) : base(factory)
    {
    }

    /// <inheritdoc/>
    public async Task<List<FeedbackTemplate>> GetMyTemplatesAsync()
    {
        return await GetAllAsync<FeedbackTemplate>(TemplateQueryEndpoint);
    }

    /// <inheritdoc/>
    public async Task<FeedbackTemplate?> GetTemplateByIdAsync(Guid id)
    {
        return await GetByIdAsync<FeedbackTemplate>(TemplateQueryEndpoint, id);
    }

    /// <inheritdoc/>
    public async Task<List<FeedbackTemplate>> GetTemplatesBySourceTypeAsync(FeedbackSourceType sourceType)
    {
        return await GetAllAsync<FeedbackTemplate>($"{TemplateQueryEndpoint}/by-source/{(int)sourceType}");
    }

    /// <inheritdoc/>
    public async Task<FeedbackTemplate> CreateTemplateAsync(FeedbackTemplate template)
    {
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync(TemplateCommandEndpoint, template, JsonOptions);

            if (response.IsSuccessStatusCode)
            {
                // CQRS pattern: Command succeeds, then refetch from query side
                var createdTemplate = await GetTemplateByIdAsync(template.Id);
                return createdTemplate ?? throw new Exception("Failed to retrieve created template");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            LogError($"Failed to create template: {response.StatusCode}", new Exception(errorContent));
            throw new Exception($"Failed to create template: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            LogError("Error creating template", ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<FeedbackTemplate?> UpdateTemplateAsync(FeedbackTemplate template)
    {
        try
        {
            var response = await HttpCommandClient.PutAsJsonAsync($"{TemplateCommandEndpoint}/{template.Id}", template, JsonOptions);

            if (response.IsSuccessStatusCode)
            {
                // CQRS pattern: Refetch from query side after update
                return await GetTemplateByIdAsync(template.Id);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            LogError($"Failed to update template {template.Id}: {response.StatusCode}", new Exception(errorContent));
            return null;
        }
        catch (Exception ex)
        {
            LogError($"Error updating template {template.Id}", ex);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<FeedbackTemplate?> PublishTemplateAsync(Guid templateId)
    {
        return await PostActionAndRefetchAsync<object, FeedbackTemplate>(
            TemplateCommandEndpoint,
            templateId,
            "publish",
            null,
            TemplateQueryEndpoint);
    }

    /// <inheritdoc/>
    public async Task<bool> ArchiveTemplateAsync(Guid templateId)
    {
        try
        {
            var response = await HttpCommandClient.PostAsync($"{TemplateCommandEndpoint}/{templateId}/archive", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error archiving template {templateId}", ex);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteTemplateAsync(Guid templateId)
    {
        return await DeleteAsync(TemplateCommandEndpoint, templateId);
    }

    /// <inheritdoc/>
    public async Task<Guid?> CloneTemplateAsync(Guid templateId, string? namePrefix = null)
    {
        try
        {
            var requestDto = new { NamePrefix = namePrefix ?? "Copy of " };

            var response = await HttpCommandClient.PostAsJsonAsync(
                $"{TemplateCommandEndpoint}/{templateId}/clone",
                requestDto,
                JsonOptions);

            if (response.IsSuccessStatusCode)
            {
                // Backend returns { NewTemplateId: Guid }
                var result = await response.Content.ReadFromJsonAsync<CloneTemplateResponse>(JsonOptions);
                return result?.NewTemplateId;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            LogError($"Failed to clone template {templateId}: {response.StatusCode}", new Exception(errorContent));
            return null;
        }
        catch (Exception ex)
        {
            LogError($"Error cloning template {templateId}", ex);
            return null;
        }
    }

    /// <summary>
    /// Response DTO for clone template operation
    /// </summary>
    private class CloneTemplateResponse
    {
        public Guid NewTemplateId { get; set; }
    }
}
