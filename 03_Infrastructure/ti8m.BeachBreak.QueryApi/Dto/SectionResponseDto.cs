namespace ti8m.BeachBreak.QueryApi.Dto;

public class SectionResponseDto
{
    public Guid SectionId { get; set; }
    public bool IsCompleted { get; set; }

    // Role-based structure: RoleKey (e.g., "Employee", "Manager") -> QuestionId -> QuestionResponse
    public Dictionary<string, Dictionary<Guid, QuestionResponseDto>> RoleResponses { get; set; } = new();
}
