namespace ti8m.BeachBreak.QueryApi.Dto;

public class QuestionnaireResponseDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public Guid AssignmentId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime StartedDate { get; set; } = DateTime.Now;
    public DateTime? CompletedDate { get; set; }
    public ResponseStatus Status { get; set; } = ResponseStatus.InProgress;
    public Dictionary<Guid, SectionResponseDto> SectionResponses { get; set; } = new();
    public int ProgressPercentage { get; set; }
}
