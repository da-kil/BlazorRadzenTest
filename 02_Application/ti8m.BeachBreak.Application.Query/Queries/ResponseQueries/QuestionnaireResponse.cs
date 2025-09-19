namespace ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;

public class QuestionnaireResponse
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime LastModified { get; set; }
    public ResponseStatus Status { get; set; }
    public Dictionary<Guid, object> SectionResponses { get; set; } = new();
    public int ProgressPercentage { get; set; }
}

public enum ResponseStatus
{
    NotStarted,
    InProgress,
    Completed,
    Submitted
}