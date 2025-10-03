using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

namespace ti8m.BeachBreak.Application.Query.Projections;

public class QuestionnaireAssignmentReadModel
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsWithdrawn { get; set; }
    public DateTime? WithdrawnDate { get; set; }
    public string? WithdrawnBy { get; set; }
    public string? WithdrawalReason { get; set; }

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
    public bool IsLocked => WorkflowState == "Finalized";

    // Computed properties for compatibility with existing UI
    public AssignmentStatus Status => DetermineStatus();

    private AssignmentStatus DetermineStatus()
    {
        if (IsWithdrawn) return AssignmentStatus.Withdrawn;
        if (CompletedDate.HasValue) return AssignmentStatus.Completed;
        if (StartedDate.HasValue) return AssignmentStatus.InProgress;
        if (DueDate.HasValue && DueDate.Value < DateTime.UtcNow) return AssignmentStatus.Overdue;
        return AssignmentStatus.Assigned;
    }

    // Apply methods for all QuestionnaireAssignment domain events
    public void Apply(QuestionnaireAssignmentAssigned @event)
    {
        Id = @event.AggregateId;
        TemplateId = @event.TemplateId;
        EmployeeId = @event.EmployeeId;
        EmployeeName = @event.EmployeeName;
        EmployeeEmail = @event.EmployeeEmail;
        AssignedDate = @event.AssignedDate;
        DueDate = @event.DueDate;
        AssignedBy = @event.AssignedBy;
        Notes = @event.Notes;
    }

    public void Apply(AssignmentWorkStarted @event)
    {
        StartedDate = @event.StartedDate;
    }

    public void Apply(AssignmentWorkCompleted @event)
    {
        CompletedDate = @event.CompletedDate;
    }

    public void Apply(AssignmentDueDateExtended @event)
    {
        DueDate = @event.NewDueDate;
    }

    public void Apply(AssignmentWithdrawn @event)
    {
        IsWithdrawn = true;
        WithdrawnDate = @event.WithdrawnDate;
        WithdrawnBy = @event.WithdrawnBy;
        WithdrawalReason = @event.WithdrawalReason;
    }

    public void Apply(EmployeeSectionCompleted @event)
    {
        var existingProgress = SectionProgress.FirstOrDefault(p => p.SectionId == @event.SectionId);
        if (existingProgress != null)
        {
            existingProgress.IsEmployeeCompleted = true;
            existingProgress.EmployeeCompletedDate = @event.CompletedDate;
        }
        else
        {
            SectionProgress.Add(new SectionProgressDto
            {
                SectionId = @event.SectionId,
                IsEmployeeCompleted = true,
                EmployeeCompletedDate = @event.CompletedDate
            });
        }

        UpdateWorkflowState();
    }

    public void Apply(ManagerSectionCompleted @event)
    {
        var existingProgress = SectionProgress.FirstOrDefault(p => p.SectionId == @event.SectionId);
        if (existingProgress != null)
        {
            existingProgress.IsManagerCompleted = true;
            existingProgress.ManagerCompletedDate = @event.CompletedDate;
        }
        else
        {
            SectionProgress.Add(new SectionProgressDto
            {
                SectionId = @event.SectionId,
                IsManagerCompleted = true,
                ManagerCompletedDate = @event.CompletedDate
            });
        }

        UpdateWorkflowState();
    }

    public void Apply(EmployeeCompletionConfirmed @event)
    {
        EmployeeConfirmedDate = @event.ConfirmedDate;
        EmployeeConfirmedBy = @event.ConfirmedBy;
        UpdateWorkflowStateOnConfirmation();
    }

    public void Apply(ManagerCompletionConfirmed @event)
    {
        ManagerConfirmedDate = @event.ConfirmedDate;
        ManagerConfirmedBy = @event.ConfirmedBy;
        UpdateWorkflowStateOnConfirmation();
    }

    public void Apply(ReviewInitiated @event)
    {
        ReviewInitiatedDate = @event.InitiatedDate;
        ReviewInitiatedBy = @event.InitiatedBy;
        WorkflowState = "InReview";
    }

    public void Apply(AnswerEditedDuringReview @event)
    {
        // Answer changes are tracked separately
        // This event is for audit trail purposes
    }

    public void Apply(EmployeeReviewConfirmed @event)
    {
        EmployeeReviewConfirmedDate = @event.ConfirmedDate;
        EmployeeReviewConfirmedBy = @event.ConfirmedBy;
        WorkflowState = "EmployeeReviewConfirmed";
    }

    public void Apply(QuestionnaireFinalized @event)
    {
        FinalizedDate = @event.FinalizedDate;
        FinalizedBy = @event.FinalizedBy;
        WorkflowState = "Finalized";
    }

    private void UpdateWorkflowState()
    {
        var hasEmployeeProgress = SectionProgress.Any(p => p.IsEmployeeCompleted);
        var hasManagerProgress = SectionProgress.Any(p => p.IsManagerCompleted);

        if (hasEmployeeProgress && hasManagerProgress)
        {
            WorkflowState = "BothInProgress";
        }
        else if (hasEmployeeProgress)
        {
            WorkflowState = "EmployeeInProgress";
        }
        else if (hasManagerProgress)
        {
            WorkflowState = "ManagerInProgress";
        }
    }

    private void UpdateWorkflowStateOnConfirmation()
    {
        if (EmployeeConfirmedDate.HasValue && ManagerConfirmedDate.HasValue)
        {
            WorkflowState = "BothConfirmed";
        }
        else if (EmployeeConfirmedDate.HasValue)
        {
            WorkflowState = "EmployeeConfirmed";
        }
        else if (ManagerConfirmedDate.HasValue)
        {
            WorkflowState = "ManagerConfirmed";
        }
    }
}