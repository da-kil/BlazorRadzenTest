namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Frontend model for feedback templates
/// </summary>
public class FeedbackTemplate
{
    public Guid Id { get; set; }

    // Bilingual content (flattened from Translation value object)
    public string NameGerman { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public string DescriptionGerman { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;

    // Configuration
    public List<EvaluationItem> Criteria { get; set; } = new();
    public List<TextSectionDefinition> TextSections { get; set; } = new();
    public int RatingScale { get; set; } = 10;
    public string ScaleLowLabel { get; set; } = "Poor";
    public string ScaleHighLabel { get; set; } = "Excellent";
    public List<int> AllowedSourceTypes { get; set; } = new();

    // Ownership (for authorization)
    public Guid CreatedByEmployeeId { get; set; }
    public int CreatedByRole { get; set; }
    public string CreatedByEmployeeName { get; set; } = string.Empty;

    // Status
    public TemplateStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public bool IsDeleted { get; set; }

    // Helper properties
    public bool CanBeUsedForFeedback => Status == TemplateStatus.Published && !IsDeleted;
    public bool IsAvailableForEditing => Status == TemplateStatus.Draft && !IsDeleted;

    /// <summary>
    /// Gets the localized name based on the specified language
    /// </summary>
    public string GetLocalizedName(Language language)
    {
        return language == Language.German && !string.IsNullOrWhiteSpace(NameGerman)
            ? NameGerman
            : NameEnglish;
    }

    /// <summary>
    /// Gets the localized description based on the specified language
    /// </summary>
    public string GetLocalizedDescription(Language language)
    {
        return language == Language.German && !string.IsNullOrWhiteSpace(DescriptionGerman)
            ? DescriptionGerman
            : DescriptionEnglish;
    }

    /// <summary>
    /// Checks if the current user can edit this template
    /// </summary>
    /// <param name="currentUserId">ID of the current user</param>
    /// <param name="currentUserRole">Role of the current user</param>
    /// <returns>True if the user can edit, false otherwise</returns>
    public bool CanBeEdited(Guid currentUserId, ApplicationRole currentUserRole)
    {
        // Can't edit non-draft templates
        if (Status != TemplateStatus.Draft || IsDeleted)
            return false;

        // Admin can edit any template
        if (currentUserRole == ApplicationRole.Admin)
            return true;

        // HR can edit HR templates
        if (CreatedByRole >= (int)ApplicationRole.HR && currentUserRole >= ApplicationRole.HR)
            return true;

        // TeamLead can only edit their own templates
        if (CreatedByRole == (int)ApplicationRole.TeamLead && CreatedByEmployeeId == currentUserId)
            return true;

        return false;
    }

    /// <summary>
    /// Gets a list of source type enums from the integer list
    /// </summary>
    public List<FeedbackSourceType> GetSourceTypes()
    {
        return AllowedSourceTypes.Select(st => (FeedbackSourceType)st).ToList();
    }
}
