using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.QueryApi.Dto;

public class QuestionnaireAssignmentDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public bool RequiresManagerReview { get; set; } = true;
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; } = DateTime.Now;
    public DateTime? DueDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsWithdrawn { get; set; }
    public DateTime? WithdrawnDate { get; set; }
    public Guid? WithdrawnByEmployeeId { get; set; }
    public string? WithdrawnByEmployeeName { get; set; }
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
    public Guid? EmployeeSubmittedByEmployeeId { get; set; }
    public string? EmployeeSubmittedByEmployeeName { get; set; }
    public DateTime? ManagerSubmittedDate { get; set; }
    public Guid? ManagerSubmittedByEmployeeId { get; set; }
    public string? ManagerSubmittedByEmployeeName { get; set; }

    // Review phase
    public DateTime? ReviewInitiatedDate { get; set; }
    public Guid? ReviewInitiatedByEmployeeId { get; set; }
    public string? ReviewInitiatedByEmployeeName { get; set; }
    public DateTime? ManagerReviewFinishedDate { get; set; }
    public Guid? ManagerReviewFinishedByEmployeeId { get; set; }
    public string? ManagerReviewFinishedByEmployeeName { get; set; }
    public string? ManagerReviewSummary { get; set; }
    public DateTime? EmployeeReviewConfirmedDate { get; set; }
    public Guid? EmployeeReviewConfirmedByEmployeeId { get; set; }
    public string? EmployeeReviewConfirmedByEmployeeName { get; set; }
    public string? EmployeeReviewComments { get; set; }

    // Final state
    public DateTime? FinalizedDate { get; set; }
    public Guid? FinalizedByEmployeeId { get; set; }
    public string? FinalizedByEmployeeName { get; set; }
    public string? ManagerFinalNotes { get; set; }
    public bool IsLocked { get; set; }

    // Reopen tracking (audit trail)
    public DateTime? LastReopenedDate { get; set; }
    public Guid? LastReopenedByEmployeeId { get; set; }
    public string? LastReopenedByEmployeeName { get; set; }
    public string? LastReopenedByRole { get; set; }
    public string? LastReopenReason { get; set; }
}
