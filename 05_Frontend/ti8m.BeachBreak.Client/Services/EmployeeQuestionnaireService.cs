using System.Net.Http.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class EmployeeQuestionnaireService : IEmployeeQuestionnaireService
{
    private readonly HttpClient httpQueryClient;
    private readonly HttpClient httpCommandClient;
    private readonly string currentEmployeeId;

    public EmployeeQuestionnaireService(IHttpClientFactory factory)
    {
        httpQueryClient = factory.CreateClient("QueryClient");
        httpCommandClient = factory.CreateClient("CommandClient");
        // TODO: Get current employee ID from authentication context
        currentEmployeeId = "b0f388c2-6294-4116-a8b2-eccafa29b3fb";
    }

    public async Task<List<QuestionnaireAssignment>> GetMyAssignmentsAsync()
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<QuestionnaireAssignment>>($"q/api/v1/employees/{currentEmployeeId}/assignments");
            return response ?? new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching my assignments: {ex.Message}");
            return new List<QuestionnaireAssignment>();
        }
    }

    public async Task<QuestionnaireAssignment?> GetMyAssignmentByIdAsync(Guid assignmentId)
    {
        try
        {
            return await httpQueryClient.GetFromJsonAsync<QuestionnaireAssignment>($"q/api/v1/employees/{currentEmployeeId}/assignments/{assignmentId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching assignment {assignmentId}: {ex.Message}");
            return null;
        }
    }

    public async Task<QuestionnaireResponse?> GetMyResponseAsync(Guid assignmentId)
    {
        try
        {
            return await httpQueryClient.GetFromJsonAsync<QuestionnaireResponse>($"q/api/v1/employees/{currentEmployeeId}/responses/assignment/{assignmentId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching my response for assignment {assignmentId}: {ex.Message}");
            return null;
        }
    }

    public async Task<QuestionnaireResponse> SaveMyResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses)
    {
        try
        {
            var response = await httpCommandClient.PostAsJsonAsync($"c/api/v1/employees/{currentEmployeeId}/responses/assignment/{assignmentId}", sectionResponses);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<QuestionnaireResponse>();
            return result ?? throw new Exception("Failed to save response");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving my response for assignment {assignmentId}: {ex.Message}");
            throw;
        }
    }

    public async Task<QuestionnaireResponse?> SubmitMyResponseAsync(Guid assignmentId)
    {
        try
        {
            var response = await httpCommandClient.PostAsync($"c/api/v1/employees/{currentEmployeeId}/responses/assignment/{assignmentId}/submit", null);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<QuestionnaireResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error submitting my response for assignment {assignmentId}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByStatusAsync(AssignmentStatus status)
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<QuestionnaireAssignment>>($"q/api/v1/employees/{currentEmployeeId}/assignments?status={status}");
            return response ?? new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching assignments by status {status}: {ex.Message}");
            return new List<QuestionnaireAssignment>();
        }
    }

    public async Task<AssignmentProgress> GetAssignmentProgressAsync(Guid assignmentId)
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<AssignmentProgress>($"q/api/v1/employees/{currentEmployeeId}/assignments/{assignmentId}/progress");
            return response ?? new AssignmentProgress { AssignmentId = assignmentId };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching assignment progress {assignmentId}: {ex.Message}");
            return new AssignmentProgress { AssignmentId = assignmentId };
        }
    }

    public async Task<List<AssignmentProgress>> GetAllAssignmentProgressAsync()
    {
        try
        {
            var response = await httpQueryClient.GetFromJsonAsync<List<AssignmentProgress>>($"q/api/v1/employees/{currentEmployeeId}/assignments/progress");
            return response ?? new List<AssignmentProgress>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching all assignment progress: {ex.Message}");
            return new List<AssignmentProgress>();
        }
    }
}