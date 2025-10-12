using ti8m.BeachBreak.Core.Domain.SharedKernel;

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
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }

    // Denormalized template metadata (populated in query handler)
    public string TemplateName { get; set; } = string.Empty;
    public Guid? TemplateCategoryId { get; set; }

    // Workflow properties
    public WorkflowState WorkflowState { get; set; } = WorkflowState.Assigned;
    public List<SectionProgressDto> SectionProgress { get; set; } = new();

    // Submission phase
    public DateTime? EmployeeSubmittedDate { get; set; }
    public string? EmployeeSubmittedBy { get; set; }
    public DateTime? ManagerSubmittedDate { get; set; }
    public string? ManagerSubmittedBy { get; set; }

    // Review phase
    public DateTime? ReviewInitiatedDate { get; set; }
    public string? ReviewInitiatedBy { get; set; }
    public DateTime? EmployeeReviewConfirmedDate { get; set; }
    public string? EmployeeReviewConfirmedBy { get; set; }
    public DateTime? ManagerReviewConfirmedDate { get; set; }
    public string? ManagerReviewConfirmedBy { get; set; }

    // Final state
    public DateTime? FinalizedDate { get; set; }
    public string? FinalizedBy { get; set; }
    public bool IsLocked { get; set; }
}
