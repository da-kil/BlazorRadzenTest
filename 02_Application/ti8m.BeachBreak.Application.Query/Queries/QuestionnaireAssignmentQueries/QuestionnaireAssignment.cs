namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

public class QuestionnaireAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; } = DateTime.Now;
    public DateTime? DueDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsWithdrawn { get; set; }
    public DateTime? WithdrawnDate { get; set; }
    public string? WithdrawnBy { get; set; }
    public string? WithdrawalReason { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }
}
