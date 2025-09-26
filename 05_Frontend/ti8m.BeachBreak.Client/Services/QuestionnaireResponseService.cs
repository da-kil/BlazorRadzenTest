using ti8m.BeachBreak.Client.Models;
using System.Net.Http.Json;

namespace ti8m.BeachBreak.Client.Services;

public class QuestionnaireResponseService : BaseApiService, IQuestionnaireResponseService
{
    private const string ResponseQueryEndpoint = "q/api/v1/responses";
    private const string ResponseCommandEndpoint = "c/api/v1/responses";
    private const string AnalyticsEndpoint = "q/api/v1/analytics";

    public QuestionnaireResponseService(IHttpClientFactory factory) : base(factory)
    {
    }

    // Response CRUD operations
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

    public async Task<bool> DeleteResponseAsync(Guid id)
    {
        return await DeleteAsync(ResponseCommandEndpoint, id);
    }

    // Response management
    public async Task<QuestionnaireResponse> SaveResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses)
    {
        var result = await PostToSubPathAsync<Dictionary<Guid, SectionResponse>, QuestionnaireResponse>(ResponseCommandEndpoint, "assignment", assignmentId, sectionResponses);
        return result ?? throw new Exception("Failed to save response");
    }

    public async Task<QuestionnaireResponse?> SubmitResponseAsync(Guid assignmentId)
    {
        return await PostActionAsync<QuestionnaireResponse>(ResponseCommandEndpoint, "assignment", assignmentId, "submit");
    }

    // Response queries
    public async Task<List<QuestionnaireResponse>> GetResponsesByEmployeeAsync(string employeeId)
    {
        return await GetAllAsync<QuestionnaireResponse>($"{ResponseQueryEndpoint}/employee/{employeeId}");
    }

    public async Task<List<QuestionnaireResponse>> GetResponsesByTemplateAsync(Guid templateId)
    {
        return await GetAllAsync<QuestionnaireResponse>($"{ResponseQueryEndpoint}/template/{templateId}");
    }

    public async Task<List<QuestionnaireResponse>> GetResponsesByStatusAsync(ResponseStatus status)
    {
        return await GetAllAsync<QuestionnaireResponse>($"{ResponseQueryEndpoint}/status/{status}");
    }

    // Response analytics
    public async Task<Dictionary<string, object>> GetResponseAnalyticsAsync()
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<Dictionary<string, object>>($"{AnalyticsEndpoint}/responses") ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            LogError("Error fetching response analytics", ex);
            return new Dictionary<string, object>();
        }
    }

    public async Task<Dictionary<string, object>> GetEmployeeResponseStatsAsync(string employeeId)
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<Dictionary<string, object>>($"{AnalyticsEndpoint}/responses/employee/{employeeId}") ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            LogError($"Error fetching response stats for employee {employeeId}", ex);
            return new Dictionary<string, object>();
        }
    }

    public async Task<Dictionary<string, object>> GetTemplateResponseStatsAsync(Guid templateId)
    {
        return await GetBySubPathAsync<Dictionary<string, object>>(AnalyticsEndpoint, "responses/template", templateId) ?? new Dictionary<string, object>();
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
}