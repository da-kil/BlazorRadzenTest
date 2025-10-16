using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Core.Domain.SharedKernel;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

public class QuestionnaireAssignment : AggregateRoot
{
    public Guid TemplateId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string EmployeeName { get; private set; }
    public string EmployeeEmail { get; private set; }
    public DateTime AssignedDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public string? AssignedBy { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? StartedDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public bool IsWithdrawn { get; private set; }
    public DateTime? WithdrawnDate { get; private set; }
    public string? WithdrawnBy { get; private set; }
    public string? WithdrawalReason { get; private set; }

    // Workflow properties
    public WorkflowState WorkflowState { get; private set; } = WorkflowState.Assigned;
    public List<SectionProgress> SectionProgressList { get; private set; } = new();

    // Submission phase
    public DateTime? EmployeeSubmittedDate { get; private set; }
    public string? EmployeeSubmittedBy { get; private set; }
    public DateTime? ManagerSubmittedDate { get; private set; }
    public string? ManagerSubmittedBy { get; private set; }

    // Review phase
    public DateTime? ReviewInitiatedDate { get; private set; }
    public string? ReviewInitiatedBy { get; private set; }
    public DateTime? ManagerReviewFinishedDate { get; private set; }
    public string? ManagerReviewFinishedBy { get; private set; }
    public string? ManagerReviewSummary { get; private set; }
    public DateTime? EmployeeReviewConfirmedDate { get; private set; }
    public string? EmployeeReviewConfirmedBy { get; private set; }
    public string? EmployeeReviewComments { get; private set; }

    // Final state
    public DateTime? FinalizedDate { get; private set; }
    public string? FinalizedBy { get; private set; }
    public string? ManagerFinalNotes { get; private set; }
    public bool IsLocked => WorkflowState == WorkflowState.Finalized;

    private QuestionnaireAssignment() { }

    public QuestionnaireAssignment(
        Guid id,
        Guid templateId,
        Guid employeeId,
        string employeeName,
        string employeeEmail,
        DateTime assignedDate,
        DateTime? dueDate,
        string? assignedBy,
        string? notes)
    {
        RaiseEvent(new QuestionnaireAssignmentAssigned(
            id,
            templateId,
            employeeId,
            employeeName,
            employeeEmail,
            assignedDate,
            dueDate,
            assignedBy,
            notes));
    }

    public void StartWork()
    {
        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot start work on a withdrawn assignment");

        if (CompletedDate.HasValue)
            throw new InvalidOperationException("Cannot start work on an already completed assignment");

        if (!StartedDate.HasValue)
        {
            RaiseEvent(new AssignmentWorkStarted(DateTime.UtcNow));
        }
    }

    public void CompleteWork()
    {
        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot complete a withdrawn assignment");

        if (CompletedDate.HasValue)
            throw new InvalidOperationException("Assignment is already completed");

        if (!StartedDate.HasValue)
        {
            // Auto-start if not already started
            RaiseEvent(new AssignmentWorkStarted(DateTime.UtcNow));
        }

        RaiseEvent(new AssignmentWorkCompleted(DateTime.UtcNow));
    }

    public void ExtendDueDate(DateTime newDueDate, string? extensionReason = null)
    {
        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot extend due date of a withdrawn assignment");

        if (CompletedDate.HasValue)
            throw new InvalidOperationException("Cannot extend due date of a completed assignment");

        if (DueDate != newDueDate)
        {
            RaiseEvent(new AssignmentDueDateExtended(newDueDate, DateTime.UtcNow, extensionReason));
        }
    }

    public void Withdraw(string withdrawnBy, string? withdrawalReason = null)
    {
        if (IsWithdrawn)
            throw new InvalidOperationException("Assignment is already withdrawn");

        if (CompletedDate.HasValue)
            throw new InvalidOperationException("Cannot withdraw a completed assignment");

        RaiseEvent(new AssignmentWithdrawn(DateTime.UtcNow, withdrawnBy, withdrawalReason));
    }

    // Workflow state query methods (business rules)
    /// <summary>
    /// Determines if an employee can edit the questionnaire based on current workflow state.
    /// Employee can edit when: Assigned, EmployeeInProgress, BothInProgress, ManagerSubmitted.
    /// Employee is READ-ONLY during: InReview (manager-led review meeting).
    /// Employee is blocked after submission: EmployeeSubmitted, BothSubmitted, and all review/final states.
    /// </summary>
    public bool CanEmployeeEdit()
    {
        return WorkflowState is
            WorkflowState.Assigned or
            WorkflowState.EmployeeInProgress or
            WorkflowState.BothInProgress or
            WorkflowState.ManagerSubmitted;
    }

    /// <summary>
    /// Determines if a manager can edit the questionnaire based on current workflow state.
    /// Manager can edit when: Assigned, ManagerInProgress, BothInProgress, EmployeeSubmitted.
    /// Manager can edit ALL sections during: InReview (including employee-only sections).
    /// Manager is blocked after submission: ManagerSubmitted, BothSubmitted, and all confirmation/final states.
    /// </summary>
    public bool CanManagerEdit()
    {
        return WorkflowState is
            WorkflowState.Assigned or
            WorkflowState.ManagerInProgress or
            WorkflowState.BothInProgress or
            WorkflowState.EmployeeSubmitted;
    }

    /// <summary>
    /// Determines if a manager can edit during the review meeting (InReview state).
    /// Manager has special edit permissions during review - can edit ALL sections including employee-only.
    /// </summary>
    public bool CanManagerEditDuringReview()
    {
        return WorkflowState == WorkflowState.InReview;
    }

    /// <summary>
    /// Determines if an employee has read-only access during review.
    /// Employee cannot edit but can view during InReview state.
    /// </summary>
    public bool IsEmployeeReadOnlyDuringReview()
    {
        return WorkflowState == WorkflowState.InReview;
    }

    // Workflow methods
    public void CompleteSectionAsEmployee(Guid sectionId)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot complete section - questionnaire is finalized");

        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot complete section - assignment is withdrawn");

        var progress = SectionProgressList.FirstOrDefault(p => p.SectionId == sectionId);
        if (progress?.IsEmployeeCompleted == true)
            throw new InvalidOperationException("Section already completed by employee");

        RaiseEvent(new EmployeeSectionCompleted(sectionId, DateTime.UtcNow));
    }

    public void CompleteBulkSectionsAsEmployee(List<Guid> sectionIds)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot complete sections - questionnaire is finalized");

        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot complete sections - assignment is withdrawn");

        if (sectionIds == null || !sectionIds.Any())
            throw new ArgumentException("Section IDs list cannot be null or empty", nameof(sectionIds));

        var completedDate = DateTime.UtcNow;

        foreach (var sectionId in sectionIds)
        {
            var progress = SectionProgressList.FirstOrDefault(p => p.SectionId == sectionId);
            if (progress?.IsEmployeeCompleted == true)
            {
                // Skip already completed sections instead of throwing
                continue;
            }

            RaiseEvent(new EmployeeSectionCompleted(sectionId, completedDate));
        }
    }

    public void CompleteSectionAsManager(Guid sectionId)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot complete section - questionnaire is finalized");

        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot complete section - assignment is withdrawn");

        var progress = SectionProgressList.FirstOrDefault(p => p.SectionId == sectionId);
        if (progress?.IsManagerCompleted == true)
            throw new InvalidOperationException("Section already completed by manager");

        RaiseEvent(new ManagerSectionCompleted(sectionId, DateTime.UtcNow));
    }

    public void CompleteBulkSectionsAsManager(List<Guid> sectionIds)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot complete sections - questionnaire is finalized");

        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot complete sections - assignment is withdrawn");

        if (sectionIds == null || !sectionIds.Any())
            throw new ArgumentException("Section IDs list cannot be null or empty", nameof(sectionIds));

        var completedDate = DateTime.UtcNow;

        foreach (var sectionId in sectionIds)
        {
            var progress = SectionProgressList.FirstOrDefault(p => p.SectionId == sectionId);
            if (progress?.IsManagerCompleted == true)
            {
                // Skip already completed sections instead of throwing
                continue;
            }

            RaiseEvent(new ManagerSectionCompleted(sectionId, completedDate));
        }
    }

    // Submit methods (Phase 1)
    public void SubmitEmployeeQuestionnaire(string submittedBy)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot submit - questionnaire is finalized");

        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot submit - assignment is withdrawn");

        if (WorkflowState == WorkflowState.EmployeeSubmitted ||
            WorkflowState == WorkflowState.BothSubmitted)
            throw new InvalidOperationException("Employee questionnaire already submitted");

        // Employee can submit when in progress OR when manager has already submitted
        if (WorkflowState != WorkflowState.EmployeeInProgress &&
            WorkflowState != WorkflowState.BothInProgress &&
            WorkflowState != WorkflowState.ManagerSubmitted)
            throw new InvalidOperationException("Employee must have started filling sections before submitting");

        RaiseEvent(new EmployeeQuestionnaireSubmitted(DateTime.UtcNow, submittedBy));
    }

    public void SubmitManagerQuestionnaire(string submittedBy)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot submit - questionnaire is finalized");

        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot submit - assignment is withdrawn");

        if (WorkflowState == WorkflowState.ManagerSubmitted ||
            WorkflowState == WorkflowState.BothSubmitted)
            throw new InvalidOperationException("Manager questionnaire already submitted");

        // Manager can submit when in progress OR when employee has already submitted
        if (WorkflowState != WorkflowState.ManagerInProgress &&
            WorkflowState != WorkflowState.BothInProgress &&
            WorkflowState != WorkflowState.EmployeeSubmitted)
            throw new InvalidOperationException("Manager must have started filling sections before submitting");

        RaiseEvent(new ManagerQuestionnaireSubmitted(DateTime.UtcNow, submittedBy));
    }

    public void InitiateReview(string initiatedBy)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot initiate review - questionnaire is finalized");

        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot initiate review - assignment is withdrawn");

        if (WorkflowState != WorkflowState.BothSubmitted)
            throw new InvalidOperationException("Both employee and manager must submit their questionnaires before review");

        RaiseEvent(new ReviewInitiated(DateTime.UtcNow, initiatedBy));
    }

    public void EditAnswerAsManagerDuringReview(
        Guid sectionId,
        Guid questionId,
        CompletionRole originalCompletionRole,
        string newAnswer,
        string editedBy)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot edit answer - questionnaire is finalized");

        if (WorkflowState != WorkflowState.InReview)
            throw new InvalidOperationException("Answers can only be edited during review meeting");

        RaiseEvent(new ManagerEditedAnswerDuringReview(
            Id,
            sectionId,
            questionId,
            originalCompletionRole,
            newAnswer,
            DateTime.UtcNow,
            editedBy
        ));
    }

    public void FinishReviewMeeting(string finishedBy, string? reviewSummary)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot finish review - questionnaire is finalized");

        if (WorkflowState != WorkflowState.InReview)
            throw new InvalidOperationException("No active review meeting to finish");

        RaiseEvent(new ManagerReviewMeetingFinished(
            Id,
            DateTime.UtcNow,
            finishedBy,
            reviewSummary
        ));
    }

    public void ConfirmReviewOutcomeAsEmployee(string confirmedBy, string? comments)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot confirm review - questionnaire is finalized");

        if (WorkflowState != WorkflowState.ManagerReviewConfirmed)
            throw new InvalidOperationException("Manager must finish review meeting before employee confirmation");

        RaiseEvent(new EmployeeConfirmedReviewOutcome(
            Id,
            DateTime.UtcNow,
            confirmedBy,
            comments
        ));
    }

    public void FinalizeAsManager(string finalizedBy, string? finalNotes)
    {
        if (IsLocked)
            throw new InvalidOperationException("Questionnaire is already finalized");

        if (WorkflowState != WorkflowState.EmployeeReviewConfirmed)
            throw new InvalidOperationException("Employee must confirm review before manager can finalize");

        RaiseEvent(new ManagerFinalizedQuestionnaire(
            Id,
            DateTime.UtcNow,
            finalizedBy,
            finalNotes
        ));
    }

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
        IsWithdrawn = false;
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
        var existingProgress = SectionProgressList.FirstOrDefault(p => p.SectionId == @event.SectionId);
        if (existingProgress != null)
        {
            SectionProgressList.Remove(existingProgress);
            SectionProgressList.Add(existingProgress.MarkEmployeeCompleted(@event.CompletedDate));
        }
        else
        {
            var newProgress = new SectionProgress(@event.SectionId).MarkEmployeeCompleted(@event.CompletedDate);
            SectionProgressList.Add(newProgress);
        }

        UpdateWorkflowState();
    }

    public void Apply(ManagerSectionCompleted @event)
    {
        var existingProgress = SectionProgressList.FirstOrDefault(p => p.SectionId == @event.SectionId);
        if (existingProgress != null)
        {
            SectionProgressList.Remove(existingProgress);
            SectionProgressList.Add(existingProgress.MarkManagerCompleted(@event.CompletedDate));
        }
        else
        {
            var newProgress = new SectionProgress(@event.SectionId).MarkManagerCompleted(@event.CompletedDate);
            SectionProgressList.Add(newProgress);
        }

        UpdateWorkflowState();
    }

    public void Apply(EmployeeQuestionnaireSubmitted @event)
    {
        EmployeeSubmittedDate = @event.SubmittedDate;
        EmployeeSubmittedBy = @event.SubmittedBy;
        UpdateWorkflowStateOnSubmission();
    }

    public void Apply(ManagerQuestionnaireSubmitted @event)
    {
        ManagerSubmittedDate = @event.SubmittedDate;
        ManagerSubmittedBy = @event.SubmittedBy;
        UpdateWorkflowStateOnSubmission();
    }

    public void Apply(ReviewInitiated @event)
    {
        ReviewInitiatedDate = @event.InitiatedDate;
        ReviewInitiatedBy = @event.InitiatedBy;
        WorkflowState = WorkflowState.InReview;
    }

    public void Apply(AnswerEditedDuringReview @event)
    {
        // Answer changes are tracked in a separate aggregate or projection
        // This event is for audit trail purposes
    }

    // Apply methods for refined review workflow
    public void Apply(ManagerEditedAnswerDuringReview @event)
    {
        // Answer changes are tracked in ReviewChangeLog projection
        // This event is for audit trail purposes only
    }

    public void Apply(ManagerReviewMeetingFinished @event)
    {
        WorkflowState = WorkflowState.ManagerReviewConfirmed;
        ManagerReviewFinishedDate = @event.FinishedDate;
        ManagerReviewFinishedBy = @event.FinishedBy;
        ManagerReviewSummary = @event.ReviewSummary;
    }

    public void Apply(EmployeeConfirmedReviewOutcome @event)
    {
        WorkflowState = WorkflowState.EmployeeReviewConfirmed;
        EmployeeReviewConfirmedDate = @event.ConfirmedDate;
        EmployeeReviewConfirmedBy = @event.ConfirmedBy;
        EmployeeReviewComments = @event.EmployeeComments;
    }

    public void Apply(ManagerFinalizedQuestionnaire @event)
    {
        WorkflowState = WorkflowState.Finalized;
        FinalizedDate = @event.FinalizedDate;
        FinalizedBy = @event.FinalizedBy;
        ManagerFinalNotes = @event.ManagerFinalNotes;
    }

    private void UpdateWorkflowState()
    {
        // Don't update state if already in submission, review, or finalization phases
        // Only update during the initial work-in-progress phase
        if (WorkflowState >= WorkflowState.EmployeeSubmitted)
        {
            // Once submitted or beyond, section completion doesn't change workflow state
            return;
        }

        var hasEmployeeProgress = SectionProgressList.Any(p => p.IsEmployeeCompleted);
        var hasManagerProgress = SectionProgressList.Any(p => p.IsManagerCompleted);

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