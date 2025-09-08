using WebApi.Models;

namespace WebApi.Services;

public class QuestionnaireService : IQuestionnaireService
{
    private readonly List<QuestionnaireTemplate> _templates = new();
    private readonly List<QuestionnaireAssignment> _assignments = new();
    private readonly List<QuestionnaireResponse> _responses = new();

    public QuestionnaireService()
    {
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        // Create sample template
        var sampleTemplate = new QuestionnaireTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Annual Performance Review 2024",
            Description = "Comprehensive annual performance review questionnaire",
            Category = "Performance Review",
            CreatedDate = DateTime.Now.AddDays(-30),
            LastModified = DateTime.Now.AddDays(-5),
            IsActive = true,
            Sections = new List<QuestionSection>
            {
                new QuestionSection
                {
                    Id = Guid.NewGuid(),
                    Title = "Self-Assessment",
                    Description = "Rate your performance in key competencies",
                    Order = 0,
                    IsRequired = true,
                    Questions = new List<QuestionItem>
                    {
                        new QuestionItem
                        {
                            Id = Guid.NewGuid(),
                            Title = "How would you rate your overall performance this year?",
                            Type = QuestionType.RatingQuestion,
                            Order = 0,
                            IsRequired = true
                        },
                        new QuestionItem
                        {
                            Id = Guid.NewGuid(),
                            Title = "What are your key accomplishments this year?",
                            Type = QuestionType.TextQuestion,
                            Order = 1,
                            IsRequired = true
                        }
                    }
                },
                new QuestionSection
                {
                    Id = Guid.NewGuid(),
                    Title = "Goal Setting",
                    Description = "Set your goals for the upcoming year",
                    Order = 1,
                    IsRequired = true,
                    Questions = new List<QuestionItem>
                    {
                        new QuestionItem
                        {
                            Id = Guid.NewGuid(),
                            Title = "Set your primary professional goal for next year",
                            Type = QuestionType.GoalQuestion,
                            Order = 0,
                            IsRequired = true
                        }
                    }
                }
            },
            Settings = new QuestionnaireSettings
            {
                AllowSaveProgress = true,
                ShowProgressBar = true,
                RequireAllSections = true,
                SuccessMessage = "Thank you for completing your annual review!",
                AllowReviewBeforeSubmit = true
            }
        };

        _templates.Add(sampleTemplate);

        // Create sample assignment
        var sampleAssignment = new QuestionnaireAssignment
        {
            Id = Guid.NewGuid(),
            TemplateId = sampleTemplate.Id,
            EmployeeId = "EMP001",
            EmployeeName = "John Smith",
            EmployeeEmail = "john.smith@company.com",
            AssignedDate = DateTime.Now.AddDays(-7),
            DueDate = DateTime.Now.AddDays(7),
            Status = AssignmentStatus.Assigned,
            AssignedBy = "HR Manager",
            Notes = "Please complete by the due date"
        };

        _assignments.Add(sampleAssignment);
    }

    // Template management
    public Task<List<QuestionnaireTemplate>> GetAllTemplatesAsync()
    {
        return Task.FromResult(_templates.Where(t => t.IsActive).ToList());
    }

    public Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid id)
    {
        return Task.FromResult(_templates.FirstOrDefault(t => t.Id == id && t.IsActive));
    }

    public Task<QuestionnaireTemplate> CreateTemplateAsync(CreateQuestionnaireTemplateRequest request)
    {
        var template = new QuestionnaireTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            CreatedDate = DateTime.Now,
            LastModified = DateTime.Now,
            IsActive = true,
            Sections = request.Sections,
            Settings = request.Settings
        };

        _templates.Add(template);
        return Task.FromResult(template);
    }

    public Task<QuestionnaireTemplate?> UpdateTemplateAsync(Guid id, UpdateQuestionnaireTemplateRequest request)
    {
        var template = _templates.FirstOrDefault(t => t.Id == id);
        if (template == null) return Task.FromResult<QuestionnaireTemplate?>(null);

        template.Name = request.Name;
        template.Description = request.Description;
        template.Category = request.Category;
        template.LastModified = DateTime.Now;
        template.Sections = request.Sections;
        template.Settings = request.Settings;

        return Task.FromResult<QuestionnaireTemplate?>(template);
    }

    public Task<bool> DeleteTemplateAsync(Guid id)
    {
        var template = _templates.FirstOrDefault(t => t.Id == id);
        if (template == null) return Task.FromResult(false);

        template.IsActive = false;
        template.LastModified = DateTime.Now;
        return Task.FromResult(true);
    }

    public Task<List<QuestionnaireTemplate>> GetTemplatesByCategoryAsync(string category)
    {
        return Task.FromResult(_templates.Where(t => t.IsActive && t.Category == category).ToList());
    }

    // Assignment management
    public Task<List<QuestionnaireAssignment>> GetAllAssignmentsAsync()
    {
        return Task.FromResult(_assignments.ToList());
    }

    public Task<QuestionnaireAssignment?> GetAssignmentByIdAsync(Guid id)
    {
        return Task.FromResult(_assignments.FirstOrDefault(a => a.Id == id));
    }

    public Task<List<QuestionnaireAssignment>> GetAssignmentsByEmployeeAsync(string employeeId)
    {
        return Task.FromResult(_assignments.Where(a => a.EmployeeId == employeeId).ToList());
    }

    public Task<List<QuestionnaireAssignment>> CreateAssignmentsAsync(CreateAssignmentRequest request)
    {
        var template = _templates.FirstOrDefault(t => t.Id == request.TemplateId && t.IsActive);
        if (template == null) return Task.FromResult(new List<QuestionnaireAssignment>());

        var assignments = new List<QuestionnaireAssignment>();
        foreach (var employeeId in request.EmployeeIds)
        {
            var assignment = new QuestionnaireAssignment
            {
                Id = Guid.NewGuid(),
                TemplateId = request.TemplateId,
                EmployeeId = employeeId,
                EmployeeName = $"Employee {employeeId}", // In real implementation, would lookup from employee service
                EmployeeEmail = $"{employeeId.ToLower()}@company.com",
                AssignedDate = DateTime.Now,
                DueDate = request.DueDate,
                Status = AssignmentStatus.Assigned,
                AssignedBy = request.AssignedBy,
                Notes = request.Notes
            };

            _assignments.Add(assignment);
            assignments.Add(assignment);
        }

        return Task.FromResult(assignments);
    }

    public Task<QuestionnaireAssignment?> UpdateAssignmentStatusAsync(Guid id, AssignmentStatus status)
    {
        var assignment = _assignments.FirstOrDefault(a => a.Id == id);
        if (assignment == null) return Task.FromResult<QuestionnaireAssignment?>(null);

        assignment.Status = status;
        if (status == AssignmentStatus.Completed)
        {
            assignment.CompletedDate = DateTime.Now;
        }

        return Task.FromResult<QuestionnaireAssignment?>(assignment);
    }

    public Task<bool> DeleteAssignmentAsync(Guid id)
    {
        var assignment = _assignments.FirstOrDefault(a => a.Id == id);
        if (assignment == null) return Task.FromResult(false);

        _assignments.Remove(assignment);
        return Task.FromResult(true);
    }

    // Response management
    public Task<List<QuestionnaireResponse>> GetAllResponsesAsync()
    {
        return Task.FromResult(_responses.ToList());
    }

    public Task<QuestionnaireResponse?> GetResponseByIdAsync(Guid id)
    {
        return Task.FromResult(_responses.FirstOrDefault(r => r.Id == id));
    }

    public Task<QuestionnaireResponse?> GetResponseByAssignmentIdAsync(Guid assignmentId)
    {
        return Task.FromResult(_responses.FirstOrDefault(r => r.AssignmentId == assignmentId));
    }

    public Task<QuestionnaireResponse> CreateOrUpdateResponseAsync(Guid assignmentId, Dictionary<Guid, SectionResponse> sectionResponses)
    {
        var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId);
        if (assignment == null) throw new ArgumentException("Assignment not found");

        var response = _responses.FirstOrDefault(r => r.AssignmentId == assignmentId);
        
        if (response == null)
        {
            response = new QuestionnaireResponse
            {
                Id = Guid.NewGuid(),
                TemplateId = assignment.TemplateId,
                AssignmentId = assignmentId,
                EmployeeId = assignment.EmployeeId,
                StartedDate = DateTime.Now,
                Status = ResponseStatus.InProgress,
                SectionResponses = sectionResponses,
                ProgressPercentage = CalculateProgress(sectionResponses)
            };
            _responses.Add(response);
        }
        else
        {
            response.SectionResponses = sectionResponses;
            response.ProgressPercentage = CalculateProgress(sectionResponses);
        }

        // Update assignment status if not already in progress
        if (assignment.Status == AssignmentStatus.Assigned)
        {
            assignment.Status = AssignmentStatus.InProgress;
        }

        return Task.FromResult(response);
    }

    public Task<QuestionnaireResponse?> SubmitResponseAsync(Guid assignmentId)
    {
        var response = _responses.FirstOrDefault(r => r.AssignmentId == assignmentId);
        var assignment = _assignments.FirstOrDefault(a => a.Id == assignmentId);
        
        if (response == null || assignment == null) 
            return Task.FromResult<QuestionnaireResponse?>(null);

        response.Status = ResponseStatus.Submitted;
        response.CompletedDate = DateTime.Now;
        response.ProgressPercentage = 100;

        assignment.Status = AssignmentStatus.Completed;
        assignment.CompletedDate = DateTime.Now;

        return Task.FromResult<QuestionnaireResponse?>(response);
    }

    // Analytics
    public Task<Dictionary<string, object>> GetTemplateAnalyticsAsync(Guid templateId)
    {
        var assignments = _assignments.Where(a => a.TemplateId == templateId).ToList();
        var responses = _responses.Where(r => r.TemplateId == templateId).ToList();

        var analytics = new Dictionary<string, object>
        {
            ["TotalAssignments"] = assignments.Count,
            ["CompletedResponses"] = responses.Count(r => r.Status == ResponseStatus.Submitted),
            ["InProgressResponses"] = responses.Count(r => r.Status == ResponseStatus.InProgress),
            ["AverageCompletionTime"] = responses
                .Where(r => r.CompletedDate.HasValue)
                .Select(r => (r.CompletedDate!.Value - r.StartedDate).TotalHours)
                .DefaultIfEmpty(0)
                .Average(),
            ["CompletionRate"] = assignments.Count > 0 ? 
                (double)assignments.Count(a => a.Status == AssignmentStatus.Completed) / assignments.Count * 100 : 0
        };

        return Task.FromResult(analytics);
    }

    public Task<Dictionary<string, object>> GetOverallAnalyticsAsync()
    {
        var analytics = new Dictionary<string, object>
        {
            ["TotalTemplates"] = _templates.Count(t => t.IsActive),
            ["TotalAssignments"] = _assignments.Count,
            ["TotalResponses"] = _responses.Count,
            ["CompletedResponses"] = _responses.Count(r => r.Status == ResponseStatus.Submitted),
            ["OverallCompletionRate"] = _assignments.Count > 0 ?
                (double)_assignments.Count(a => a.Status == AssignmentStatus.Completed) / _assignments.Count * 100 : 0,
            ["TemplatesByCategory"] = _templates
                .Where(t => t.IsActive)
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return Task.FromResult(analytics);
    }

    private int CalculateProgress(Dictionary<Guid, SectionResponse> sectionResponses)
    {
        if (!sectionResponses.Any()) return 0;

        var completedSections = sectionResponses.Values.Count(s => s.IsCompleted);
        return (int)Math.Round((double)completedSections / sectionResponses.Count * 100);
    }
}