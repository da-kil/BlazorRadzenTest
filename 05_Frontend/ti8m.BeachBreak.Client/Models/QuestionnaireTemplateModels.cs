namespace ti8m.BeachBreak.Client.Models;

public class QuestionnaireTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? LastModified { get; set; }
    public bool IsActive { get; set; } = true;
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
    SelfAssessment,      // 1-4 scale with comments like SelfAssessmentStep.razor
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

// Employee assignment models
public class QuestionnaireAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; } = DateTime.Now;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }
}

public enum AssignmentStatus
{
    Assigned,
    InProgress,
    Completed,
    Overdue,
    Cancelled
}

// Response models
public class QuestionnaireResponse
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public Guid AssignmentId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime StartedDate { get; set; } = DateTime.Now;
    public DateTime? CompletedDate { get; set; }
    public ResponseStatus Status { get; set; } = ResponseStatus.InProgress;
    public Dictionary<Guid, SectionResponse> SectionResponses { get; set; } = new();
    public int ProgressPercentage { get; set; }
}

public enum ResponseStatus
{
    NotStarted,
    InProgress,
    Completed,
    Submitted
}

public class SectionResponse
{
    public Guid SectionId { get; set; }
    public bool IsCompleted { get; set; }
    public Dictionary<Guid, QuestionResponse> QuestionResponses { get; set; } = new();
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