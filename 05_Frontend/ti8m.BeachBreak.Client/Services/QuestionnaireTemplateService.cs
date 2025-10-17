using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto;

namespace ti8m.BeachBreak.Client.Services;

public class QuestionnaireTemplateService : BaseApiService, IQuestionnaireTemplateService
{
    private const string TemplateQueryEndpoint = "q/api/v1/questionnaire-templates";
    private const string TemplateCommandEndpoint = "c/api/v1/questionnaire-templates";
    private const string AnalyticsEndpoint = "q/api/v1/analytics";
    private const string PublishedTemplateQueryEndpoint = "q/api/v1/questionnaire-templates/published";

    public QuestionnaireTemplateService(IHttpClientFactory factory) : base(factory)
    {
    }

    // Template CRUD operations
    public async Task<List<QuestionnaireTemplate>> GetAllTemplatesAsync()
    {
        return await GetAllAsync<QuestionnaireTemplate>(TemplateQueryEndpoint);
    }

    public async Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid id)
    {
        return await GetByIdAsync<QuestionnaireTemplate>(TemplateQueryEndpoint, id);
    }

    public async Task<QuestionnaireTemplate> CreateTemplateAsync(QuestionnaireTemplate template)
    {
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync(TemplateCommandEndpoint, template);

            if (response.IsSuccessStatusCode)
            {
                // The backend doesn't return the created template, so we refetch
                // Match by name and most recent creation date for better reliability
                var templates = await GetAllTemplatesAsync();
                var createdTemplate = templates
                    .Where(t => t.Name == template.Name)
                    .OrderByDescending(t => t.CreatedDate)
                    .FirstOrDefault();

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

    public async Task<QuestionnaireTemplate?> UpdateTemplateAsync(QuestionnaireTemplate template)
    {
        try
        {
            var response = await HttpCommandClient.PutAsJsonAsync($"{TemplateCommandEndpoint}/{template.Id}", template);

            if (response.IsSuccessStatusCode)
            {
                // Refetch the updated template from the Query API
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

    public async Task<bool> DeleteTemplateAsync(Guid id)
    {
        return await DeleteAsync(TemplateCommandEndpoint, id);
    }

    // Template filtering and queries
    public async Task<List<QuestionnaireTemplate>> GetTemplatesByCategoryAsync(string category)
    {
        return await GetAllAsync<QuestionnaireTemplate>($"{TemplateQueryEndpoint}/category/{category}");
    }

    public async Task<List<QuestionnaireTemplate>> GetPublishedTemplatesAsync()
    {
        return await GetAllAsync<QuestionnaireTemplate>($"{TemplateQueryEndpoint}/published");
    }

    public async Task<List<QuestionnaireTemplate>> GetDraftTemplatesAsync()
    {
        return await GetAllAsync<QuestionnaireTemplate>($"{TemplateQueryEndpoint}/drafts");
    }

    public async Task<List<QuestionnaireTemplate>> GetAssignableTemplatesAsync()
    {
        return await GetAllAsync<QuestionnaireTemplate>($"{TemplateQueryEndpoint}/assignable");
    }

    public async Task<List<QuestionnaireTemplate>> GetActiveTemplatesAsync()
    {
        try
        {
            var templates = await GetAllTemplatesAsync();
            return templates.Where(t => t.Status != TemplateStatus.Archived).ToList();
        }
        catch (Exception ex)
        {
            LogError("Error fetching active templates", ex);
            return new List<QuestionnaireTemplate>();
        }
    }

    public async Task<List<QuestionnaireTemplate>> GetArchivedTemplatesAsync()
    {
        try
        {
            var templates = await GetAllTemplatesAsync();
            return templates.Where(t => t.Status == TemplateStatus.Archived).ToList();
        }
        catch (Exception ex)
        {
            LogError("Error fetching archived templates", ex);
            return new List<QuestionnaireTemplate>();
        }
    }

    public async Task<List<QuestionnaireTemplate>> GetAllPublishedTemplatesAsync()
    {
        return await GetAllAsync<QuestionnaireTemplate>(PublishedTemplateQueryEndpoint);
    }

    // Template status operations
    public async Task<QuestionnaireTemplate?> PublishTemplateAsync(Guid templateId, string publishedBy)
    {
        return await PostActionAndRefetchAsync<string, QuestionnaireTemplate>(TemplateCommandEndpoint, templateId, "publish", publishedBy, TemplateQueryEndpoint);
    }

    public async Task<QuestionnaireTemplate?> UnpublishTemplateAsync(Guid templateId)
    {
        return await PostActionAndRefetchAsync<object, QuestionnaireTemplate>(TemplateCommandEndpoint, templateId, "unpublish", null, TemplateQueryEndpoint);
    }

    public async Task<QuestionnaireTemplate?> ArchiveTemplateAsync(Guid templateId)
    {
        return await PostActionAndRefetchAsync<object, QuestionnaireTemplate>(TemplateCommandEndpoint, templateId, "archive", null, TemplateQueryEndpoint);
    }

    public async Task<QuestionnaireTemplate?> RestoreTemplateAsync(Guid templateId)
    {
        return await PostActionAndRefetchAsync<object, QuestionnaireTemplate>(TemplateCommandEndpoint, templateId, "restore", null, TemplateQueryEndpoint);
    }

    // Template cloning
    public async Task<Guid?> CloneTemplateAsync(Guid templateId, string? namePrefix = null)
    {
        try
        {
            var requestDto = new CloneTemplateRequestDto { NamePrefix = namePrefix };

            var response = await HttpCommandClient.PostAsJsonAsync(
                $"{TemplateCommandEndpoint}/{templateId}/clone",
                requestDto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CloneTemplateResponseDto>();
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

    // Template analytics
    public async Task<Dictionary<string, object>> GetTemplateAnalyticsAsync(Guid templateId)
    {
        return await GetBySubPathAsync<Dictionary<string, object>>(AnalyticsEndpoint, "template", templateId) ?? new Dictionary<string, object>();
    }

    public async Task<Dictionary<string, object>> GetPublishingAnalyticsAsync()
    {
        try
        {
            var templates = await GetAllTemplatesAsync();
            var analytics = new Dictionary<string, object>
            {
                ["TotalTemplates"] = templates.Count,
                ["PublishedTemplates"] = templates.Count(t => t.Status == TemplateStatus.Published),
                ["DraftTemplates"] = templates.Count(t => t.Status == TemplateStatus.Draft),
                ["ArchivedTemplates"] = templates.Count(t => t.Status == TemplateStatus.Archived),
                ["AssignableTemplates"] = templates.Count(t => t.CanBeAssigned),
                ["PublishingRate"] = templates.Count > 0 ? (double)templates.Count(t => t.Status == TemplateStatus.Published) / templates.Count : 0.0
            };
            return analytics;
        }
        catch (Exception ex)
        {
            LogError("Error fetching publishing analytics", ex);
            return new Dictionary<string, object>();
        }
    }
}