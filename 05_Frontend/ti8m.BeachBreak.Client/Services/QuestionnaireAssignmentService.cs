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
        string? notes)
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
        string? notes)
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

    public async Task<QuestionnaireAssignment?> UpdateAssignmentWorkflowStateAsync(Guid id, WorkflowState workflowState)
    {
        return await PatchAsync<WorkflowState, QuestionnaireAssignment>(AssignmentCommandEndpoint, id, "workflowState", workflowState);
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

    public async Task<List<QuestionnaireAssignment>> GetAssignmentsByWorkflowStateAsync(WorkflowState workflowState)
    {
        return await GetAllAsync<QuestionnaireAssignment>($"{AssignmentQueryEndpoint}/workflowState/{workflowState}");
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

    public async Task<bool> CompleteBulkSectionsAsEmployeeAsync(Guid assignmentId, List<Guid> sectionIds)
    {
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/sections/bulk-complete-employee", sectionIds);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error completing {sectionIds.Count} sections as employee for assignment {assignmentId}", ex);
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

    public async Task<bool> CompleteBulkSectionsAsManagerAsync(Guid assignmentId, List<Guid> sectionIds)
    {
        try
        {
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/sections/bulk-complete-manager", sectionIds);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error completing {sectionIds.Count} sections as manager for assignment {assignmentId}", ex);
            return false;
        }
    }

    public async Task<bool> SubmitEmployeeQuestionnaireAsync(Guid assignmentId, string submittedBy)
    {
        try
        {
            var dto = new SubmitQuestionnaireDto { SubmittedBy = submittedBy };
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/submit-employee", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error submitting employee questionnaire for assignment {assignmentId}", ex);
            return false;
        }
    }

    public async Task<bool> SubmitManagerQuestionnaireAsync(Guid assignmentId, string submittedBy)
    {
        try
        {
            var dto = new SubmitQuestionnaireDto { SubmittedBy = submittedBy };
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/submit-manager", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error submitting manager questionnaire for assignment {assignmentId}", ex);
            return false;
        }
    }

    public async Task<bool> FinishReviewMeetingAsync(Guid assignmentId, string finishedBy, string? reviewSummary)
    {
        try
        {
            var dto = new FinishReviewMeetingDto { FinishedBy = finishedBy, ReviewSummary = reviewSummary };
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/review/finish", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error finishing review meeting for assignment {assignmentId}", ex);
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

    public async Task<bool> EditAnswerDuringReviewAsync(Guid assignmentId, Guid sectionId, Guid questionId, CompletionRole originalCompletionRole, string answer, string editedBy)
    {
        try
        {
            var dto = new EditAnswerDto
            {
                SectionId = sectionId,
                QuestionId = questionId,
                OriginalCompletionRole = originalCompletionRole.ToString(),
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

    public async Task<bool> ConfirmEmployeeReviewAsync(Guid assignmentId, string confirmedBy, string? comments)
    {
        try
        {
            var dto = new ConfirmReviewOutcomeDto { ConfirmedBy = confirmedBy, EmployeeComments = comments };
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/review/confirm-employee", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error confirming employee review for assignment {assignmentId}", ex);
            return false;
        }
    }

    public async Task<bool> FinalizeQuestionnaireAsync(Guid assignmentId, string finalizedBy, string? finalNotes)
    {
        try
        {
            var dto = new FinalizeQuestionnaireDto { FinalizedBy = finalizedBy, ManagerFinalNotes = finalNotes };
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/review/finalize-manager", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error finalizing questionnaire for assignment {assignmentId}", ex);
            return false;
        }
    }

    // Review changes tracking
    public async Task<List<ReviewChangeDto>> GetReviewChangesAsync(Guid assignmentId)
    {
        return await GetAllAsync<ReviewChangeDto>($"{AssignmentQueryEndpoint}/{assignmentId}/review-changes");
    }
}