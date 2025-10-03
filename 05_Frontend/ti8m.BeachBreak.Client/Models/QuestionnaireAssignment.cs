namespace ti8m.BeachBreak.Client.Models;

public class QuestionnaireAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; } = DateTime.Now;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }

    // Workflow properties
    public string WorkflowState { get; set; } = "Assigned";
    public List<SectionProgressDto> SectionProgress { get; set; } = new();
    public DateTime? EmployeeConfirmedDate { get; set; }
    public string? EmployeeConfirmedBy { get; set; }
    public DateTime? ManagerConfirmedDate { get; set; }
    public string? ManagerConfirmedBy { get; set; }
    public DateTime? ReviewInitiatedDate { get; set; }
    public string? ReviewInitiatedBy { get; set; }
    public DateTime? EmployeeReviewConfirmedDate { get; set; }
    public string? EmployeeReviewConfirmedBy { get; set; }
    public DateTime? FinalizedDate { get; set; }
    public string? FinalizedBy { get; set; }
    public bool IsLocked { get; set; }
}