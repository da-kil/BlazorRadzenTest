namespace ti8m.BeachBreak.CommandApi.DTOs;

/// <summary>
/// DTO for text question responses with multiple text sections.
/// Provides type-safe access to text data without magic strings.
/// </summary>
public class TextResponseDto
{
    public List<string> TextSections { get; set; } = new();

}