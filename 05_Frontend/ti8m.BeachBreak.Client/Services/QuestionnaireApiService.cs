using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class QuestionnaireApiService : BaseApiService, IQuestionnaireApiService
{
    private const string TemplateQueryEndpoint = "q/api/v1/questionnaire-templates";
    private const string TemplateCommandEndpoint = "c/api/v1/questionnaire-templates";
    private const string AssignmentQueryEndpoint = "q/api/v1/assignments";
    private const string AssignmentCommandEndpoint = "c/api/v1/assignments";
    private const string ResponseQueryEndpoint = "q/api/v1/responses";
    private const string ResponseCommandEndpoint = "c/api/v1/responses";
    private const string AnalyticsEndpoint = "q/api/v1/analytics";

    public QuestionnaireApiService(IHttpClientFactory factory) : base(factory)
    {
    }

    // Template management
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
        var createRequest = new
        {
            template.Name,
            template.Description,
            template.Category,
            template.IsActive,
            template.IsPublished,
            template.PublishedDate,
            template.LastPublishedDate,
            template.PublishedBy,
            template.Sections,
            template.Settings
        };

        var result = await CreateWithResponseAsync<object, QuestionnaireTemplate>(TemplateCommandEndpoint, createRequest);
        return result ?? throw new Exception("Failed to create template");
    }

    public async Task<QuestionnaireTemplate?> UpdateTemplateAsync(QuestionnaireTemplate template)
    {
        var updateRequest = new
        {
            template.Name,
            template.Description,
            template.Category,
            template.IsActive,
            template.IsPublished,
            template.PublishedDate,
            template.LastPublishedDate,
            template.PublishedBy,
            template.Sections,
            template.Settings
        };

        return await UpdateWithResponseAsync<object, QuestionnaireTemplate>(TemplateCommandEndpoint, template.Id, updateRequest);
    }

    public async Task<bool> DeleteTemplateAsync(Guid id)
    {
        return await DeleteAsync(TemplateCommandEndpoint, id);
    }

    public async Task<List<QuestionnaireTemplate>> GetTemplatesByCategoryAsync(string category)
    {
        return await GetAllAsync<QuestionnaireTemplate>($"{TemplateQueryEndpoint}/category/{category}");
    }

    // Assignment management
    public async Task<List<QuestionnaireAssignment>> GetAllAssignmentsAsync()
    {
        return await GetAllAsync<QuestionnaireAssignment>(AssignmentQueryEndpoint);
    }

    public async Task<QuestionnaireAssignment?> GetAssignmentByIdAsync(Guid id)
    {
        return await GetByIdAsync<QuestionnaireAssignment>(AssignmentQueryEndpoint, id);
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByEmployeeAsync(string employeeId)
    {
        return await GetAllAsync<QuestionnaireAssignment>($"{AssignmentQueryEndpoint}/employee/{employeeId}");
    }

    public async Task<List<QuestionnaireAssignment>> CreateAssignmentsAsync(
        Guid templateId,
        List<string> employeeIds,
        DateTime? dueDate,
        string? notes,
        string assignedBy)
    {
        var createRequest = new
        {
            TemplateId = templateId,
            EmployeeIds = employeeIds,
            DueDate = dueDate,
            Notes = notes,
            AssignedBy = assignedBy
        };

        return await CreateWithListResponseAsync<object, QuestionnaireAssignment>(AssignmentCommandEndpoint, createRequest);
    }

    public async Task<QuestionnaireAssignment?> UpdateAssignmentStatusAsync(Guid id, AssignmentStatus status)
    {
        return await PatchAsync<AssignmentStatus, QuestionnaireAssignment>(AssignmentCommandEndpoint, id, "status", status);
    }

    public async Task<bool> DeleteAssignmentAsync(Guid id)
    {
        return await DeleteAsync(AssignmentCommandEndpoint, id);
    }

    // Response management
    public async Task<List<QuestionnaireResponse>> GetAllResponsesAsync()
    {
        return await GetAllAsync<QuestionnaireResponse>(ResponseQueryEndpoint);
    }

    public async Task<QuestionnaireResponse?> GetResponseByIdAsync(Guid id)
    {
        return await GetByIdAsync<QuestionnaireResponse>(ResponseQueryEndpoint, id);
    }

    public async Task<QuestionnaireResponse?> GetResponseByAssignmentIdAsync(Guid assignmentId)
    {
        return await GetBySubPathAsync<QuestionnaireResponse>(ResponseQueryEndpoint, "assignment", assignmentId);
    }

    public async Task<QuestionnaireResponse> SaveResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses)
    {
        var result = await PostToSubPathAsync<Dictionary<Guid, SectionResponse>, QuestionnaireResponse>(ResponseCommandEndpoint, "assignment", assignmentId, sectionResponses);
        return result ?? throw new Exception("Failed to save response");
    }

    public async Task<QuestionnaireResponse?> SubmitResponseAsync(Guid assignmentId)
    {
        return await PostActionAsync<QuestionnaireResponse>(ResponseCommandEndpoint, "assignment", assignmentId, "submit");
    }

    // Analytics
    public async Task<Dictionary<string, object>> GetTemplateAnalyticsAsync(Guid templateId)
    {
        return await GetBySubPathAsync<Dictionary<string, object>>(AnalyticsEndpoint, "template", templateId) ?? new Dictionary<string, object>();
    }

    public async Task<Dictionary<string, object>> GetOverallAnalyticsAsync()
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<Dictionary<string, object>>($"{AnalyticsEndpoint}/overview") ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            LogError("Error fetching overall analytics", ex);
            return new Dictionary<string, object>();
        }
    }

    // Enhanced status-specific template queries
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
            return templates.Where(t => t.IsActive).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching active templates: {ex.Message}");
            return new List<QuestionnaireTemplate>();
        }
    }

    public async Task<List<QuestionnaireTemplate>> GetInactiveTemplatesAsync()
    {
        try
        {
            var templates = await GetAllTemplatesAsync();
            return templates.Where(t => !t.IsActive).ToList();
        }
        catch (Exception ex)
        {
            LogError("Error fetching inactive templates", ex);
            return new List<QuestionnaireTemplate>();
        }
    }

    // Publishing operations
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

    public async Task<Dictionary<string, object>> GetPublishingAnalyticsAsync()
    {
        try
        {
            var templates = await GetAllTemplatesAsync();
            var analytics = new Dictionary<string, object>
            {
                ["TotalTemplates"] = templates.Count,
                ["PublishedTemplates"] = templates.Count(t => t.IsPublished),
                ["DraftTemplates"] = templates.Count(t => t.Status == TemplateStatus.Draft),
                ["InactiveTemplates"] = templates.Count(t => !t.IsActive),
                ["AssignableTemplates"] = templates.Count(t => t.CanBeAssigned),
                ["PublishingRate"] = templates.Count > 0 ? (double)templates.Count(t => t.IsPublished) / templates.Count : 0.0
            };
            return analytics;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching publishing analytics: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }
}