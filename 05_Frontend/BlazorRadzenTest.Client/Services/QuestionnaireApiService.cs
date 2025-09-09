using System.Net.Http.Json;
using System.Text.Json;
using BlazorRadzenTest.Client.Models;

namespace BlazorRadzenTest.Client.Services;

public class QuestionnaireApiService : IQuestionnaireApiService
{
    private readonly HttpClient _httpClient;

    public QuestionnaireApiService(IHttpClientFactory Factory)
    {
        _httpClient = Factory.CreateClient("ApiClient");
    }

    // Template management
    public async Task<List<QuestionnaireTemplate>> GetAllTemplatesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<QuestionnaireTemplate>>("api/QuestionnaireTemplates");
            return response ?? new List<QuestionnaireTemplate>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching templates: {ex.Message}");
            return new List<QuestionnaireTemplate>();
        }
    }

    public async Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<QuestionnaireTemplate>($"api/QuestionnaireTemplates/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching template {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<QuestionnaireTemplate> CreateTemplateAsync(QuestionnaireTemplate template)
    {
        try
        {
            var createRequest = new
            {
                Name = template.Name,
                Description = template.Description,
                Category = template.Category,
                Sections = template.Sections,
                Settings = template.Settings
            };

            var response = await _httpClient.PostAsJsonAsync("api/questionnaireTemplates", createRequest);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<QuestionnaireTemplate>();
            return result ?? throw new Exception("Failed to create template");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating template: {ex.Message}");
            throw;
        }
    }

    public async Task<QuestionnaireTemplate?> UpdateTemplateAsync(QuestionnaireTemplate template)
    {
        try
        {
            var updateRequest = new
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                Category = template.Category,
                Sections = template.Sections,
                Settings = template.Settings
            };

            var response = await _httpClient.PutAsJsonAsync("api/questionnaireTemplates/{template.Id}", updateRequest);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<QuestionnaireTemplate>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating template {template.Id}: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteTemplateAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync("api/questionnaireTemplates/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting template {id}: {ex.Message}");
            return false;
        }
    }

    public async Task<List<QuestionnaireTemplate>> GetTemplatesByCategoryAsync(string category)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<QuestionnaireTemplate>>("api/questionnaireTemplates/category/{category}");
            return response ?? new List<QuestionnaireTemplate>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching templates by category {category}: {ex.Message}");
            return new List<QuestionnaireTemplate>();
        }
    }

    // Assignment management
    public async Task<List<QuestionnaireAssignment>> GetAllAssignmentsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<QuestionnaireAssignment>>("api/assignments");
            return response ?? new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching assignments: {ex.Message}");
            return new List<QuestionnaireAssignment>();
        }
    }

    public async Task<QuestionnaireAssignment?> GetAssignmentByIdAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<QuestionnaireAssignment>("api/assignments/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching assignment {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByEmployeeAsync(string employeeId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<QuestionnaireAssignment>>("api/assignments/employee/{employeeId}");
            return response ?? new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching assignments for employee {employeeId}: {ex.Message}");
            return new List<QuestionnaireAssignment>();
        }
    }

    public async Task<List<QuestionnaireAssignment>> CreateAssignmentsAsync(
        Guid templateId, 
        List<string> employeeIds, 
        DateTime? dueDate, 
        string? notes, 
        string assignedBy)
    {
        try
        {
            var createRequest = new
            {
                TemplateId = templateId,
                EmployeeIds = employeeIds,
                DueDate = dueDate,
                Notes = notes,
                AssignedBy = assignedBy
            };

            var response = await _httpClient.PostAsJsonAsync("api/assignments", createRequest);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<List<QuestionnaireAssignment>>();
            return result ?? new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating assignments: {ex.Message}");
            return new List<QuestionnaireAssignment>();
        }
    }

    public async Task<QuestionnaireAssignment?> UpdateAssignmentStatusAsync(Guid id, AssignmentStatus status)
    {
        try
        {
            var response = await _httpClient.PatchAsJsonAsync("api/assignments/{id}/status", status);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating assignment status {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteAssignmentAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync("api/assignments/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting assignment {id}: {ex.Message}");
            return false;
        }
    }

    // Response management
    public async Task<List<QuestionnaireResponse>> GetAllResponsesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<QuestionnaireResponse>>("api/responses");
            return response ?? new List<QuestionnaireResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching responses: {ex.Message}");
            return new List<QuestionnaireResponse>();
        }
    }

    public async Task<QuestionnaireResponse?> GetResponseByIdAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<QuestionnaireResponse>("api/responses/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching response {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<QuestionnaireResponse?> GetResponseByAssignmentIdAsync(Guid assignmentId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<QuestionnaireResponse>("api/responses/assignment/{assignmentId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching response for assignment {assignmentId}: {ex.Message}");
            return null;
        }
    }

    public async Task<QuestionnaireResponse> SaveResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/responses/assignment/{assignmentId}", sectionResponses);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<QuestionnaireResponse>();
            return result ?? throw new Exception("Failed to save response");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving response for assignment {assignmentId}: {ex.Message}");
            throw;
        }
    }

    public async Task<QuestionnaireResponse?> SubmitResponseAsync(Guid assignmentId)
    {
        try
        {
            var response = await _httpClient.PostAsync("api/responses/assignment/{assignmentId}/submit", null);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<QuestionnaireResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error submitting response for assignment {assignmentId}: {ex.Message}");
            return null;
        }
    }

    // Analytics
    public async Task<Dictionary<string, object>> GetTemplateAnalyticsAsync(Guid templateId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Dictionary<string, object>>("api/analytics/template/{templateId}");
            return response ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching template analytics {templateId}: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    public async Task<Dictionary<string, object>> GetOverallAnalyticsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Dictionary<string, object>>("api/analytics/overview");
            return response ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching overall analytics: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }
}