using ti8m.BeachBreak.Client.Models;

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
        var result = await CreateWithResponseAsync<object, QuestionnaireTemplate>(TemplateCommandEndpoint, template);
        return result ?? throw new Exception("Failed to create template");
    }

    public async Task<QuestionnaireTemplate?> UpdateTemplateAsync(QuestionnaireTemplate template)
    {
        return await UpdateWithResponseAsync<object, QuestionnaireTemplate>(TemplateCommandEndpoint, template.Id, template);
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

    public async Task<List<QuestionnaireTemplate>> GetInactiveTemplatesAsync()
    {
        try
        {
            var templates = await GetAllTemplatesAsync();
            return templates.Where(t => t.Status == TemplateStatus.Archived).ToList();
        }
        catch (Exception ex)
        {
            LogError("Error fetching inactive templates", ex);
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

    public async Task<QuestionnaireTemplate?> ActivateTemplateAsync(Guid templateId)
    {
        return await PostActionAndRefetchAsync<object, QuestionnaireTemplate>(TemplateCommandEndpoint, templateId, "activate", null, TemplateQueryEndpoint);
    }

    public async Task<QuestionnaireTemplate?> DeactivateTemplateAsync(Guid templateId)
    {
        return await PostActionAndRefetchAsync<object, QuestionnaireTemplate>(TemplateCommandEndpoint, templateId, "deactivate", null, TemplateQueryEndpoint);
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