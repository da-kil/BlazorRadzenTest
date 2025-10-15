namespace ti8m.BeachBreak.Client.Models;

public class SectionResponse
{
    public Guid SectionId { get; set; }
    public bool IsCompleted { get; set; }

    // Role-based structure: RoleKey (e.g., "Employee", "Manager") -> QuestionId -> QuestionResponse
    public Dictionary<string, Dictionary<Guid, QuestionResponse>> RoleResponses { get; set; } = new();
}