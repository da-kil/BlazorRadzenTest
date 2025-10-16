namespace ti8m.BeachBreak.Client.Models;

public class QuestionnaireResponse
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public Guid AssignmentId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime StartedDate { get; set; } = DateTime.Now;
    public Dictionary<Guid, SectionResponse> SectionResponses { get; set; } = new();
    public int ProgressPercentage { get; set; }
}