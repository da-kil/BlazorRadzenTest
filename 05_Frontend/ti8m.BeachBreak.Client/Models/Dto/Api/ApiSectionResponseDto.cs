namespace ti8m.BeachBreak.Client.Models.DTOs.Api;

/// <summary>
/// Frontend DTO for deserializing API section responses.
/// Only includes properties actually used in the mapping.
/// </summary>
public class ApiSectionResponseDto
{
    public Guid SectionId { get; set; }

    /// <summary>
    /// Role-based response structure: ResponseRole (Employee/Manager) -> SectionId -> QuestionResponse
    /// </summary>
    public Dictionary<ResponseRole, Dictionary<string, QuestionResponseDto>> RoleResponses { get; set; } = new();
}
