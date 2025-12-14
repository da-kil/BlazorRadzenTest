using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.DTOs;
using ti8m.BeachBreak.Client.Models.DTOs.Api;

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
        try
        {
            // API returns ApiQuestionnaireResponseDto directly (not wrapped in Result)
            var apiResponse = await HttpQueryClient.GetFromJsonAsync<ApiQuestionnaireResponseDto>($"{ResponseQueryEndpoint}/assignment/{assignmentId}", JsonOptions);

            if (apiResponse == null)
            {
                return null;
            }

            // Build QuestionnaireResponse with strongly-typed data
            var questionnaireResponse = new QuestionnaireResponse
            {
                AssignmentId = assignmentId,
                SectionResponses = new Dictionary<Guid, SectionResponse>()
            };

            // Map API structure to frontend structure (2-level: Section IS the question)
            foreach (var sectionKvp in apiResponse.SectionResponses)
            {
                var apiSection = sectionKvp.Value;
                var sectionResponse = new SectionResponse
                {
                    SectionId = apiSection.SectionId,
                    RoleResponses = new Dictionary<ResponseRole, QuestionResponse>()
                };

                // Map each role's response
                foreach (var roleKvp in apiSection.RoleResponses)
                {
                    var role = roleKvp.Key;
                    var apiQuestion = roleKvp.Value;

                    var questionResponse = new QuestionResponse
                    {
                        QuestionId = apiQuestion.QuestionId,
                        QuestionType = apiQuestion.QuestionType,
                        LastModified = apiQuestion.LastModified,
                        ResponseData = apiQuestion.ResponseData
                    };

                    sectionResponse.RoleResponses[role] = questionResponse;
                }

                questionnaireResponse.SectionResponses[sectionKvp.Key] = sectionResponse;
            }

            return questionnaireResponse;
        }
        catch (Exception ex)
        {
            LogError($"Error getting response for assignment {assignmentId}", ex);
            return null;
        }
    }

    public async Task<bool> DeleteResponseAsync(Guid id)
    {
        return await DeleteAsync(ResponseCommandEndpoint, id);
    }

    // Response management
    public async Task<QuestionnaireResponse> SaveResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses)
    {
        return await SaveResponseAsync(assignmentId, sectionResponses, templateId: null);
    }

    /// <summary>
    /// Saves response with templateId optimization for better performance.
    /// </summary>
    public async Task<QuestionnaireResponse> SaveResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses, Guid? templateId)
    {
        try
        {
            // Convert section-based responses to question-based DTO format expected by backend
            var dto = QuestionnaireResponseConverter.ConvertToSaveQuestionnaireResponseDto(sectionResponses, templateId);

            var response = await HttpCommandClient.PostAsJsonAsync($"{ResponseCommandEndpoint}/assignment/{assignmentId}", dto, JsonOptions);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();
            if (result?.Succeeded == true && result.Payload != default)
            {
                return new QuestionnaireResponse { Id = result.Payload, AssignmentId = assignmentId, SectionResponses = sectionResponses };
            }
            throw new Exception(result?.Message ?? "Failed to save response");
        }
        catch (Exception ex)
        {
            LogError($"Error saving response for assignment {assignmentId}", ex);
            throw;
        }
    }

    public async Task<QuestionnaireResponse> SaveManagerResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses)
    {
        return await SaveManagerResponseAsync(assignmentId, sectionResponses, templateId: null);
    }

    /// <summary>
    /// Saves manager response with templateId optimization for better performance.
    /// </summary>
    public async Task<QuestionnaireResponse> SaveManagerResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses, Guid? templateId)
    {
        try
        {
            // Convert section-based responses to question-based DTO format expected by backend
            var dto = QuestionnaireResponseConverter.ConvertToSaveQuestionnaireResponseDto(sectionResponses, templateId);

            var response = await HttpCommandClient.PostAsJsonAsync($"{ResponseCommandEndpoint}/manager/assignment/{assignmentId}", dto, JsonOptions);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();
            if (result?.Succeeded == true && result.Payload != default)
            {
                return new QuestionnaireResponse { Id = result.Payload, AssignmentId = assignmentId, SectionResponses = sectionResponses };
            }
            throw new Exception(result?.Message ?? "Failed to save manager response");
        }
        catch (Exception ex)
        {
            LogError($"Error saving manager response for assignment {assignmentId}", ex);
            throw;
        }
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