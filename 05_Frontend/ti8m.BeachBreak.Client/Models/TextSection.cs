namespace ti8m.BeachBreak.Client.Models;

public class TextSection
{
    // Core properties
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public int Order { get; set; }

    // UI state properties (used by TextQuestionEditor)
    public bool? IsExpanded { get; set; } = false;
    public bool IsEditingTitle { get; set; } = false;
    public bool IsSelected { get; set; } = false;
    public bool ShowAutoSave { get; set; } = false;
}