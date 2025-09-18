using ti8m.BeachBreak.Core.Infrastructure.ValueObjects;

namespace ti8m.BeachBreak.Client.Models;

public class QuestionnaireTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public MultilingualText Name { get; set; } = new();
    public MultilingualText Description { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? LastModified { get; set; }

    // Semantic status properties
    public bool IsActive { get; set; } = true;           // System availability
    public bool IsPublished { get; set; } = false;      // Ready for assignments
    public DateTime? PublishedDate { get; set; }        // First publish timestamp
    public DateTime? LastPublishedDate { get; set; }    // Most recent publish
    public string PublishedBy { get; set; } = string.Empty; // Who published it

    public List<QuestionSection> Sections { get; set; } = new();
    public QuestionnaireSettings Settings { get; set; } = new();

    // Business logic properties
    public bool CanBeAssigned => IsActive && IsPublished;
    public bool IsAvailableForEditing => IsActive;
    public bool IsVisibleInCatalog => IsActive && IsPublished;

    // Status determination
    public TemplateStatus Status => (IsActive, IsPublished) switch
    {
        (true, true)   => TemplateStatus.Published,
        (true, false)  => TemplateStatus.Draft,
        (false, true)  => TemplateStatus.PublishedInactive,
        (false, false) => TemplateStatus.Inactive,
    };
}

public class QuestionSection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public MultilingualText Title { get; set; } = new();
    public MultilingualText Description { get; set; } = new();
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public List<QuestionItem> Questions { get; set; } = new();
}

public class QuestionItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public MultilingualText Title { get; set; } = new();
    public MultilingualText Description { get; set; } = new();
    public QuestionType Type { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public MultilingualOptions Options { get; set; } = new(); // For choice-based questions
}

public enum QuestionType
{
    SelfAssessment,      // 1-4 scale with comments like SelfAssessmentStep.razor
    GoalAchievement,     // Goal achievement evaluation like GoalReviewStep.razor
    TextQuestion         // Text area questions like CareerPlanningStep.razor
}

public enum TemplateStatus
{
    Draft,              // Active but not published
    Published,          // Active and published
    PublishedInactive,  // Published but temporarily disabled
    Inactive            // Completely disabled
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

// Helper class for template status management
public static class TemplateStatusHelper
{
    public static string GetStatusBadgeClass(TemplateStatus status) => status switch
    {
        TemplateStatus.Published => "badge bg-success",
        TemplateStatus.Draft => "badge bg-warning text-dark",
        TemplateStatus.PublishedInactive => "badge bg-secondary",
        TemplateStatus.Inactive => "badge bg-danger",
        _ => "badge bg-info"
    };

    public static string GetStatusText(TemplateStatus status) => status switch
    {
        TemplateStatus.Published => "PUBLISHED",
        TemplateStatus.Draft => "DRAFT",
        TemplateStatus.PublishedInactive => "DISABLED",
        TemplateStatus.Inactive => "INACTIVE",
        _ => "UNKNOWN"
    };

    public static string GetStatusIcon(TemplateStatus status) => status switch
    {
        TemplateStatus.Published => "publish",
        TemplateStatus.Draft => "edit",
        TemplateStatus.PublishedInactive => "block",
        TemplateStatus.Inactive => "disabled_by_default",
        _ => "help"
    };

    public static string GetStatusDescription(TemplateStatus status) => status switch
    {
        TemplateStatus.Published => "Available for assignments and visible in catalog",
        TemplateStatus.Draft => "In development - not yet available for assignments",
        TemplateStatus.PublishedInactive => "Published but temporarily disabled",
        TemplateStatus.Inactive => "Not available for use",
        _ => "Status unknown"
    };

    public static List<string> GetAvailableActions(TemplateStatus status) => status switch
    {
        TemplateStatus.Draft => new List<string> { "Save", "Publish" },
        TemplateStatus.Published => new List<string> { "Save", "Unpublish", "Disable" },
        TemplateStatus.PublishedInactive => new List<string> { "Enable", "Edit" },
        TemplateStatus.Inactive => new List<string> { "Activate" },
        _ => new List<string>()
    };

    public static bool CanPerformAction(TemplateStatus status, string action) => action.ToLower() switch
    {
        "save" => status == TemplateStatus.Draft || status == TemplateStatus.Published,
        "publish" => status == TemplateStatus.Draft,
        "unpublish" => status == TemplateStatus.Published,
        "disable" => status == TemplateStatus.Published,
        "enable" => status == TemplateStatus.PublishedInactive,
        "activate" => status == TemplateStatus.Inactive,
        "edit" => status != TemplateStatus.Inactive,
        _ => false
    };
}