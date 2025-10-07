using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto;
using System.Net.Http.Json;

namespace ti8m.BeachBreak.Client.Services;

public class QuestionnaireAssignmentService : BaseApiService, IQuestionnaireAssignmentService
{
    private const string AssignmentQueryEndpoint = "q/api/v1/assignments";
    private const string AssignmentCommandEndpoint = "c/api/v1/assignments";
    private const string AnalyticsEndpoint = "q/api/v1/analytics";

    public QuestionnaireAssignmentService(IHttpClientFactory factory) : base(factory)
    {
    }

    // Assignment CRUD operations
    public async Task<List<QuestionnaireAssignment>> GetAllAssignmentsAsync()
    {
        return await GetAllAsync<QuestionnaireAssignment>(AssignmentQueryEndpoint);
    }

    public async Task<QuestionnaireAssignment?> GetAssignmentByIdAsync(Guid id)
    {
        return await GetByIdAsync<QuestionnaireAssignment>(AssignmentQueryEndpoint, id);
    }

    public async Task<bool> DeleteAssignmentAsync(Guid id)
    {
        return await DeleteAsync(AssignmentCommandEndpoint, id);
    }

    // Assignment creation and management
    public async Task<List<QuestionnaireAssignment>> CreateAssignmentsAsync(
        Guid templateId,
        List<EmployeeDto> employees,
        DateTime? dueDate,
        string? notes,
        string assignedBy)
    {
        var employeeAssignments = employees.Select(emp => new EmployeeAssignmentDto
        {
            EmployeeId = emp.Id,
            EmployeeName = emp.FullName,
            EmployeeEmail = emp.EMail
        }).ToList();

        var createRequest = new CreateBulkAssignmentsDto
        {
            TemplateId = templateId,
            EmployeeAssignments = employeeAssignments,
            DueDate = dueDate,
            AssignedBy = assignedBy,
            Notes = notes
        };

        // Create the bulk assignments - HR/Admin endpoint
        var result = await CreateWithResponseAsync<CreateBulkAssignmentsDto, object>($"{AssignmentCommandEndpoint}/bulk", createRequest);

        if (result == null)
        {
            return new List<QuestionnaireAssignment>();
        }

        // Query back the assignments for this template to get the newly created ones
        // This is necessary because the command API only returns Result, not the created assignments
        var allAssignments = await GetAssignmentsByTemplateAsync(templateId);

        // Filter to assignments created for the specific employees (best effort to return the new ones)
        var employeeIds = employees.Select(e => e.Id).ToList();
        var newAssignments = allAssignments.Where(a =>
            employeeIds.Contains(a.EmployeeId) &&
            a.AssignedDate >= DateTime.UtcNow.AddMinutes(-1) // Recently created
        ).ToList();

        return newAssignments.Any() ? newAssignments : allAssignments.Where(a => employeeIds.Contains(a.EmployeeId)).ToList();
    }

    /// <summary>
    /// Creates assignments for a manager's direct reports. TeamLead role only.
    /// Backend validates that all employees are direct reports of the authenticated manager.
    /// </summary>
    public async Task<List<QuestionnaireAssignment>> CreateManagerAssignmentsAsync(
        Guid templateId,
        List<EmployeeDto> employees,
        DateTime? dueDate,
        string? notes,
        string? assignedBy = null)
    {
        var employeeAssignments = employees.Select(emp => new EmployeeAssignmentDto
        {
            EmployeeId = emp.Id,
            EmployeeName = emp.FullName,
            EmployeeEmail = emp.EMail
        }).ToList();

        var createRequest = new CreateBulkAssignmentsDto
        {
            TemplateId = templateId,
            EmployeeAssignments = employeeAssignments,
            DueDate = dueDate,
            AssignedBy = assignedBy, // Backend will default to authenticated user if null
            Notes = notes
        };

        // Create the bulk assignments - Manager endpoint with authorization checks
        var result = await CreateWithResponseAsync<CreateBulkAssignmentsDto, object>($"{AssignmentCommandEndpoint}/manager/bulk", createRequest);

        if (result == null)
        {
            return new List<QuestionnaireAssignment>();
        }

        // Query back the assignments for this template to get the newly created ones
        var allAssignments = await GetAssignmentsByTemplateAsync(templateId);

        // Filter to assignments created for the specific employees (best effort to return the new ones)
        var employeeIds = employees.Select(e => e.Id).ToList();
        var newAssignments = allAssignments.Where(a =>
            employeeIds.Contains(a.EmployeeId) &&
            a.AssignedDate >= DateTime.UtcNow.AddMinutes(-1) // Recently created
        ).ToList();

        return newAssignments.Any() ? newAssignments : allAssignments.Where(a => employeeIds.Contains(a.EmployeeId)).ToList();
    }

    public async Task<QuestionnaireAssignment?> UpdateAssignmentStatusAsync(Guid id, AssignmentStatus status)
    {
        return await PatchAsync<AssignmentStatus, QuestionnaireAssignment>(AssignmentCommandEndpoint, id, "status", status);
    }

    // Assignment queries
    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByEmployeeAsync(Guid employeeId)
    {
        return await GetAllAsync<QuestionnaireAssignment>($"{AssignmentQueryEndpoint}/employee/{employeeId}");
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByTemplateAsync(Guid templateId)
    {
        return await GetAllAsync<QuestionnaireAssignment>($"{AssignmentQueryEndpoint}/template/{templateId}");
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByStatusAsync(AssignmentStatus status)
    {
        return await GetAllAsync<QuestionnaireAssignment>($"{AssignmentQueryEndpoint}/status/{status}");
    }

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByAssignerAsync(string assignerId)
    {
        return await GetAllAsync<QuestionnaireAssignment>($"{AssignmentQueryEndpoint}/assigner/{assignerId}");
    }

    // Assignment analytics
    public async Task<Dictionary<string, object>> GetAssignmentAnalyticsAsync()
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<Dictionary<string, object>>($"{AnalyticsEndpoint}/assignments") ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            LogError("Error fetching assignment analytics", ex);
            return new Dictionary<string, object>();
        }
    }

    public async Task<Dictionary<string, object>> GetEmployeeAssignmentStatsAsync(Guid employeeId)
    {
        try
        {
            return await HttpQueryClient.GetFromJsonAsync<Dictionary<string, object>>($"{AnalyticsEndpoint}/assignments/employee/{employeeId}") ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            LogError($"Error fetching assignment stats for employee {employeeId}", ex);
            return new Dictionary<string, object>();
        }
    }

    public async Task<Dictionary<string, object>> GetTemplateAssignmentStatsAsync(Guid templateId)
    {
        return await GetBySubPathAsync<Dictionary<string, object>>(AnalyticsEndpoint, "assignments/template", templateId) ?? new Dictionary<string, object>();
    }

    // Workflow operations
    public async Task<bool> CompleteSectionAsEmployeeAsync(Guid assignmentId, Guid sectionId)
    {
        try
        {
            var response = await HttpCommandClient.PostAsync($"{AssignmentCommandEndpoint}/{assignmentId}/sections/{sectionId}/complete-employee", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error completing section {sectionId} as employee for assignment {assignmentId}", ex);
            return false;
        }
    }

    public async Task<bool> CompleteSectionAsManagerAsync(Guid assignmentId, Guid sectionId)
    {
        try
        {
            var response = await HttpCommandClient.PostAsync($"{AssignmentCommandEndpoint}/{assignmentId}/sections/{sectionId}/complete-manager", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error completing section {sectionId} as manager for assignment {assignmentId}", ex);
            return false;
        }
    }

    public async Task<bool> ConfirmEmployeeCompletionAsync(Guid assignmentId, string confirmedBy)
    {
        try
        {
            var dto = new ConfirmCompletionDto { ConfirmedBy = confirmedBy };
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/confirm-employee", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error confirming employee completion for assignment {assignmentId}", ex);
            return false;
        }
    }

    public async Task<bool> ConfirmManagerCompletionAsync(Guid assignmentId, string confirmedBy)
    {
        try
        {
            var dto = new ConfirmCompletionDto { ConfirmedBy = confirmedBy };
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/confirm-manager", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error confirming manager completion for assignment {assignmentId}", ex);
            return false;
        }
    }

    public async Task<bool> InitiateReviewAsync(Guid assignmentId, string initiatedBy)
    {
        try
        {
            var dto = new InitiateReviewDto { InitiatedBy = initiatedBy };
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/initiate-review", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error initiating review for assignment {assignmentId}", ex);
            return false;
        }
    }

    public async Task<bool> EditAnswerDuringReviewAsync(Guid assignmentId, Guid sectionId, Guid questionId, string answer, string editedBy)
    {
        try
        {
            var dto = new EditAnswerDto
            {
                SectionId = sectionId,
                QuestionId = questionId,
                Answer = answer,
                EditedBy = editedBy
            };
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/edit-answer", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error editing answer during review for assignment {assignmentId}", ex);
            return false;
        }
    }

    public async Task<bool> ConfirmEmployeeReviewAsync(Guid assignmentId, string confirmedBy)
    {
        try
        {
            var dto = new ConfirmCompletionDto { ConfirmedBy = confirmedBy };
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/confirm-employee-review", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error confirming employee review for assignment {assignmentId}", ex);
            return false;
        }
    }

    public async Task<bool> FinalizeQuestionnaireAsync(Guid assignmentId, string finalizedBy)
    {
        try
        {
            var dto = new FinalizeQuestionnaireDto { FinalizedBy = finalizedBy };
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/finalize", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error finalizing questionnaire for assignment {assignmentId}", ex);
            return false;
        }
    }
}