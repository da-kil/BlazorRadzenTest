namespace ti8m.BeachBreak.Client.Models.DTOs.Api;

/// <summary>
/// Frontend DTO for deserializing API questionnaire responses.
/// Only includes properties actually used in the mapping.
/// </summary>
public class ApiQuestionnaireResponseDto
{
    public Dictionary<Guid, ApiSectionResponseDto> SectionResponses { get; set; } = new();
}
