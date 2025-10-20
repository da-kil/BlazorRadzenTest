using ti8m.BeachBreak.Core.Domain.SharedKernel;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

namespace ti8m.BeachBreak.Application.Query.Projections;

public class QuestionnaireAssignmentReadModel
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public bool RequiresManagerReview { get; set; } = true;
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
    public Guid? WithdrawnByEmployeeId { get; set; }
    public string? WithdrawalReason { get; set; }

    // Workflow properties
    public WorkflowState WorkflowState { get; set; } = WorkflowState.Assigned;
    public List<SectionProgressDto> SectionProgress { get; set; } = new();

    // Submission phase
    public DateTime? EmployeeSubmittedDate { get; set; }
    public Guid? EmployeeSubmittedByEmployeeId { get; set; }
    public DateTime? ManagerSubmittedDate { get; set; }
    public Guid? ManagerSubmittedByEmployeeId { get; set; }

    // Review phase
    public DateTime? ReviewInitiatedDate { get; set; }
    public Guid? ReviewInitiatedByEmployeeId { get; set; }
    public DateTime? ManagerReviewFinishedDate { get; set; }
    public Guid? ManagerReviewFinishedByEmployeeId { get; set; }
    public string? ManagerReviewSummary { get; set; }
    public DateTime? EmployeeReviewConfirmedDate { get; set; }
    public Guid? EmployeeReviewConfirmedByEmployeeId { get; set; }
    public string? EmployeeReviewComments { get; set; }

    // Final state
    public DateTime? FinalizedDate { get; set; }
    public Guid? FinalizedByEmployeeId { get; set; }
    public string? ManagerFinalNotes { get; set; }
    public bool IsLocked => WorkflowState == WorkflowState.Finalized;

    // Apply methods for all QuestionnaireAssignment domain events
    public void Apply(QuestionnaireAssignmentAssigned @event)
    {
        Id = @event.AggregateId;
        TemplateId = @event.TemplateId;
        RequiresManagerReview = @event.RequiresManagerReview;
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
        WithdrawnByEmployeeId = @event.WithdrawnByEmployeeId;
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

    public void Apply(EmployeeQuestionnaireSubmitted @event)
    {
        EmployeeSubmittedDate = @event.SubmittedDate;
        EmployeeSubmittedByEmployeeId = @event.SubmittedByEmployeeId;
        UpdateWorkflowStateOnSubmission();
    }

    public void Apply(ManagerQuestionnaireSubmitted @event)
    {
        ManagerSubmittedDate = @event.SubmittedDate;
        ManagerSubmittedByEmployeeId = @event.SubmittedByEmployeeId;
        UpdateWorkflowStateOnSubmission();
    }

    public void Apply(ReviewInitiated @event)
    {
        ReviewInitiatedDate = @event.InitiatedDate;
        ReviewInitiatedByEmployeeId = @event.InitiatedByEmployeeId;
        WorkflowState = WorkflowState.InReview;
    }

    public void Apply(AnswerEditedDuringReview @event)
    {
        // Answer changes are tracked separately
        // This event is for audit trail purposes
    }

    public void Apply(ManagerEditedAnswerDuringReview @event)
    {
        // Answer changes are tracked in ReviewChangeLog projection
        // This event is for audit trail purposes only
    }

    public void Apply(ManagerReviewMeetingFinished @event)
    {
        WorkflowState = WorkflowState.ManagerReviewConfirmed;
        ManagerReviewFinishedDate = @event.FinishedDate;
        ManagerReviewFinishedByEmployeeId = @event.FinishedByEmployeeId;
        ManagerReviewSummary = @event.ReviewSummary;
    }

    public void Apply(EmployeeConfirmedReviewOutcome @event)
    {
        WorkflowState = WorkflowState.EmployeeReviewConfirmed;
        EmployeeReviewConfirmedDate = @event.ConfirmedDate;
        EmployeeReviewConfirmedByEmployeeId = @event.ConfirmedByEmployeeId;
        EmployeeReviewComments = @event.EmployeeComments;
    }

    public void Apply(ManagerFinalizedQuestionnaire @event)
    {
        WorkflowState = WorkflowState.Finalized;
        FinalizedDate = @event.FinalizedDate;
        FinalizedByEmployeeId = @event.FinalizedByEmployeeId;
        ManagerFinalNotes = @event.ManagerFinalNotes;
    }

    public void Apply(QuestionnaireAutoFinalized @event)
    {
        WorkflowState = WorkflowState.Finalized;
        FinalizedDate = @event.FinalizedDate;
        FinalizedByEmployeeId = @event.FinalizedByEmployeeId;
        ManagerFinalNotes = @event.Reason;
    }

    private void UpdateWorkflowState()
    {
        // CRITICAL: Do not update workflow state if either party has already submitted
        // Once submitted, only submission events (via UpdateWorkflowStateOnSubmission) should change the state
        if (EmployeeSubmittedDate.HasValue || ManagerSubmittedDate.HasValue)
        {
            return; // Preserve the current submitted state
        }

        var hasEmployeeProgress = SectionProgress.Any(p => p.IsEmployeeCompleted);
        var hasManagerProgress = SectionProgress.Any(p => p.IsManagerCompleted);

        if (hasEmployeeProgress && hasManagerProgress)
        {
            WorkflowState = WorkflowState.BothInProgress;
        }
        else if (hasEmployeeProgress)
        {
            WorkflowState = WorkflowState.EmployeeInProgress;
        }
        else if (hasManagerProgress)
        {
            WorkflowState = WorkflowState.ManagerInProgress;
        }
    }

    private void UpdateWorkflowStateOnSubmission()
    {
        if (EmployeeSubmittedDate.HasValue && ManagerSubmittedDate.HasValue)
        {
            WorkflowState = WorkflowState.BothSubmitted;
        }
        else if (EmployeeSubmittedDate.HasValue)
        {
            WorkflowState = WorkflowState.EmployeeSubmitted;
        }
        else if (ManagerSubmittedDate.HasValue)
        {
            WorkflowState = WorkflowState.ManagerSubmitted;
        }
    }
}