# Questionnaire Review Workflow Documentation

## Overview

This document describes the complete questionnaire review workflow implemented in the BeachBreak system. The workflow supports a collaborative performance review process where both employees and managers contribute to questionnaire completion, followed by a structured review and finalization process.

## Architecture Decision

This implementation follows **Option 2: Explicit Submission States** from the architecture analysis, which provides:
- Clear separation between "Submit" and "Confirm" actions
- Multiple read-only phases for data integrity
- Manager-led review meeting with special editing privileges
- Explicit state tracking for both employee and manager submissions

## Workflow States

### 1. Initial Assignment States

| State | Description | Who Can Edit | Who Can Submit |
|-------|-------------|--------------|----------------|
| `Assigned` | Initial state when questionnaire is assigned | Employee, Manager | None |
| `EmployeeInProgress` | Employee has started but not submitted | Employee | Employee |
| `ManagerInProgress` | Manager has started but not submitted | Manager | Manager |
| `BothInProgress` | Both employee and manager have started | Both | Both |

### 2. Submission States (Phase 1 Read-Only)

| State | Description | Who Can Edit | Who Can Submit |
|-------|-------------|--------------|----------------|
| `EmployeeSubmitted` | Employee submitted, manager still working | Manager | Manager |
| `ManagerSubmitted` | Manager submitted, employee still working | Employee | Employee |
| `BothSubmitted` | Both parties have submitted | **None** | None |

**Key Characteristics:**
- First read-only phase begins when both parties submit
- No editing allowed in `BothSubmitted` state
- Ensures data integrity before review meeting

### 3. Review Meeting State

| State | Description | Who Can Edit | Special Permissions |
|-------|-------------|--------------|---------------------|
| `InReview` | Manager-led review meeting in progress | **Manager only** | Manager can edit any answer during review |

**Key Characteristics:**
- Only managers can initiate review (requires `BothSubmitted` state)
- Employee is **read-only** during review meeting
- Manager has special editing privileges via `EditAnswerDuringReview` API
- Review changes are tracked in `ReviewChange` audit log

### 4. Post-Review Confirmation States

| State | Description | Who Can Edit | Next Action Required |
|-------|-------------|--------------|---------------------|
| `ManagerReviewConfirmed` | Manager finished review meeting | **None** | Employee must confirm |
| `EmployeeReviewConfirmed` | Employee confirmed review outcome | **None** | Manager must finalize |

**Key Characteristics:**
- Both states are read-only
- Sequential confirmation: Manager finishes → Employee confirms → Manager finalizes
- Ensures both parties acknowledge review outcomes

### 5. Final State

| State | Description | Who Can Edit | Notes |
|-------|-------------|--------------|-------|
| `Finalized` | Questionnaire locked and completed | **None** | Permanent read-only state |

**Key Characteristics:**
- Terminal state - no further changes allowed
- Archive and reporting can safely use finalized data

## Complete Workflow Phases

### Phase 1: Collaborative Completion

```
Assigned
   ↓ (employee starts editing)
EmployeeInProgress
   ↓ (manager starts editing)
BothInProgress
   ↓ (employee submits)
ManagerSubmitted (employee submitted, manager still working)
   ↓ (manager submits)
BothSubmitted ← FIRST READ-ONLY PHASE
```

**Alternative paths:**
- Manager submits first: `BothInProgress` → `EmployeeSubmitted` → `BothSubmitted`
- Only employee works: `EmployeeInProgress` → `EmployeeSubmitted` → (manager starts) → `BothInProgress` → etc.
- Only manager works: `ManagerInProgress` → `ManagerSubmitted` → (employee starts) → `BothInProgress` → etc.

### Phase 2: Review Meeting

```
BothSubmitted
   ↓ (manager initiates review)
InReview ← MANAGER CAN EDIT
   ↓ (manager finishes review meeting)
ManagerReviewConfirmed ← SECOND READ-ONLY PHASE
```

**During Review Meeting:**
- Manager views both employee and manager responses
- Manager can edit any answer (tracked as `ReviewChange`)
- Employee views responses in read-only mode
- Review summary can be added by manager

### Phase 3: Final Confirmation

```
ManagerReviewConfirmed
   ↓ (employee confirms review outcome)
EmployeeReviewConfirmed ← THIRD READ-ONLY PHASE
   ↓ (manager finalizes)
Finalized ← PERMANENT READ-ONLY
```

**Confirmation Steps:**
1. **Manager finishes review meeting** - adds optional review summary
2. **Employee confirms** - acknowledges review discussion, adds optional comments
3. **Manager finalizes** - locks questionnaire, adds optional final notes

## State Transition Rules

### Employee Actions

| Current State | Action | Next State | API Endpoint |
|---------------|--------|------------|--------------|
| Assigned, EmployeeInProgress, BothInProgress, ManagerSubmitted | Submit Employee Questionnaire | EmployeeSubmitted or BothSubmitted | `POST /c/api/v1/assignments/{id}/submit-employee` |
| ManagerReviewConfirmed | Confirm Review Outcome | EmployeeReviewConfirmed | `POST /c/api/v1/assignments/{id}/review/confirm-employee` |

**Business Rules:**
- Cannot submit if already submitted
- Cannot edit during `InReview` (manager-led review meeting)
- Must wait for manager to finish review before confirming

### Manager Actions

| Current State | Action | Next State | API Endpoint |
|---------------|--------|------------|--------------|
| Assigned, ManagerInProgress, BothInProgress, EmployeeSubmitted | Submit Manager Questionnaire | ManagerSubmitted or BothSubmitted | `POST /c/api/v1/assignments/{id}/submit-manager` |
| BothSubmitted | Initiate Review Meeting | InReview | `POST /c/api/v1/assignments/{id}/initiate-review` |
| InReview | Edit Answer During Review | InReview | `POST /c/api/v1/assignments/{id}/edit-answer` |
| InReview | Finish Review Meeting | ManagerReviewConfirmed | `POST /c/api/v1/assignments/{id}/review/finish` |
| EmployeeReviewConfirmed | Finalize Questionnaire | Finalized | `POST /c/api/v1/assignments/{id}/review/finalize-manager` |

**Business Rules:**
- Cannot initiate review unless both parties submitted
- Can only edit during `InReview` state
- Must wait for employee confirmation before finalizing

## API Endpoints Reference

### Command API (C API)

All endpoints require authentication. Manager endpoints require `TeamLead` role.

#### Submission Endpoints

```
POST /c/api/v1/assignments/{assignmentId}/submit-employee
Body: { "SubmittedBy": "string" }
Authorization: Employee or Manager (for the assigned employee)
Returns: 200 OK on success
```

```
POST /c/api/v1/assignments/{assignmentId}/submit-manager
Body: { "SubmittedBy": "string" }
Authorization: Manager (TeamLead role)
Returns: 200 OK on success
```

#### Review Meeting Endpoints

```
POST /c/api/v1/assignments/{assignmentId}/initiate-review
Body: { "InitiatedBy": "string" }
Authorization: Manager (TeamLead role)
Returns: 200 OK on success
Requires: WorkflowState = BothSubmitted
```

```
POST /c/api/v1/assignments/{assignmentId}/edit-answer
Body: {
  "SectionId": "guid",
  "QuestionId": "guid",
  "OriginalCompletionRole": "Employee|Manager",
  "Answer": "string",
  "EditedBy": "string"
}
Authorization: Manager (TeamLead role)
Returns: 200 OK on success
Requires: WorkflowState = InReview
Creates: ReviewChange audit record
```

```
POST /c/api/v1/assignments/{assignmentId}/review/finish
Body: {
  "FinishedBy": "string",
  "ReviewSummary": "string?" (optional)
}
Authorization: Manager (TeamLead role)
Returns: 200 OK on success
Requires: WorkflowState = InReview
```

#### Confirmation Endpoints

```
POST /c/api/v1/assignments/{assignmentId}/review/confirm-employee
Body: {
  "ConfirmedBy": "string",
  "EmployeeComments": "string?" (optional)
}
Authorization: Employee
Returns: 200 OK on success
Requires: WorkflowState = ManagerReviewConfirmed
```

```
POST /c/api/v1/assignments/{assignmentId}/review/finalize-manager
Body: {
  "FinalizedBy": "string",
  "ManagerFinalNotes": "string?" (optional)
}
Authorization: Manager (TeamLead role)
Returns: 200 OK on success
Requires: WorkflowState = EmployeeReviewConfirmed
```

### Query API (Q API)

```
GET /q/api/v1/assignments/{assignmentId}
Returns: QuestionnaireAssignment with current WorkflowState

GET /q/api/v1/assignments/{assignmentId}/review-changes
Returns: List<ReviewChangeDto> - audit log of changes made during review
```

## Domain Events

All workflow transitions emit domain events for event sourcing:

### Submission Events

```csharp
public record EmployeeQuestionnaireSubmitted(
    DateTime SubmittedAt,
    string SubmittedBy
) : IDomainEvent;

public record ManagerQuestionnaireSubmitted(
    DateTime SubmittedAt,
    string SubmittedBy
) : IDomainEvent;
```

### Review Meeting Events

```csharp
public record ReviewInitiated(
    Guid AssignmentId,
    DateTime InitiatedAt,
    string InitiatedBy
) : IDomainEvent;

public record AnswerEditedDuringReview(
    Guid AssignmentId,
    Guid SectionId,
    Guid QuestionId,
    string OriginalCompletionRole,
    string PreviousAnswer,
    string NewAnswer,
    DateTime EditedAt,
    string EditedBy
) : IDomainEvent;

public record ManagerReviewMeetingFinished(
    Guid AssignmentId,
    DateTime FinishedAt,
    string FinishedBy,
    string? ReviewSummary
) : IDomainEvent;
```

### Confirmation Events

```csharp
public record EmployeeConfirmedReviewOutcome(
    Guid AssignmentId,
    DateTime ConfirmedAt,
    string ConfirmedBy,
    string? EmployeeComments
) : IDomainEvent;

public record ManagerFinalizedQuestionnaire(
    Guid AssignmentId,
    DateTime FinalizedAt,
    string FinalizedBy,
    string? ManagerFinalNotes
) : IDomainEvent;
```

## UI Components

### WorkflowStateHelper.cs
Location: `05_Frontend/ti8m.BeachBreak.Client/Models/WorkflowStateHelper.cs`

Provides helper methods to determine allowed actions based on workflow state:

```csharp
// Employee permissions
WorkflowStateHelper.CanEmployeeEdit(assignment)       // Can edit responses
WorkflowStateHelper.CanEmployeeSubmit(assignment)     // Can submit questionnaire
WorkflowStateHelper.CanEmployeeConfirmReview(assignment) // Can confirm review

// Manager permissions
WorkflowStateHelper.CanManagerEdit(assignment)        // Can edit responses
WorkflowStateHelper.CanManagerEditDuringReview(assignment) // Special review edit
WorkflowStateHelper.CanManagerSubmit(assignment)      // Can submit questionnaire
WorkflowStateHelper.CanInitiateReview(assignment)     // Can start review meeting
WorkflowStateHelper.CanManagerFinishReviewMeeting(assignment) // Can finish review
WorkflowStateHelper.CanManagerFinalize(assignment)    // Can finalize questionnaire

// State messages
WorkflowStateHelper.GetEmployeeActionMessage(assignment) // Display message
WorkflowStateHelper.GetManagerActionMessage(assignment)  // Display message
```

### WorkflowActionButtons.razor
Location: `05_Frontend/ti8m.BeachBreak.Client/Components/Shared/WorkflowActionButtons.razor`

Displays context-appropriate action buttons based on workflow state and user role:

**Employee Buttons:**
- "Submit Questionnaire" - when `CanEmployeeSubmit()`
- "Confirm Review" - when `CanEmployeeConfirmReview()`

**Manager Buttons:**
- "Submit Questionnaire" - when `CanManagerSubmit()`
- "Initiate Review Meeting" - when `CanInitiateReview()`
- "Finish Review Meeting" - when `CanManagerFinishReviewMeeting()`
- "Finalize Questionnaire" - when `CanManagerFinalize()`

### DynamicQuestionnaire.razor
Location: `05_Frontend/ti8m.BeachBreak.Client/Pages/DynamicQuestionnaire.razor`

Main questionnaire page that respects workflow state:

```csharp
private bool IsQuestionnaireCompleted()
{
    if (isEmployee && !isManager)
    {
        // Employee is read-only during: EmployeeSubmitted, BothSubmitted, InReview,
        // EmployeeReviewConfirmed, ManagerReviewConfirmed, Finalized
        return assignment.WorkflowState is WorkflowState.EmployeeSubmitted
            or WorkflowState.BothSubmitted
            or WorkflowState.InReview  // CRITICAL: Employee read-only during review
            || isPostReviewConfirmation
            || isPhase2ReadOnly;
    }
    // Manager logic...
}
```

### QuestionnaireCompletion.razor
Location: `05_Frontend/ti8m.BeachBreak.Client/Components/Shared/QuestionnaireCompletion.razor`

Alternative questionnaire completion component with workflow-aware submission:

```csharp
private async Task SubmitQuestionnaire()
{
    // Role-based submission
    if (isEmployee && WorkflowStateHelper.CanEmployeeSubmit(Assignment))
    {
        await AssignmentService.SubmitEmployeeQuestionnaireAsync(Assignment.Id, userName);
    }
    else if (isManager && WorkflowStateHelper.CanManagerSubmit(Assignment))
    {
        await AssignmentService.SubmitManagerQuestionnaireAsync(Assignment.Id, userName);
    }
}
```

All input fields respect `canEdit` flag determined by `WorkflowStateHelper`.

## Common Scenarios

### Scenario 1: Employee First Completion

1. **Employee fills out questionnaire**
   - State: `Assigned` → `EmployeeInProgress`
   - Employee edits responses

2. **Employee submits**
   - State: `EmployeeInProgress` → `EmployeeSubmitted`
   - Employee now read-only
   - Manager can still edit

3. **Manager fills out questionnaire**
   - State: `EmployeeSubmitted` → `BothInProgress`
   - Manager edits responses

4. **Manager submits**
   - State: `BothInProgress` → `BothSubmitted`
   - **Both parties now read-only**

5. **Manager initiates review meeting**
   - State: `BothSubmitted` → `InReview`
   - Manager can edit, employee read-only

6. **Manager makes changes during review**
   - API: `POST /edit-answer` for each change
   - Each change creates `ReviewChange` audit record
   - State remains: `InReview`

7. **Manager finishes review meeting**
   - State: `InReview` → `ManagerReviewConfirmed`
   - Both parties read-only
   - Optional review summary added

8. **Employee confirms review**
   - State: `ManagerReviewConfirmed` → `EmployeeReviewConfirmed`
   - Optional employee comments added

9. **Manager finalizes**
   - State: `EmployeeReviewConfirmed` → `Finalized`
   - Permanent read-only
   - Optional final notes added

### Scenario 2: Simultaneous Work

1. **Both start working simultaneously**
   - State: `Assigned` → `BothInProgress`

2. **Manager submits first**
   - State: `BothInProgress` → `EmployeeSubmitted`
   - Manager read-only, employee can still edit

3. **Employee submits**
   - State: `EmployeeSubmitted` → `BothSubmitted`
   - Proceeds to review as in Scenario 1

### Scenario 3: Review with No Changes

1. **After both submit** (`BothSubmitted`)
2. **Manager initiates review** (`InReview`)
3. **Manager discusses with employee but makes no edits**
   - No `edit-answer` API calls
   - No `ReviewChange` records created
4. **Manager finishes review** (`ManagerReviewConfirmed`)
5. **Employee confirms** (`EmployeeReviewConfirmed`)
6. **Manager finalizes** (`Finalized`)

## Data Integrity Features

### Read-Only Phases

The workflow includes multiple read-only phases to ensure data integrity:

1. **Phase 1: BothSubmitted**
   - Both parties have submitted
   - Prevents changes before formal review

2. **Phase 2: InReview (Employee Perspective)**
   - Employee views responses read-only during manager-led review
   - Manager has controlled editing via audit trail

3. **Phase 3: Post-Review Confirmation**
   - ManagerReviewConfirmed and EmployeeReviewConfirmed
   - Both parties acknowledge final state before locking

4. **Phase 4: Finalized**
   - Permanent read-only state
   - Safe for archiving and reporting

### Audit Trail

All changes during review are tracked:

```csharp
public class ReviewChangeDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid SectionId { get; set; }
    public Guid QuestionId { get; set; }
    public string OriginalCompletionRole { get; set; } // Who originally answered
    public string PreviousAnswer { get; set; }
    public string NewAnswer { get; set; }
    public DateTime EditedAt { get; set; }
    public string EditedBy { get; set; }
}
```

Query review changes:
```
GET /q/api/v1/assignments/{assignmentId}/review-changes
```

## Technical Implementation Details

### Aggregate Pattern

The `QuestionnaireAssignment` aggregate enforces all business rules:

```csharp
public class QuestionnaireAssignment : AggregateRoot
{
    public void SubmitEmployeeQuestionnaire(string submittedBy)
    {
        if (WorkflowState == WorkflowState.EmployeeSubmitted ||
            WorkflowState == WorkflowState.BothSubmitted)
            throw new InvalidOperationException("Employee already submitted");

        RaiseEvent(new EmployeeQuestionnaireSubmitted(DateTime.UtcNow, submittedBy));
    }

    public void InitiateReview(string initiatedBy)
    {
        if (WorkflowState != WorkflowState.BothSubmitted)
            throw new InvalidOperationException("Both parties must submit before review");

        RaiseEvent(new ReviewInitiated(Id, DateTime.UtcNow, initiatedBy));
    }

    // Apply methods update state from events...
    private void Apply(EmployeeQuestionnaireSubmitted @event)
    {
        WorkflowState = WorkflowState switch
        {
            WorkflowState.EmployeeInProgress => WorkflowState.EmployeeSubmitted,
            WorkflowState.BothInProgress => WorkflowState.BothSubmitted,
            WorkflowState.ManagerSubmitted => WorkflowState.BothSubmitted,
            _ => WorkflowState
        };
    }
}
```

### Event Sourcing

All state changes are captured as domain events and stored in the event store (Marten/PostgreSQL). This provides:
- Complete audit trail of all workflow transitions
- Ability to reconstruct state at any point in time
- Business intelligence and analytics capabilities

### CQRS Separation

- **Command API** handles state changes and enforces business rules
- **Query API** provides read-optimized views of current state
- Separate deployment and scaling of command vs query sides

## Error Handling

### Common Validation Errors

| Error | HTTP Status | Cause | Solution |
|-------|-------------|-------|----------|
| "Employee already submitted" | 400 | Attempting to submit twice | Check WorkflowState before submission |
| "Both parties must submit before review" | 400 | Initiating review from wrong state | Wait for BothSubmitted state |
| "No active review meeting" | 400 | Finishing review when not InReview | Initiate review first |
| "Manager must finish review before confirmation" | 400 | Employee confirming before manager | Wait for ManagerReviewConfirmed |
| "Employee must confirm before finalization" | 400 | Manager finalizing before employee confirms | Wait for EmployeeReviewConfirmed |

### Authorization Errors

| Error | HTTP Status | Cause | Solution |
|-------|-------------|-------|----------|
| 401 Unauthorized | 401 | Not authenticated | Provide valid authentication token |
| 403 Forbidden | 403 | Insufficient permissions | Ensure user has TeamLead role for manager actions |

## Best Practices

### For Developers

1. **Always check WorkflowState** before performing actions
2. **Use WorkflowStateHelper** methods in UI components
3. **Handle InvalidOperationException** from aggregate methods
4. **Never bypass the aggregate** for state changes
5. **Query review changes** to show audit trail in UI
6. **Add meaningful ReviewSummary and Comments** for better documentation

### For Users (Managers)

1. **Review employee responses** before submitting your own
2. **Use review meeting** for collaborative discussion
3. **Add review summary** when finishing review meeting
4. **Track changes** made during review for transparency
5. **Add final notes** when finalizing for documentation

### For Users (Employees)

1. **Submit when ready** - you cannot edit after submission
2. **Review is read-only** - discuss changes with manager during meeting
3. **Add comments** when confirming review if you have concerns
4. **Check review changes** to see what was modified

## Migration from Old System

If migrating from a previous workflow implementation:

1. **Old "Submit" behavior** → Now explicitly "Submit Employee/Manager Questionnaire"
2. **Old "Confirm" behavior** → Now separated into:
   - Initiate Review Meeting
   - Finish Review Meeting
   - Confirm Review Outcome (Employee)
   - Finalize Questionnaire (Manager)
3. **Review changes** → Now tracked in ReviewChange audit log
4. **Read-only enforcement** → Now has multiple phases with clear rules

## Future Enhancements

Potential future additions to the workflow:

1. **Rejection/Revision Flow** - Allow employee or manager to request revisions
2. **Deadline Notifications** - Alert users when submission deadlines approach
3. **Bulk Review** - Manager reviews multiple assignments simultaneously
4. **Template Versioning** - Track which template version was used
5. **Anonymous Feedback** - Option for anonymous employee feedback
6. **Review Meeting Scheduling** - Calendar integration for scheduling review meetings
7. **Signature Capture** - Digital signatures for final confirmation
8. **PDF Export** - Generate PDF of finalized questionnaire

## Conclusion

This workflow provides a structured, auditable process for collaborative performance reviews with:
- Clear role-based permissions
- Multiple read-only phases for data integrity
- Manager-led review meeting with audit trail
- Sequential confirmation process
- Event-sourced history for compliance and analytics

For questions or issues, refer to the codebase at `C:\projects\BlazorRadzenTest` or consult the architecture documents.
