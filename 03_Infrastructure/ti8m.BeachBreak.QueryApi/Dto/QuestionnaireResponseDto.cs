using ProgrammerAL.JsonSerializerRegistrationGenerator.Attributes;
using ti8m.BeachBreak.QueryApi.Serialization;

namespace ti8m.BeachBreak.QueryApi.Dto;

[RegisterJsonSerialization(typeof(QueryApiJsonSerializerContext))]
public class QuestionnaireResponseDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public Guid AssignmentId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime StartedDate { get; set; } = DateTime.Now;
    public Dictionary<Guid, SectionResponseDto> SectionResponses { get; set; } = new();
    public int ProgressPercentage { get; set; }
}
