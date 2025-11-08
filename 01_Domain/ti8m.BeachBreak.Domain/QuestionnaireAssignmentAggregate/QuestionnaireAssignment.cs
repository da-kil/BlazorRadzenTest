using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

public class QuestionnaireAssignment : AggregateRoot
{
    public Guid TemplateId { get; private set; }
    public bool RequiresManagerReview { get; private set; } = true;
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
    public Guid? WithdrawnByEmployeeId { get; private set; }
    public string? WithdrawalReason { get; private set; }

    // Workflow properties
    public WorkflowState WorkflowState { get; private set; } = WorkflowState.Assigned;
    public List<SectionProgress> SectionProgressList { get; private set; } = new();

    // Submission phase
    public DateTime? EmployeeSubmittedDate { get; private set; }
    public Guid? EmployeeSubmittedByEmployeeId { get; private set; }
    public DateTime? ManagerSubmittedDate { get; private set; }
    public Guid? ManagerSubmittedByEmployeeId { get; private set; }

    // Review phase
    public DateTime? ReviewInitiatedDate { get; private set; }
    public Guid? ReviewInitiatedByEmployeeId { get; private set; }
    public DateTime? ManagerReviewFinishedDate { get; private set; }
    public Guid? ManagerReviewFinishedByEmployeeId { get; private set; }
    public string? ManagerReviewSummary { get; private set; }
    public DateTime? EmployeeReviewConfirmedDate { get; private set; }
    public Guid? EmployeeReviewConfirmedByEmployeeId { get; private set; }
    public string? EmployeeReviewComments { get; private set; }

    // Final state
    public DateTime? FinalizedDate { get; private set; }
    public Guid? FinalizedByEmployeeId { get; private set; }
    public string? ManagerFinalNotes { get; private set; }
    public bool IsLocked => WorkflowState == WorkflowState.Finalized;

    // Goal management (for Goal question type)
    private readonly Dictionary<Guid, List<Goal>> _goalsByQuestion = new();
    private readonly Dictionary<Guid, Guid> _predecessorLinks = new();
    private readonly Dictionary<Guid, List<GoalRating>> _goalRatingsByQuestion = new();

    // Public readonly accessors for query purposes
    public IReadOnlyDictionary<Guid, IReadOnlyList<Goal>> GoalsByQuestion =>
        _goalsByQuestion.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<Goal>)kvp.Value.AsReadOnly());

    public IReadOnlyDictionary<Guid, Guid> PredecessorLinks => _predecessorLinks;

    public IReadOnlyDictionary<Guid, IReadOnlyList<GoalRating>> GoalRatingsByQuestion =>
        _goalRatingsByQuestion.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<GoalRating>)kvp.Value.AsReadOnly());

    private QuestionnaireAssignment() { }

    public QuestionnaireAssignment(
        Guid id,
        Guid templateId,
        bool requiresManagerReview,
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
            requiresManagerReview,
            employeeId,
            employeeName,
            employeeEmail,
            assignedDate,
            dueDate,
            assignedBy,
            notes));
    }

    public void StartWork(ApplicationRole? startedBy = null)
    {
        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot start work on a withdrawn assignment");

        if (CompletedDate.HasValue)
            throw new InvalidOperationException("Cannot start work on an already completed assignment");

        if (!StartedDate.HasValue)
        {
            RaiseEvent(new AssignmentWorkStarted(DateTime.UtcNow));

            // Update workflow state based on having started work (even without completed sections)
            // This ensures questionnaires move from Assigned to appropriate InProgress state when first response is saved
            if (startedBy.HasValue)
            {
                UpdateWorkflowStateFromStartedWork(startedBy.Value);
            }
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

    public void Withdraw(Guid withdrawnByEmployeeId, string? withdrawalReason = null)
    {
        if (IsWithdrawn)
            throw new InvalidOperationException("Assignment is already withdrawn");

        if (CompletedDate.HasValue)
            throw new InvalidOperationException("Cannot withdraw a completed assignment");

        RaiseEvent(new AssignmentWithdrawn(DateTime.UtcNow, withdrawnByEmployeeId, withdrawalReason));
    }

    // Workflow state query methods (business rules)
    /// <summary>
    /// Determines if an employee can edit the questionnaire based on current workflow state.
    /// Employee can edit when: Assigned, EmployeeInProgress, ManagerInProgress, BothInProgress, ManagerSubmitted.
    /// Employee is READ-ONLY during: InReview (manager-led review meeting).
    /// Employee is blocked after submission: EmployeeSubmitted, BothSubmitted, and all review/final states.
    /// </summary>
    public bool CanEmployeeEdit()
    {
        return WorkflowState is
            WorkflowState.Assigned or
            WorkflowState.EmployeeInProgress or
            WorkflowState.ManagerInProgress or
            WorkflowState.BothInProgress or
            WorkflowState.ManagerSubmitted;
    }

    /// <summary>
    /// Determines if a manager can edit the questionnaire based on current workflow state.
    /// Manager can edit when: Assigned, EmployeeInProgress, ManagerInProgress, BothInProgress, EmployeeSubmitted.
    /// Manager can edit ALL sections during: InReview (including employee-only sections).
    /// Manager is blocked after submission: ManagerSubmitted, BothSubmitted, and all confirmation/final states.
    /// </summary>
    public bool CanManagerEdit()
    {
        return WorkflowState is
            WorkflowState.Assigned or
            WorkflowState.EmployeeInProgress or
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

        // Enforce workflow state restrictions for questionnaire modifications
        if (WorkflowState == WorkflowState.BothSubmitted)
            throw new InvalidOperationException("Cannot complete section - both questionnaires have been submitted");

        if (WorkflowState == WorkflowState.EmployeeSubmitted)
            throw new InvalidOperationException("Cannot complete section - employee questionnaire has been submitted");

        if (WorkflowState == WorkflowState.InReview)
            throw new InvalidOperationException("Cannot complete section - questionnaire is in review");

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

        // Enforce workflow state restrictions for questionnaire modifications
        if (WorkflowState == WorkflowState.BothSubmitted)
            throw new InvalidOperationException("Cannot complete sections - both questionnaires have been submitted");

        if (WorkflowState == WorkflowState.EmployeeSubmitted)
            throw new InvalidOperationException("Cannot complete sections - employee questionnaire has been submitted");

        if (WorkflowState == WorkflowState.InReview)
            throw new InvalidOperationException("Cannot complete sections - questionnaire is in review");

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

        // Enforce workflow state restrictions for questionnaire modifications
        if (WorkflowState == WorkflowState.BothSubmitted)
            throw new InvalidOperationException("Cannot complete section - both questionnaires have been submitted");

        if (WorkflowState == WorkflowState.ManagerSubmitted)
            throw new InvalidOperationException("Cannot complete section - manager questionnaire has been submitted");

        if (WorkflowState == WorkflowState.InReview)
            throw new InvalidOperationException("Cannot complete section - questionnaire is in review");

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

        // Enforce workflow state restrictions for questionnaire modifications
        if (WorkflowState == WorkflowState.BothSubmitted)
            throw new InvalidOperationException("Cannot complete sections - both questionnaires have been submitted");

        if (WorkflowState == WorkflowState.ManagerSubmitted)
            throw new InvalidOperationException("Cannot complete sections - manager questionnaire has been submitted");

        if (WorkflowState == WorkflowState.InReview)
            throw new InvalidOperationException("Cannot complete sections - questionnaire is in review");

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
    public void SubmitEmployeeQuestionnaire(Guid submittedByEmployeeId)
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

        RaiseEvent(new EmployeeQuestionnaireSubmitted(DateTime.UtcNow, submittedByEmployeeId));

        // Auto-finalize if manager review is not required
        if (!RequiresManagerReview)
        {
            RaiseEvent(new QuestionnaireAutoFinalized(
                Id,
                DateTime.UtcNow,
                submittedByEmployeeId,
                "Auto-finalized: Manager review not required"));
        }
    }

    public void SubmitManagerQuestionnaire(Guid submittedByEmployeeId)
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

        RaiseEvent(new ManagerQuestionnaireSubmitted(DateTime.UtcNow, submittedByEmployeeId));
    }

    public void InitiateReview(Guid initiatedByEmployeeId)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot initiate review - questionnaire is finalized");

        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot initiate review - assignment is withdrawn");

        if (WorkflowState != WorkflowState.BothSubmitted)
            throw new InvalidOperationException("Both employee and manager must submit their questionnaires before review");

        // Validate that all predecessor goals have been rated by both Employee and Manager roles
        ValidatePredecessorGoalsCompletion();

        RaiseEvent(new ReviewInitiated(DateTime.UtcNow, initiatedByEmployeeId));
    }

    public void EditAnswerAsManagerDuringReview(
        Guid sectionId,
        Guid questionId,
        ApplicationRole originalCompletionRole,
        string newAnswer,
        Guid editedByEmployeeId)
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
            editedByEmployeeId
        ));
    }

    public void FinishReviewMeeting(Guid finishedByEmployeeId, string? reviewSummary)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot finish review - questionnaire is finalized");

        if (WorkflowState != WorkflowState.InReview)
            throw new InvalidOperationException("No active review meeting to finish");

        // Validate that all predecessor goals have been rated by both Employee and Manager roles
        ValidatePredecessorGoalsCompletion();

        RaiseEvent(new ManagerReviewMeetingFinished(
            Id,
            DateTime.UtcNow,
            finishedByEmployeeId,
            reviewSummary
        ));
    }

    public void ConfirmReviewOutcomeAsEmployee(Guid confirmedByEmployeeId, string? comments)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot confirm review - questionnaire is finalized");

        if (WorkflowState != WorkflowState.ManagerReviewConfirmed)
            throw new InvalidOperationException("Manager must finish review meeting before employee confirmation");

        // Only the assigned employee can confirm their own review
        if (confirmedByEmployeeId != EmployeeId)
            throw new InvalidOperationException("Only the assigned employee can confirm the review outcome");

        RaiseEvent(new EmployeeConfirmedReviewOutcome(
            Id,
            DateTime.UtcNow,
            confirmedByEmployeeId,
            comments
        ));
    }

    public void FinalizeAsManager(Guid finalizedByEmployeeId, string? finalNotes)
    {
        if (IsLocked)
            throw new InvalidOperationException("Questionnaire is already finalized");

        if (WorkflowState != WorkflowState.EmployeeReviewConfirmed)
            throw new InvalidOperationException("Employee must confirm review before manager can finalize");

        RaiseEvent(new ManagerFinalizedQuestionnaire(
            Id,
            DateTime.UtcNow,
            finalizedByEmployeeId,
            finalNotes
        ));
    }

    // Goal management methods
    public void LinkPredecessorQuestionnaire(
        Guid questionId,
        Guid predecessorAssignmentId,
        ApplicationRole linkedByRole,
        Guid linkedByEmployeeId)
    {
        if (_predecessorLinks.ContainsKey(questionId))
            throw new InvalidOperationException($"Question {questionId} already linked to a predecessor questionnaire");

        if (IsLocked)
            throw new InvalidOperationException("Cannot link predecessor - questionnaire is finalized");

        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot link predecessor - assignment is withdrawn");

        // Validate edit permissions based on role
        if (linkedByRole == ApplicationRole.Employee && !CanEmployeeEdit())
            throw new InvalidOperationException($"Employee cannot link predecessor in state {WorkflowState}");

        if (linkedByRole is ApplicationRole.TeamLead or ApplicationRole.HR or ApplicationRole.HRLead or ApplicationRole.Admin && !CanManagerEdit())
            throw new InvalidOperationException($"Manager cannot link predecessor in state {WorkflowState}");

        // Validate role-specific workflow state restrictions
        if (linkedByRole == ApplicationRole.Employee && WorkflowState == WorkflowState.ManagerInProgress)
            throw new InvalidOperationException("Employee cannot link during ManagerInProgress state");

        if (linkedByRole is ApplicationRole.TeamLead or ApplicationRole.HR or ApplicationRole.HRLead or ApplicationRole.Admin && WorkflowState == WorkflowState.EmployeeInProgress)
            throw new InvalidOperationException("Manager cannot link during EmployeeInProgress state");

        if (_goalsByQuestion.ContainsKey(questionId) && _goalsByQuestion[questionId].Any())
            throw new InvalidOperationException("Cannot link predecessor after goals have been added to this question");

        RaiseEvent(new PredecessorQuestionnaireLinked(
            predecessorAssignmentId,
            questionId,
            linkedByRole,
            DateTime.UtcNow,
            linkedByEmployeeId));
    }

    public void AddGoal(
        Guid questionId,
        Guid goalId,
        ApplicationRole addedByRole,
        DateTime timeframeFrom,
        DateTime timeframeTo,
        string objectiveDescription,
        string measurementMetric,
        decimal weightingPercentage,
        Guid addedByEmployeeId)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot add goal - questionnaire is finalized");

        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot add goal - assignment is withdrawn");

        // Validate edit permissions based on role and workflow state
        if (addedByRole == ApplicationRole.Employee && !CanEmployeeEdit())
            throw new InvalidOperationException($"Employee cannot add goals in state {WorkflowState}");

        // Special handling for InReview state - managers can add goals during review meetings
        if (WorkflowState == WorkflowState.InReview)
        {
            // During review, only roles with management authority can add goals
            if (addedByRole is not (ApplicationRole.TeamLead or ApplicationRole.HR or ApplicationRole.HRLead or ApplicationRole.Admin))
                throw new InvalidOperationException("Only manager can add goals during review meeting");
        }
        else
        {
            // Standard validation for non-review states
            if (addedByRole is ApplicationRole.TeamLead or ApplicationRole.HR or ApplicationRole.HRLead or ApplicationRole.Admin && !CanManagerEdit())
                throw new InvalidOperationException($"Manager cannot add goals in state {WorkflowState}");
        }

        // Validate role-specific workflow state restrictions
        if (addedByRole == ApplicationRole.Employee && WorkflowState == WorkflowState.ManagerInProgress)
            throw new InvalidOperationException("Employee cannot add goals during ManagerInProgress state");

        if (addedByRole is ApplicationRole.TeamLead or ApplicationRole.HR or ApplicationRole.HRLead or ApplicationRole.Admin && WorkflowState == WorkflowState.EmployeeInProgress)
            throw new InvalidOperationException("Manager cannot add goals during EmployeeInProgress state");

        ValidateWeightingTotal(questionId, addedByRole, weightingPercentage);
        ValidateTimeframe(timeframeFrom, timeframeTo);

        RaiseEvent(new GoalAdded(
            questionId,
            goalId,
            addedByRole,
            timeframeFrom,
            timeframeTo,
            objectiveDescription,
            measurementMetric,
            weightingPercentage,
            DateTime.UtcNow,
            addedByEmployeeId));
    }

    public void ModifyGoal(
        Guid goalId,
        DateTime? timeframeFrom,
        DateTime? timeframeTo,
        string? objectiveDescription,
        string? measurementMetric,
        decimal? weightingPercentage,
        ApplicationRole modifiedByRole,
        string? changeReason,
        Guid modifiedByEmployeeId)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot modify goal - questionnaire is finalized");

        // Allow modifications during in-progress states (by goal owner) and during review (by manager)
        var goal = _goalsByQuestion.Values.SelectMany(g => g).FirstOrDefault(g => g.Id == goalId);
        if (goal == null)
        {
            // Debug information for goal not found
            var allGoals = _goalsByQuestion.Values.SelectMany(g => g).ToList();
            var goalInfo = allGoals.Any()
                ? string.Join(", ", allGoals.Select(g => $"{g.Id} (Q:{g.QuestionId})"))
                : "no goals found";
            throw new InvalidOperationException($"Goal {goalId} not found. Assignment {Id} in state {WorkflowState} has {allGoals.Count} total goals: {goalInfo}");
        }

        // Validate permissions based on workflow state
        if (WorkflowState == WorkflowState.InReview)
        {
            // During formal review, only roles with management authority can modify any goal
            if (modifiedByRole is not (ApplicationRole.TeamLead or ApplicationRole.HR or ApplicationRole.HRLead or ApplicationRole.Admin))
                throw new InvalidOperationException("Only manager can modify goals during review meeting");

            // Change reason is required during review state
            if (string.IsNullOrWhiteSpace(changeReason))
                throw new ArgumentException("Change reason is required during review", nameof(changeReason));
        }
        else if (WorkflowState == WorkflowState.EmployeeSubmitted)
        {
            // After employee submission, only managers can modify any goal (collaborative editing)
            if (modifiedByRole is not (ApplicationRole.TeamLead or ApplicationRole.HR or ApplicationRole.HRLead or ApplicationRole.Admin))
                throw new InvalidOperationException("Only manager can modify goals after employee submission");

            // Change reason is optional during collaborative editing
        }
        else if (WorkflowState == WorkflowState.ManagerSubmitted)
        {
            // After manager submission, only employees can modify goals they added (collaborative editing)
            if (modifiedByRole is not ApplicationRole.Employee)
                throw new InvalidOperationException("Only employee can modify goals after manager submission");

            // Employee can only modify goals they added
            if (goal.AddedByRole != ApplicationRole.Employee)
                throw new InvalidOperationException("Employee can only modify goals they added");

            // Change reason is optional during collaborative editing
        }
        else if (WorkflowState is WorkflowState.EmployeeInProgress or WorkflowState.ManagerInProgress or WorkflowState.BothInProgress)
        {
            // During in-progress, user can only modify their own goals
            if (goal.AddedByRole != modifiedByRole)
                throw new InvalidOperationException($"Cannot modify goal added by {goal.AddedByRole}");

            // Change reason is optional during in-progress states
        }
        else
        {
            throw new InvalidOperationException($"Goals cannot be modified in state {WorkflowState}");
        }

        if (weightingPercentage.HasValue)
        {
            var questionId = goal.QuestionId;
            var otherGoalsTotal = _goalsByQuestion[questionId]
                .Where(g => g.Id != goalId && g.AddedByRole == goal.AddedByRole)
                .Sum(g => g.WeightingPercentage);

            if (otherGoalsTotal + weightingPercentage.Value > 100m)
                throw new InvalidOperationException($"Total weighting would exceed 100% (current: {otherGoalsTotal}%, adding: {weightingPercentage.Value}%)");
        }

        RaiseEvent(new GoalModified(
            goalId,
            timeframeFrom,
            timeframeTo,
            objectiveDescription,
            measurementMetric,
            weightingPercentage,
            modifiedByRole,
            changeReason,
            DateTime.UtcNow,
            modifiedByEmployeeId));
    }

    public void DeleteGoal(
        Guid goalId,
        Guid deletedByEmployeeId)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot delete goal - questionnaire is finalized");

        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot delete goal - assignment is withdrawn");

        // Find the goal
        var goal = _goalsByQuestion.Values.SelectMany(g => g).FirstOrDefault(g => g.Id == goalId);
        if (goal == null)
        {
            // Debug information for goal not found
            var allGoals = _goalsByQuestion.Values.SelectMany(g => g).ToList();
            var goalInfo = allGoals.Any()
                ? string.Join(", ", allGoals.Select(g => $"{g.Id} (Q:{g.QuestionId})"))
                : "no goals found";
            throw new InvalidOperationException($"Goal {goalId} not found. Assignment {Id} in state {WorkflowState} has {allGoals.Count} total goals: {goalInfo}");
        }

        // Can only delete during in-progress states
        if (WorkflowState is not (WorkflowState.EmployeeInProgress or WorkflowState.ManagerInProgress or WorkflowState.BothInProgress))
            throw new InvalidOperationException($"Goals cannot be deleted in state {WorkflowState}");

        RaiseEvent(new GoalDeleted(
            goalId,
            DateTime.UtcNow,
            deletedByEmployeeId));
    }

    public void RatePredecessorGoal(
        Guid questionId,
        Guid sourceAssignmentId,
        Guid sourceGoalId,
        PredecessorGoalData predecessorGoal,
        ApplicationRole ratedByRole,
        decimal degreeOfAchievement,
        string justification,
        Guid ratedByEmployeeId)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot rate goal - questionnaire is finalized");

        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot rate goal - assignment is withdrawn");

        if (!_predecessorLinks.TryGetValue(questionId, out var linkedPredecessor) || linkedPredecessor != sourceAssignmentId)
            throw new InvalidOperationException("Source assignment not linked as predecessor");

        // Validate edit permissions based on role
        if (ratedByRole == ApplicationRole.Employee && !CanEmployeeEdit())
            throw new InvalidOperationException($"Employee cannot rate goals in state {WorkflowState}");

        if (ratedByRole is ApplicationRole.TeamLead or ApplicationRole.HR or ApplicationRole.HRLead or ApplicationRole.Admin && !CanManagerEdit())
            throw new InvalidOperationException($"Manager cannot rate goals in state {WorkflowState}");

        // Validate role-specific workflow state restrictions
        if (ratedByRole == ApplicationRole.Employee && WorkflowState == WorkflowState.ManagerInProgress)
            throw new InvalidOperationException("Employee cannot rate goals during ManagerInProgress state");

        if (ratedByRole is ApplicationRole.TeamLead or ApplicationRole.HR or ApplicationRole.HRLead or ApplicationRole.Admin && WorkflowState == WorkflowState.EmployeeInProgress)
            throw new InvalidOperationException("Manager cannot rate goals during EmployeeInProgress state");

        if (degreeOfAchievement < 0 || degreeOfAchievement > 100)
            throw new ArgumentException("Degree of achievement must be between 0 and 100", nameof(degreeOfAchievement));

        // Validate justification is required when rating predecessor goals (including 0% achievement)
        if (string.IsNullOrWhiteSpace(justification))
            throw new ArgumentException("Justification is required when rating predecessor goals (including 0% achievement)", nameof(justification));

        RaiseEvent(new PredecessorGoalRated(
            questionId,
            sourceAssignmentId,
            sourceGoalId,
            ratedByRole,
            degreeOfAchievement,
            justification,
            DateTime.UtcNow,
            ratedByEmployeeId,
            predecessorGoal));
    }

    public void ModifyPredecessorGoalRating(
        Guid sourceGoalId,
        ApplicationRole modifiedByRole,
        decimal? degreeOfAchievement,
        string? justification,
        string changeReason,
        Guid modifiedByEmployeeId)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot modify rating - questionnaire is finalized");

        if (WorkflowState != WorkflowState.InReview)
            throw new InvalidOperationException("Ratings can only be modified during review meeting");

        if (string.IsNullOrWhiteSpace(changeReason))
            throw new ArgumentException("Change reason is required", nameof(changeReason));

        var rating = _goalRatingsByQuestion.Values.SelectMany(r => r).FirstOrDefault(r => r.SourceGoalId == sourceGoalId && r.RatedByRole == modifiedByRole);
        if (rating == null)
            throw new InvalidOperationException($"Rating for goal {sourceGoalId} by {modifiedByRole} not found");

        if (degreeOfAchievement.HasValue && (degreeOfAchievement.Value < 0 || degreeOfAchievement.Value > 100))
            throw new ArgumentException("Degree of achievement must be between 0 and 100", nameof(degreeOfAchievement));

        // Validate justification is required when degree is being modified
        // Check both current degree (from existing rating) and new degree being set
        var finalDegree = degreeOfAchievement ?? rating.DegreeOfAchievement;
        var finalJustification = justification ?? rating.Justification;

        if (string.IsNullOrWhiteSpace(finalJustification))
            throw new ArgumentException("Justification is required when modifying predecessor goal ratings (including 0% achievement)", nameof(justification));

        RaiseEvent(new PredecessorGoalRatingModified(
            sourceGoalId,
            modifiedByRole,
            degreeOfAchievement,
            justification,
            changeReason,
            DateTime.UtcNow,
            modifiedByEmployeeId));
    }

    /// <summary>
    /// Gets captured data of a specific goal for rating purposes.
    /// Used when rating goals from predecessor questionnaires.
    /// </summary>
    public PredecessorGoalData GetPredecessorGoalData(Guid questionId, Guid goalId)
    {
        if (!_goalsByQuestion.TryGetValue(questionId, out var goals))
            throw new InvalidOperationException($"Question {questionId} has no goals");

        var goal = goals.FirstOrDefault(g => g.Id == goalId);
        if (goal == null)
            throw new InvalidOperationException($"Goal {goalId} not found in question {questionId}");

        return new PredecessorGoalData(
            goal.ObjectiveDescription,
            goal.TimeframeFrom,
            goal.TimeframeTo,
            goal.MeasurementMetric,
            goal.AddedByRole,
            goal.WeightingPercentage);
    }

    private bool IsInProgressState()
    {
        return WorkflowState is
            WorkflowState.EmployeeInProgress or
            WorkflowState.ManagerInProgress or
            WorkflowState.BothInProgress;
    }

    private void ValidateWeightingTotal(Guid questionId, ApplicationRole role, decimal newWeighting)
    {
        if (!_goalsByQuestion.TryGetValue(questionId, out var goals))
            goals = new List<Goal>();

        var currentTotal = goals.Where(g => g.AddedByRole == role).Sum(g => g.WeightingPercentage);
        if (currentTotal + newWeighting > 100m)
        {
            throw new InvalidOperationException($"Total weighting for {role} would exceed 100% (current: {currentTotal}%, adding: {newWeighting}%)");
        }
    }

    private void ValidateTimeframe(DateTime timeframeFrom, DateTime timeframeTo)
    {
        if (timeframeFrom >= timeframeTo)
            throw new ArgumentException("Timeframe 'From' must be before 'To'");
    }

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
        WithdrawnByEmployeeId = @event.WithdrawnByEmployeeId;
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
        ManagerFinalNotes = @event.Reason; // Store reason in notes
    }

    /// <summary>
    /// Reopens the workflow to a previous state for corrections.
    /// Requires Admin, HR, or TeamLead authorization.
    /// Note: Data-level authorization (TeamLead to their team) must be checked by caller.
    /// Note: Finalized state CANNOT be reopened - must create new assignment.
    /// Note: Email notifications will be sent by the application layer.
    /// </summary>
    public void ReopenWorkflow(
        WorkflowState targetState,
        string reopenReason,
        Guid reopenedByEmployeeId,
        string reopenedByRole)
    {
        if (IsLocked)
            throw new InvalidOperationException("Cannot reopen - questionnaire is finalized and locked permanently. Create a new assignment instead.");

        if (IsWithdrawn)
            throw new InvalidOperationException("Cannot reopen - assignment is withdrawn");

        if (string.IsNullOrWhiteSpace(reopenReason))
            throw new ArgumentException("Reopen reason is required and cannot be empty", nameof(reopenReason));

        if (reopenReason.Length < 10)
            throw new ArgumentException("Reopen reason must be at least 10 characters", nameof(reopenReason));

        var validationResult = WorkflowStateMachine.CanTransitionBackward(
            WorkflowState,
            targetState,
            reopenedByRole,
            out var failureReason);

        if (validationResult == WorkflowStateMachine.ValidationResult.Invalid)
        {
            throw new InvalidWorkflowTransitionException(
                WorkflowState,
                targetState,
                failureReason ?? "Reopen not allowed",
                isReopenAttempt: true);
        }

        RaiseEvent(new WorkflowReopened(
            WorkflowState,
            targetState,
            reopenReason,
            DateTime.UtcNow,
            reopenedByEmployeeId,
            reopenedByRole
        ));
    }

    public void Apply(WorkflowReopened @event)
    {
        WorkflowState = @event.ToState;

        // Reset submission flags based on target state
        if (@event.ToState == WorkflowState.EmployeeInProgress)
        {
            EmployeeSubmittedDate = null;
            EmployeeSubmittedByEmployeeId = null;
        }
        else if (@event.ToState == WorkflowState.ManagerInProgress)
        {
            ManagerSubmittedDate = null;
            ManagerSubmittedByEmployeeId = null;
        }
        else if (@event.ToState == WorkflowState.BothInProgress)
        {
            EmployeeSubmittedDate = null;
            EmployeeSubmittedByEmployeeId = null;
            ManagerSubmittedDate = null;
            ManagerSubmittedByEmployeeId = null;
        }
        else if (@event.ToState == WorkflowState.InReview)
        {
            // Reset review confirmation flags and dates, but preserve comments/summary for editing
            ManagerReviewFinishedDate = null;
            ManagerReviewFinishedByEmployeeId = null;
            // NOTE: ManagerReviewSummary is NOT cleared - preserve it so manager can edit
            EmployeeReviewConfirmedDate = null;
            EmployeeReviewConfirmedByEmployeeId = null;
            // NOTE: EmployeeReviewComments is NOT cleared - preserve it so it remains visible after reopening
        }
    }

    public void Apply(WorkflowStateTransitioned @event)
    {
        WorkflowState = @event.ToState;
    }

    /// <summary>
    /// Helper method to transition workflow state with validation.
    /// Raises WorkflowStateTransitioned event if transition is valid.
    /// </summary>
    private void TransitionWorkflowState(
        WorkflowState targetState,
        string reason,
        Guid? transitionedBy = null)
    {
        if (WorkflowState == targetState)
        {
            // No transition needed
            return;
        }

        var validationResult = WorkflowStateMachine.CanTransitionForward(
            WorkflowState,
            targetState,
            out var failureReason);

        if (validationResult == WorkflowStateMachine.ValidationResult.Invalid)
        {
            throw new InvalidWorkflowTransitionException(
                WorkflowState,
                targetState,
                failureReason ?? "Unknown error");
        }

        RaiseEvent(new WorkflowStateTransitioned(
            WorkflowState,
            targetState,
            reason,
            DateTime.UtcNow,
            transitionedBy
        ));
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

        var newState = WorkflowStateMachine.DetermineProgressState(
            hasEmployeeProgress,
            hasManagerProgress,
            WorkflowState);

        if (newState != WorkflowState)
        {
            TransitionWorkflowState(
                newState,
                "Section completion progress update");
        }
    }

    private void UpdateWorkflowStateFromStartedWork(ApplicationRole startedBy)
    {
        // Don't update state if already in submission, review, or finalization phases
        if (WorkflowState >= WorkflowState.EmployeeSubmitted)
        {
            return;
        }

        var hasEmployeeProgress = SectionProgressList.Any(p => p.IsEmployeeCompleted);
        var hasManagerProgress = SectionProgressList.Any(p => p.IsManagerCompleted);

        // Map ApplicationRole to CompletionRole for workflow state machine compatibility
        var completionRole = startedBy == ApplicationRole.Employee ? CompletionRole.Employee : CompletionRole.Manager;

        var newState = WorkflowStateMachine.DetermineProgressStateFromStartedWork(
            StartedDate.HasValue,
            hasEmployeeProgress,
            hasManagerProgress,
            completionRole,
            WorkflowState);

        if (newState != WorkflowState)
        {
            var roleDescription = startedBy == ApplicationRole.Employee ? "Employee" : "Manager";
            TransitionWorkflowState(
                newState,
                $"{roleDescription} started work - first response saved");
        }
    }

    private void UpdateWorkflowStateOnSubmission()
    {
        var newState = WorkflowStateMachine.DetermineSubmissionState(
            EmployeeSubmittedDate.HasValue,
            ManagerSubmittedDate.HasValue,
            RequiresManagerReview);

        if (newState != WorkflowState)
        {
            TransitionWorkflowState(
                newState,
                "Questionnaire submission",
                EmployeeSubmittedDate.HasValue ? EmployeeSubmittedByEmployeeId : ManagerSubmittedByEmployeeId);
        }
    }

    // Apply methods for goal events
    public void Apply(PredecessorQuestionnaireLinked @event)
    {
        _predecessorLinks[@event.QuestionId] = @event.PredecessorAssignmentId;
    }

    public void Apply(GoalAdded @event)
    {
        if (!_goalsByQuestion.ContainsKey(@event.QuestionId))
            _goalsByQuestion[@event.QuestionId] = new List<Goal>();

        var goal = new Goal(
            @event.GoalId,
            @event.QuestionId,
            @event.AddedByRole,
            @event.TimeframeFrom,
            @event.TimeframeTo,
            @event.ObjectiveDescription,
            @event.MeasurementMetric,
            @event.WeightingPercentage,
            @event.AddedAt,
            @event.AddedByEmployeeId);

        _goalsByQuestion[@event.QuestionId].Add(goal);
    }

    public void Apply(GoalModified @event)
    {
        var goal = _goalsByQuestion.Values.SelectMany(g => g).FirstOrDefault(g => g.Id == @event.GoalId);
        if (goal == null)
            return;

        var questionId = goal.QuestionId;
        var modifiedGoal = goal.ApplyModification(
            @event.TimeframeFrom,
            @event.TimeframeTo,
            @event.ObjectiveDescription,
            @event.MeasurementMetric,
            @event.WeightingPercentage,
            @event.ModifiedByRole,
            @event.ChangeReason,
            @event.ModifiedAt,
            @event.ModifiedByEmployeeId);

        _goalsByQuestion[questionId].Remove(goal);
        _goalsByQuestion[questionId].Add(modifiedGoal);
    }

    public void Apply(GoalDeleted @event)
    {
        var goal = _goalsByQuestion.Values.SelectMany(g => g).FirstOrDefault(g => g.Id == @event.GoalId);
        if (goal == null)
            return;

        var questionId = goal.QuestionId;
        _goalsByQuestion[questionId].Remove(goal);
    }

    public void Apply(PredecessorGoalRated @event)
    {
        if (!_goalRatingsByQuestion.ContainsKey(@event.QuestionId))
            _goalRatingsByQuestion[@event.QuestionId] = new List<GoalRating>();

        // Generate unique Id for this rating entity
        var ratingId = Guid.NewGuid();

        var rating = new GoalRating(
            ratingId,
            @event.SourceAssignmentId,
            @event.SourceGoalId,
            @event.QuestionId,
            @event.PredecessorGoal,
            @event.RatedByRole,
            @event.DegreeOfAchievement,
            @event.Justification,
            @event.RatedAt,
            @event.RatedByEmployeeId);

        _goalRatingsByQuestion[@event.QuestionId].Add(rating);
    }

    public void Apply(PredecessorGoalRatingModified @event)
    {
        var rating = _goalRatingsByQuestion.Values.SelectMany(r => r)
            .FirstOrDefault(r => r.SourceGoalId == @event.SourceGoalId && r.RatedByRole == @event.ModifiedByRole);

        if (rating == null)
            return;

        var questionId = rating.QuestionId;
        var modifiedRating = rating.ApplyModification(
            @event.DegreeOfAchievement,
            @event.Justification,
            @event.ModifiedByRole,
            @event.ChangeReason,
            @event.ModifiedAt,
            @event.ModifiedByEmployeeId);

        _goalRatingsByQuestion[questionId].Remove(rating);
        _goalRatingsByQuestion[questionId].Add(modifiedRating);
    }

    /// <summary>
    /// Validates that all predecessor goals have been rated by both Employee and Manager roles.
    /// This ensures that the review cannot proceed until all predecessor goals have complete assessments.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when any predecessor goal is missing ratings from either Employee or Manager roles.
    /// </exception>
    private void ValidatePredecessorGoalsCompletion()
    {
        // Get all questions that have predecessor links
        var questionsWithPredecessors = _predecessorLinks.Keys.ToList();

        if (!questionsWithPredecessors.Any())
        {
            // No predecessor goals to validate
            return;
        }

        var incompleteGoals = new List<string>();

        foreach (var questionId in questionsWithPredecessors)
        {
            // Get all ratings for this question
            var ratingsForQuestion = _goalRatingsByQuestion.TryGetValue(questionId, out var ratings)
                ? ratings.ToList()
                : new List<GoalRating>();

            if (!ratingsForQuestion.Any())
            {
                incompleteGoals.Add($"Question {questionId}: No predecessor goals have been rated");
                continue;
            }

            // Group ratings by SourceGoalId to check each predecessor goal
            var ratingsByGoal = ratingsForQuestion.GroupBy(r => r.SourceGoalId).ToList();

            foreach (var goalGroup in ratingsByGoal)
            {
                var sourceGoalId = goalGroup.Key;
                var ratingsForGoal = goalGroup.ToList();

                // Check if we have ratings from both Employee and Manager roles
                var hasEmployeeRating = ratingsForGoal.Any(r => r.RatedByRole == ApplicationRole.Employee);
                var hasManagerRating = ratingsForGoal.Any(r =>
                    r.RatedByRole == ApplicationRole.TeamLead ||
                    r.RatedByRole == ApplicationRole.HR ||
                    r.RatedByRole == ApplicationRole.HRLead ||
                    r.RatedByRole == ApplicationRole.Admin);

                if (!hasEmployeeRating)
                {
                    incompleteGoals.Add($"Goal {sourceGoalId}: Missing Employee assessment");
                }

                if (!hasManagerRating)
                {
                    incompleteGoals.Add($"Goal {sourceGoalId}: Missing Manager assessment");
                }

                // Additional validation: Check that all ratings have valid justifications
                foreach (var rating in ratingsForGoal)
                {
                    if (string.IsNullOrWhiteSpace(rating.Justification))
                    {
                        var roleDisplayName = rating.RatedByRole == ApplicationRole.Employee ? "Employee" : "Manager";
                        incompleteGoals.Add($"Goal {sourceGoalId}: {roleDisplayName} assessment missing justification");
                    }
                }
            }
        }

        if (incompleteGoals.Any())
        {
            var errorMessage = "Cannot proceed with review - all predecessor goals must be assessed by both Employee and Manager with justifications:\n" +
                             string.Join("\n", incompleteGoals);
            throw new InvalidOperationException(errorMessage);
        }
    }
}