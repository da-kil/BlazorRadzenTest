namespace ti8m.BeachBreak.CommandApi.Models;

public class QuestionnaireTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? LastModified { get; set; }
    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;
    public DateTime? PublishedDate { get; set; }
    public DateTime? LastPublishedDate { get; set; }
    public string PublishedBy { get; set; } = string.Empty;
    public List<QuestionSection> Sections { get; set; } = new();
    public QuestionnaireSettings Settings { get; set; } = new();
}

public class QuestionSection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public List<QuestionItem> Questions { get; set; } = new();
}

public class QuestionItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public List<string> Options { get; set; } = new(); // For choice-based questions
}

public enum QuestionType
{
    Assessment,          // 1-4 scale with comments - can be used by employee or manager
    GoalAchievement,     // Goal achievement evaluation like GoalReviewStep.razor
    TextQuestion         // Text area questions like CareerPlanningStep.razor
}

public class QuestionnaireSettings
{
    public bool AllowSaveProgress { get; set; } = true;
    public bool ShowProgressBar { get; set; } = true;
    public bool RequireAllSections { get; set; } = true;
    public string SuccessMessage { get; set; } = "Questionnaire completed successfully!";
    public string IncompleteMessage { get; set; } = "Please complete all required sections.";
    public TimeSpan? TimeLimit { get; set; }
    public bool AllowReviewBeforeSubmit { get; set; } = true;
}
// Response models
public class QuestionnaireResponse
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public Guid AssignmentId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime StartedDate { get; set; } = DateTime.Now;
    public Dictionary<Guid, SectionResponse> SectionResponses { get; set; } = new();
    public int ProgressPercentage { get; set; }
}

public class SectionResponse
{
    public Guid SectionId { get; set; }
    public bool IsCompleted { get; set; }

    // Role-based structure: RoleKey (e.g., "Employee", "Manager") -> QuestionId -> QuestionResponse
    public Dictionary<string, Dictionary<Guid, QuestionResponse>> RoleResponses { get; set; } = new();
}

public class QuestionResponse
{
    public Guid QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }
    public object? Value { get; set; }
    public string? TextValue { get; set; }
    public int? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }
    public List<string>? MultipleValues { get; set; }
    public Dictionary<string, object>? ComplexValue { get; set; } // For complex questions like goals
    public DateTime LastModified { get; set; } = DateTime.Now;
}

// DTOs for API requests
public class CreateQuestionnaireTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<QuestionSection> Sections { get; set; } = new();
    public QuestionnaireSettings Settings { get; set; } = new();
}

public class UpdateQuestionnaireTemplateRequest : CreateQuestionnaireTemplateRequest
{
    public Guid Id { get; set; }
}

public class CreateAssignmentRequest
{
    public Guid TemplateId { get; set; }
    public List<string> EmployeeIds { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
}

public class SubmitResponseRequest
{
    public Guid AssignmentId { get; set; }
    public Dictionary<Guid, SectionResponse> SectionResponses { get; set; } = new();
}

public enum TemplateStatus
{
    Draft = 0,      // Template can be edited, not assignable
    Published = 1,  // Template is read-only, can be assigned
    Archived = 2    // Template is inactive, cannot be assigned or edited
}