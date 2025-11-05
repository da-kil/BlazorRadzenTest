namespace ti8m.BeachBreak.Client.Models.CommandDTOs;

/// <summary>
/// Command DTO for text question responses.
/// Eliminates magic string keys with strongly-typed list.
/// </summary>
public class TextResponseCommandDto
{
    public List<string> TextSections { get; set; } = new();
}
