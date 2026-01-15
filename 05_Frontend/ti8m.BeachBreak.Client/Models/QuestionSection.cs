namespace ti8m.BeachBreak.Client.Models;

public class QuestionSection
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Bilingual content properties - matching QueryApi DTO naming
    public string TitleEnglish { get; set; } = string.Empty;
    public string TitleGerman { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;
    public string DescriptionGerman { get; set; } = string.Empty;


    public int Order { get; set; }

    // Role assignment for dual completion workflow
    public CompletionRole CompletionRole { get; set; } = CompletionRole.Employee;
    public bool IsEmployeeCompleted { get; set; } = false;
    public bool IsManagerCompleted { get; set; } = false;
    public DateTime? EmployeeCompletedDate { get; set; }
    public DateTime? ManagerCompletedDate { get; set; }

    public QuestionType Type { get; set; }
    public IQuestionConfiguration Configuration { get; set; } = new AssessmentConfiguration();
    public bool IsInstanceSpecific { get; set; } = false;

    // Helper methods using strongly-typed configuration
    public int GetItemCount()
    {
        return Type switch
        {
            QuestionType.Assessment when Configuration is AssessmentConfiguration assessmentConfig
                => assessmentConfig.Evaluations.Count,
            QuestionType.Goal
                => 0, // Goals are added dynamically during workflow, not in template
            QuestionType.TextQuestion when Configuration is TextQuestionConfiguration textConfig
                => textConfig.TextSections.Count,
            QuestionType.EmployeeFeedback
                => 0, // Feedback records are linked during workflow, not configured in template
            _ => 0
        };
    }

    public int GetRequiredItemCount()
    {
        return Type switch
        {
            QuestionType.Assessment when Configuration is AssessmentConfiguration assessmentConfig
                => assessmentConfig.Evaluations.Count(c => c.IsRequired),
            QuestionType.Goal
                => 0, // Goals are added dynamically during workflow, not in template
            QuestionType.TextQuestion when Configuration is TextQuestionConfiguration textConfig
                => textConfig.TextSections.Count(t => t.IsRequired),
            QuestionType.EmployeeFeedback
                => 0, // Feedback records are linked during workflow, not configured in template
            _ => 0
        };
    }

    public string GetTypeIcon()
    {
        return Type switch
        {
            QuestionType.Assessment => "self_improvement",
            QuestionType.Goal => "track_changes",
            QuestionType.TextQuestion => "psychology",
            QuestionType.EmployeeFeedback => "feedback",
            _ => "help"
        };
    }

    public string GetTypeName()
    {
        return Type switch
        {
            QuestionType.Assessment => "Assessment",
            QuestionType.Goal => "Goal Achievement",
            QuestionType.TextQuestion => "Text Question",
            QuestionType.EmployeeFeedback => "Employee Feedback",
            _ => "Unknown"
        };
    }

    public string GetTypeColor()
    {
        return Type switch
        {
            QuestionType.Assessment => "var(--rz-primary)", // primary-color
            QuestionType.Goal => "var(--rz-success)", // success-color
            QuestionType.TextQuestion => "var(--rz-secondary)", // secondary-color
            QuestionType.EmployeeFeedback => "var(--rz-info)", // info-color (light blue)
            _ => "var(--rz-base-500)"
        };
    }

    // Completion role helper methods
    public bool IsCompletedForRole(string userRole)
    {
        return userRole.ToLower() switch
        {
            "employee" => IsEmployeeCompleted,
            "manager" => IsManagerCompleted,
            _ => false
        };
    }

    public bool IsFullyCompleted()
    {
        return CompletionRole switch
        {
            CompletionRole.Employee => IsEmployeeCompleted,
            CompletionRole.Manager => IsManagerCompleted,
            CompletionRole.Both => IsEmployeeCompleted && IsManagerCompleted,
            _ => false
        };
    }

    public string GetCompletionStatusText()
    {
        return CompletionRole switch
        {
            CompletionRole.Employee => IsEmployeeCompleted ? "Completed by Employee" : "Pending Employee",
            CompletionRole.Manager => IsManagerCompleted ? "Completed by Manager" : "Pending Manager",
            CompletionRole.Both => (IsEmployeeCompleted, IsManagerCompleted) switch
            {
                (true, true) => "Completed by Both",
                (true, false) => "Employee ✓, Manager Pending",
                (false, true) => "Manager ✓, Employee Pending",
                (false, false) => "Pending Both"
            },
            _ => "Unknown"
        };
    }

    public string GetRoleIcon()
    {
        return CompletionRole switch
        {
            CompletionRole.Employee => "person",
            CompletionRole.Manager => "supervisor_account",
            CompletionRole.Both => "groups",
            _ => "help"
        };
    }

    public string GetRoleColor()
    {
        return CompletionRole switch
        {
            CompletionRole.Employee => "var(--rz-primary)", // Blue
            CompletionRole.Manager => "var(--rz-success)", // Green
            CompletionRole.Both => "var(--rz-secondary)", // Purple
            _ => "var(--rz-base-500)"
        };
    }

    // Helper methods for language-aware content display
    public string GetLocalizedTitle(Language language)
    {
        return language == Language.German ? TitleGerman : TitleEnglish;
    }

    public string GetLocalizedDescription(Language language)
    {
        return language == Language.German ? DescriptionGerman : DescriptionEnglish;
    }

    // Helper method to fallback to English if German is empty
    public string GetLocalizedTitleWithFallback(Language language)
    {
        var localized = GetLocalizedTitle(language);
        return !string.IsNullOrWhiteSpace(localized) ? localized : TitleEnglish;
    }

    public string GetLocalizedDescriptionWithFallback(Language language)
    {
        var localized = GetLocalizedDescription(language);
        return !string.IsNullOrWhiteSpace(localized) ? localized : DescriptionEnglish;
    }
}