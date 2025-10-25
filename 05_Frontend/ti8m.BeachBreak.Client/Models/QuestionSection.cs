namespace ti8m.BeachBreak.Client.Models;

public class QuestionSection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;

    // Role assignment for dual completion workflow
    public CompletionRole CompletionRole { get; set; } = CompletionRole.Employee;
    public bool IsEmployeeCompleted { get; set; } = false;
    public bool IsManagerCompleted { get; set; } = false;
    public DateTime? EmployeeCompletedDate { get; set; }
    public DateTime? ManagerCompletedDate { get; set; }

    // New simplified structure - each section has one question type
    public QuestionType QuestionType { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();

    // Questions are the core content of each section
    public List<QuestionItem> Questions { get; set; } = new();

    // Helper methods for the new simplified structure
    public int GetItemCount()
    {
        return QuestionType switch
        {
            QuestionType.Assessment => GetCompetencies().Count,
            QuestionType.Goal => GetGoalCategories().Count,
            QuestionType.TextQuestion => GetTextSections().Count,
            _ => 0
        };
    }

    public int GetRequiredItemCount()
    {
        return QuestionType switch
        {
            QuestionType.Assessment => GetCompetencies().Count(c => c.IsRequired),
            QuestionType.Goal => GetGoalCategories().Count(g => g.IsRequired),
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
            QuestionType.Assessment => "self_improvement",
            QuestionType.Goal => "track_changes",
            QuestionType.TextQuestion => "psychology",
            _ => "help"
        };
    }

    public string GetTypeName()
    {
        return QuestionType switch
        {
            QuestionType.Assessment => "Assessment",
            QuestionType.Goal => "Goal Achievement",
            QuestionType.TextQuestion => "Text Question",
            _ => "Unknown"
        };
    }

    public string GetTypeColor()
    {
        return QuestionType switch
        {
            QuestionType.Assessment => "#0F60FF", // primary-color
            QuestionType.Goal => "#00E6C8", // success-color
            QuestionType.TextQuestion => "#935BA9", // purple-rain
            _ => "#6c757d"
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
            CompletionRole.Employee => "#0F60FF", // Blue
            CompletionRole.Manager => "#00E6C8", // Green
            CompletionRole.Both => "#935BA9", // Purple
            _ => "#6c757d"
        };
    }
}