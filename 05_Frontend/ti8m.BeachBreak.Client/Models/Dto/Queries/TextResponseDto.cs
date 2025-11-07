namespace ti8m.BeachBreak.Client.Models.DTOs;

/// <summary>
/// DTO for text question responses with multiple text sections.
/// Provides type-safe access to text data without magic strings.
/// </summary>
public class TextResponseDto
{
    public List<string> TextSections { get; set; } = new();

    /// <summary>
    /// Gets text for a specific section index with bounds checking.
    /// </summary>
    public string GetSectionText(int sectionIndex) =>
        sectionIndex >= 0 && sectionIndex < TextSections.Count ? TextSections[sectionIndex] : string.Empty;

    /// <summary>
    /// Sets text for a specific section, expanding the list if necessary.
    /// </summary>
    public void SetSectionText(int sectionIndex, string text)
    {
        // Expand list if necessary
        while (TextSections.Count <= sectionIndex)
        {
            TextSections.Add(string.Empty);
        }

        TextSections[sectionIndex] = text ?? string.Empty;
    }

    /// <summary>
    /// Creates a TextResponseDto from a single text value.
    /// </summary>
    public static TextResponseDto FromSingleText(string text) => new() { TextSections = [text] };

    /// <summary>
    /// Creates a TextResponseDto from multiple text sections.
    /// </summary>
    public static TextResponseDto FromMultipleTexts(params string[] texts) => new() { TextSections = texts.ToList() };

    /// <summary>
    /// Validates that all required sections have content.
    /// </summary>
    public bool IsComplete(int requiredSections = 1) =>
        TextSections.Count >= requiredSections &&
        TextSections.Take(requiredSections).All(text => !string.IsNullOrWhiteSpace(text));
}