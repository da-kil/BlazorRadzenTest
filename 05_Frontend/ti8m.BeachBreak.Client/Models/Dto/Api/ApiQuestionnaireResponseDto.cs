namespace ti8m.BeachBreak.Client.Models.DTOs.Api;

/// <summary>
/// Frontend DTO for deserializing API questionnaire responses.
/// Only includes properties actually used in the mapping.
/// </summary>
public class ApiQuestionnaireResponseDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public Guid AssignmentId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime StartedDate { get; set; } = DateTime.Now;
    public Dictionary<Guid, ApiSectionResponseDto> SectionResponses { get; set; } = new();
    public int ProgressPercentage { get; set; }
}
