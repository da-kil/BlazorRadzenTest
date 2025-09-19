namespace ti8m.BeachBreak.Client.Models;

public class QuestionnaireTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
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
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;

    // New simplified structure - each section has one question type
    public QuestionType QuestionType { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();

    // Keep Questions for backward compatibility during migration
    [Obsolete("Use QuestionType and Configuration instead. This will be removed in future versions.")]
    public List<QuestionItem> Questions { get; set; } = new();

    // Helper methods for the new simplified structure
    public int GetItemCount()
    {
        return QuestionType switch
        {
            QuestionType.SelfAssessment => GetCompetencies().Count,
            QuestionType.GoalAchievement => GetGoalCategories().Count,
            QuestionType.TextQuestion => GetTextSections().Count,
            _ => 0
        };
    }

    public int GetRequiredItemCount()
    {
        return QuestionType switch
        {
            QuestionType.SelfAssessment => GetCompetencies().Count(c => c.IsRequired),
            QuestionType.GoalAchievement => GetGoalCategories().Count(g => g.IsRequired),
            QuestionType.TextQuestion => GetTextSections().Count(t => t.IsRequired),
            _ => 0
        };
    }

    public List<CompetencyDefinition> GetCompetencies()
    {
        if (Configuration.TryGetValue("Competencies", out var competenciesObj))
        {
            if (competenciesObj is List<CompetencyDefinition> competencies)
                return competencies;
        }
        return new List<CompetencyDefinition>();
    }

    public void SetCompetencies(List<CompetencyDefinition> competencies)
    {
        Configuration["Competencies"] = competencies;
    }

    public List<GoalCategory> GetGoalCategories()
    {
        if (Configuration.TryGetValue("GoalCategories", out var categoriesObj))
        {
            if (categoriesObj is List<GoalCategory> categories)
                return categories;
        }
        return new List<GoalCategory>();
    }

    public void SetGoalCategories(List<GoalCategory> categories)
    {
        Configuration["GoalCategories"] = categories;
    }

    public List<TextSection> GetTextSections()
    {
        if (Configuration.TryGetValue("TextSections", out var sectionsObj))
        {
            if (sectionsObj is List<TextSection> sections)
                return sections;
        }
        return new List<TextSection>();
    }

    public void SetTextSections(List<TextSection> sections)
    {
        Configuration["TextSections"] = sections;
    }

    public string GetTypeIcon()
    {
        return QuestionType switch
        {
            QuestionType.SelfAssessment => "self_improvement",
            QuestionType.GoalAchievement => "track_changes",
            QuestionType.TextQuestion => "psychology",
            _ => "help"
        };
    }

    public string GetTypeName()
    {
        return QuestionType switch
        {
            QuestionType.SelfAssessment => "Self-Assessment",
            QuestionType.GoalAchievement => "Goal Achievement",
            QuestionType.TextQuestion => "Text Question",
            _ => "Unknown"
        };
    }

    public string GetTypeColor()
    {
        return QuestionType switch
        {
            QuestionType.SelfAssessment => "#0F60FF", // primary-color
            QuestionType.GoalAchievement => "#00E6C8", // success-color
            QuestionType.TextQuestion => "#935BA9", // purple-rain
            _ => "#6c757d"
        };
    }
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

// Supporting classes for the new simplified structure
public class GoalCategory
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public int Order { get; set; }
}

public class TextSection
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public int Order { get; set; }
}