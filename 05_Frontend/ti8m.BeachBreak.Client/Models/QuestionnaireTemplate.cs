namespace ti8m.BeachBreak.Client.Models;

public class QuestionnaireTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Bilingual content properties - matching QueryApi DTO naming
    public string NameEnglish { get; set; } = string.Empty;
    public string NameGerman { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;
    public string DescriptionGerman { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public bool RequiresManagerReview { get; set; } = true;
    public bool IsCustomizable { get; set; } = false;
    public bool AutoInitialize { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;
    public DateTime? PublishedDate { get; set; }        // First publish timestamp
    public DateTime? LastPublishedDate { get; set; }    // Most recent publish
    public Guid? PublishedByEmployeeId { get; set; }    // Employee who published it
    public string? PublishedByEmployeeName { get; set; } // Resolved employee name for display

    public List<QuestionSection> Sections { get; set; } = new();

    // Business logic properties derived from status
    public bool CanBeAssigned => Status == TemplateStatus.Published;
    public bool IsAvailableForEditing => Status == TemplateStatus.Draft;
    public bool IsVisibleInCatalog => Status == TemplateStatus.Published;

    // Helper methods for language-aware content display
    public string GetLocalizedName(Language language)
    {
        return language == Language.German ? NameGerman : NameEnglish;
    }

    public string GetLocalizedDescription(Language language)
    {
        return language == Language.German ? DescriptionGerman : DescriptionEnglish;
    }

    // Helper method to fallback to English if German is empty
    public string GetLocalizedNameWithFallback(Language language)
    {
        var localized = GetLocalizedName(language);
        return !string.IsNullOrWhiteSpace(localized) ? localized : NameEnglish;
    }

    public string GetLocalizedDescriptionWithFallback(Language language)
    {
        var localized = GetLocalizedDescription(language);
        return !string.IsNullOrWhiteSpace(localized) ? localized : DescriptionEnglish;
    }
}