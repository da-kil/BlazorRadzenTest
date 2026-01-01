using ti8m.BeachBreak.Client.Models;
using ti8m.BeachBreak.Client.Models.Dto;
using System.Net.Http.Json;

namespace ti8m.BeachBreak.Client.Services;

public class QuestionnaireAssignmentService : BaseApiService, IQuestionnaireAssignmentService
{
    private const string AssignmentQueryEndpoint = "q/api/v1/assignments";
    private const string AssignmentCommandEndpoint = "c/api/v1/assignments";
    private const string EmployeeAssignmentEndpoint = "q/api/v1/employees/me/assignments";
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

    /// <summary>
    /// Gets a specific assignment for the currently authenticated employee.
    /// Uses the employee endpoint that requires the assignment to belong to the current user.
    /// </summary>
    public async Task<QuestionnaireAssignment?> GetMyAssignmentByIdAsync(Guid id)
    {
        return await GetByIdAsync<QuestionnaireAssignment>(EmployeeAssignmentEndpoint, id);
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

        try
        {
            // Create the bulk assignments - HR/Admin endpoint
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/bulk", createRequest);

            // Check if request was successful
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                LogError($"Failed to create assignments: {response.StatusCode} - {errorContent}", new Exception(errorContent));
                return new List<QuestionnaireAssignment>();
            }

            // Query back all assignments to get the newly created ones
            // This is necessary because the command API only returns Result, not the created assignments
            // Note: /assignments/template/{id} endpoint doesn't exist, so we fetch all and filter client-side
            var allAssignments = await GetAllAssignmentsAsync();

            // Filter to assignments created for the specific employees and template (best effort to return the new ones)
            var employeeIds = employees.Select(e => e.Id).ToList();
            var newAssignments = allAssignments.Where(a =>
                a.TemplateId == templateId &&
                employeeIds.Contains(a.EmployeeId) &&
                a.AssignedDate >= DateTime.UtcNow.AddMinutes(-1) // Recently created
            ).ToList();

            return newAssignments.Any() ? newAssignments.Where(a => a.TemplateId == templateId && employeeIds.Contains(a.EmployeeId)).ToList() : new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            LogError("Error creating bulk assignments", ex);
            throw;
        }
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

        try
        {
            // Create the bulk assignments - Manager endpoint with authorization checks
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/manager/bulk", createRequest);

            // Check if request was successful
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                LogError($"Failed to create manager assignments: {response.StatusCode} - {errorContent}", new Exception(errorContent));
                return new List<QuestionnaireAssignment>();
            }

            // Query back all assignments to get the newly created ones
            // Note: /assignments/template/{id} endpoint doesn't exist, so we fetch all and filter client-side
            var allAssignments = await GetAllAssignmentsAsync();

            // Filter to assignments created for the specific employees and template (best effort to return the new ones)
            var employeeIds = employees.Select(e => e.Id).ToList();
            var newAssignments = allAssignments.Where(a =>
                a.TemplateId == templateId &&
                employeeIds.Contains(a.EmployeeId) &&
                a.AssignedDate >= DateTime.UtcNow.AddMinutes(-1) // Recently created
            ).ToList();

            return newAssignments.Any() ? newAssignments.Where(a => a.TemplateId == templateId && employeeIds.Contains(a.EmployeeId)).ToList() : new List<QuestionnaireAssignment>();
        }
        catch (Exception ex)
        {
            LogError("Error creating manager bulk assignments", ex);
            throw;
        }
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
    public async Task<bool> SubmitEmployeeQuestionnaireAsync(Guid assignmentId, string submittedBy)
    {
        try
        {
            var dto = new SubmitQuestionnaireDto();
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
            var dto = new SubmitQuestionnaireDto();
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
            var dto = new FinishReviewMeetingDto { ReviewSummary = reviewSummary };
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
            var dto = new InitiateReviewDto();
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/initiate-review", dto);

            if (!response.IsSuccessStatusCode)
            {
                // Extract error message from response to show user
                var errorContent = await response.Content.ReadAsStringAsync();
                LogError($"Error initiating review for assignment {assignmentId}: {response.StatusCode} - {errorContent}", new Exception(errorContent));
                throw new HttpRequestException($"Unable to initiate review: {errorContent}");
            }

            return true;
        }
        catch (HttpRequestException)
        {
            // Re-throw HTTP exceptions so UI can show the error message
            throw;
        }
        catch (Exception ex)
        {
            LogError($"Error initiating review for assignment {assignmentId}", ex);
            throw new HttpRequestException("An unexpected error occurred while initiating the review. Please try again.");
        }
    }

    public async Task<bool> EditAnswerDuringReviewAsync(Guid assignmentId, Guid sectionId, Guid questionId, CompletionRole originalCompletionRole, string answer, string editedBy)
    {
        try
        {
            // Convert CompletionRole to ApplicationRole for backend API
            string applicationRole = originalCompletionRole switch
            {
                CompletionRole.Employee => "Employee",
                CompletionRole.Manager => "TeamLead", // Map Manager to a valid ApplicationRole
                _ => "Employee"
            };

            var dto = new EditAnswerDto
            {
                SectionId = sectionId,
                QuestionId = questionId,
                OriginalCompletionRole = applicationRole,
                Answer = answer
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
            var dto = new ConfirmReviewOutcomeDto { EmployeeComments = comments };
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/review/confirm-employee", dto);

            if (!response.IsSuccessStatusCode)
            {
                // Extract error message from response to show user
                var errorContent = await response.Content.ReadAsStringAsync();
                LogError($"Error confirming employee review for assignment {assignmentId}: {response.StatusCode} - {errorContent}", new Exception(errorContent));
                throw new HttpRequestException($"Unable to confirm review: {errorContent}");
            }

            return true;
        }
        catch (HttpRequestException)
        {
            // Re-throw HTTP exceptions so UI can show the error message
            throw;
        }
        catch (Exception ex)
        {
            LogError($"Error confirming employee review for assignment {assignmentId}", ex);
            throw new HttpRequestException("An unexpected error occurred while confirming the review. Please try again.");
        }
    }

    public async Task<bool> FinalizeQuestionnaireAsync(Guid assignmentId, string finalizedBy, string? finalNotes)
    {
        try
        {
            var dto = new FinalizeQuestionnaireDto { FinalizedBy = finalizedBy, ManagerFinalNotes = finalNotes };
            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/review/finalize-manager", dto);

            if (!response.IsSuccessStatusCode)
            {
                // Extract error message from response to show user
                var errorContent = await response.Content.ReadAsStringAsync();
                LogError($"Error finalizing questionnaire for assignment {assignmentId}: {response.StatusCode} - {errorContent}", new Exception(errorContent));
                throw new HttpRequestException($"Unable to finalize questionnaire: {errorContent}");
            }

            return true;
        }
        catch (HttpRequestException)
        {
            // Re-throw HTTP exceptions so UI can show the error message
            throw;
        }
        catch (Exception ex)
        {
            LogError($"Error finalizing questionnaire for assignment {assignmentId}", ex);
            throw new HttpRequestException("An unexpected error occurred while finalizing the questionnaire. Please try again.");
        }
    }

    // InReview note management
    public async Task<Result<Guid>> AddInReviewNoteAsync(Guid assignmentId, string content, Guid? sectionId)
    {
        try
        {
            var dto = new
            {
                Content = content,
                SectionId = sectionId,
            };

            var response = await HttpCommandClient.PostAsJsonAsync(
                $"{AssignmentCommandEndpoint}/{assignmentId}/notes",
                dto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();
                return result ?? Result<Guid>.Fail("Failed to deserialize response");
            }

            return Result<Guid>.Fail($"API call failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            LogError($"Error adding InReview note for assignment {assignmentId}", ex);
            return Result<Guid>.Fail($"Network error: {ex.Message}");
        }
    }

    public async Task<bool> UpdateInReviewNoteAsync(Guid assignmentId, Guid noteId, string content)
    {
        try
        {
            var dto = new { Content = content };
            var response = await HttpCommandClient.PutAsJsonAsync(
                $"{AssignmentCommandEndpoint}/{assignmentId}/notes/{noteId}",
                dto);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error updating InReview note {noteId} for assignment {assignmentId}", ex);
            return false;
        }
    }

    public async Task<bool> DeleteInReviewNoteAsync(Guid assignmentId, Guid noteId)
    {
        try
        {
            var response = await HttpCommandClient.DeleteAsync(
                $"{AssignmentCommandEndpoint}/{assignmentId}/notes/{noteId}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            LogError($"Error deleting InReview note {noteId} for assignment {assignmentId}", ex);
            return false;
        }
    }

    // Review changes tracking
    public async Task<List<ReviewChangeDto>> GetReviewChangesAsync(Guid assignmentId)
    {
        return await GetAllAsync<ReviewChangeDto>($"{AssignmentQueryEndpoint}/{assignmentId}/review-changes");
    }

    /// <summary>
    /// Reopens a questionnaire assignment to a previous workflow state.
    /// Only authorized roles (Admin, HR, TeamLead) can reopen assignments.
    /// </summary>
    public async Task<bool> ReopenQuestionnaireAsync(Guid assignmentId, WorkflowState targetState, string reopenReason)
    {
        try
        {
            var dto = new ReopenQuestionnaireDto
            {
                TargetState = targetState,
                ReopenReason = reopenReason
            };

            var response = await HttpCommandClient.PostAsJsonAsync($"{AssignmentCommandEndpoint}/{assignmentId}/reopen", dto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                LogError($"Failed to reopen assignment {assignmentId}: {response.StatusCode} - {errorContent}", new Exception(errorContent));
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            LogError($"Error reopening assignment {assignmentId}", ex);
            return false;
        }
    }
}