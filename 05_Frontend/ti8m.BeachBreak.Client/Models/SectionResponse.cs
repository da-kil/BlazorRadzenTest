namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Represents responses for a single questionnaire section.
/// Uses ResponseRole enum for type-safe dictionary keys.
/// </summary>
public class SectionResponse
{
    public Guid SectionId { get; set; }
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Role-based response structure: ResponseRole (Employee/Manager) -> QuestionId -> QuestionResponse
    /// Provides compile-time type safety and prevents invalid role keys.
    /// </summary>
    public Dictionary<ResponseRole, Dictionary<Guid, QuestionResponse>> RoleResponses { get; set; } = new();
}