namespace ti8m.BeachBreak.Client.Models.Dto;

/// <summary>
/// Client-side DTO for adding custom question sections to an assignment during initialization.
/// Custom sections are instance-specific and excluded from aggregate reports.
/// Matches the CommandApi DTO for type-safe communication.
/// </summary>
public class AddCustomSectionsDto
{
    /// <summary>
    /// List of custom question sections to add.
    /// Only Assessment and TextQuestion types allowed (no Goal questions).
    /// </summary>
    public List<QuestionSection> Sections { get; set; } = new();
}
