# Questionnaire Assignment Workflow States and Transitions

## Table of Contents
1. [Overview](#overview)
2. [Workflow States](#workflow-states)
3. [State Transition Rules](#state-transition-rules)
4. [Authorization Model](#authorization-model)
5. [Auto-Initialization Feature](#auto-initialization-feature)
6. [Workflow Phases](#workflow-phases)
7. [State Transition Diagrams](#state-transition-diagrams)
8. [Implementation Details](#implementation-details)

---

## Overview

The BeachBreak questionnaire system uses a sophisticated state machine to manage the lifecycle of questionnaire assignments from creation through finalization. The workflow supports both simple (employee-only) and complex (employee + manager) questionnaire types with optional review phases.

**Key Principles:**
- **Event-Sourced**: All state changes are tracked as domain events
- **Validation-First**: State transitions are validated before execution
- **Role-Based**: Different roles have different permissions at each state
- **Unidirectional Flow**: Generally moves forward, with limited backward transitions for corrections

---

## Workflow States

### State Enum Definition

```csharp
public enum WorkflowState
{
    Assigned = 0,           // Initial state - manager preparation phase
    Initialized = 1,        // Manager completed initialization
    EmployeeInProgress = 2, // Employee filling questionnaire
    ManagerInProgress = 3,  // Manager filling questionnaire
    BothInProgress = 4,     // Both employee and manager filling

    // Submission Phase (Read-Only)
    EmployeeSubmitted = 5,  // Employee completed and submitted
    ManagerSubmitted = 6,   // Manager completed and submitted
    BothSubmitted = 7,      // Both submitted, awaiting review

    // Review Phase
    InReview = 8,           // Manager-led review meeting

    // Post-Review Confirmation
    ReviewFinished = 9,         // Manager finished review, awaiting employee confirmation
    EmployeeReviewConfirmed = 10, // Employee confirmed review outcome

    // Final State (Read-Only)
    Finalized = 11          // Terminal state - questionnaire archived
}
```

### State Descriptions

#### 1. **Assigned** (Value: 0)
- **Purpose**: Manager-only preparation phase
- **Duration**: From creation until manager completes initialization
- **Manager Actions**:
  - Link predecessor questionnaire for goal tracking
  - Add custom sections (if `IsCustomizable=true`)
  - Add initialization notes for employee
- **Employee Visibility**: ❌ **NOT visible to employees**
- **Can Edit**: ✅ Manager only
- **Next States**: `Initialized`
- **Added**: 2026-01-06 (Initialized Workflow State feature)

#### 2. **Initialized** (Value: 1)
- **Purpose**: Manager completed initialization, both parties can begin work
- **Duration**: From initialization until first party starts filling
- **Manager Actions**: Can start filling their questionnaire
- **Employee Actions**: Can start filling their questionnaire
- **Employee Visibility**: ✅ **Visible to employees**
- **Can Edit**: ✅ Both employee and manager
- **Next States**: `EmployeeInProgress`, `ManagerInProgress`, `BothInProgress`
- **Key Feature**: This is the first state where employees can see and access the assignment

#### 3. **EmployeeInProgress** (Value: 2)
- **Purpose**: Employee actively filling questionnaire, manager not started
- **Manager Actions**: Can start filling (transitions to `BothInProgress`)
- **Employee Actions**: Can continue filling, can submit when complete
- **Can Edit**: ✅ Employee (✅ Manager can also start)
- **Next States**: `BothInProgress`, `EmployeeSubmitted`

#### 4. **ManagerInProgress** (Value: 3)
- **Purpose**: Manager actively filling questionnaire, employee not started
- **Manager Actions**: Can continue filling, can submit when complete
- **Employee Actions**: Can start filling (transitions to `BothInProgress`)
- **Can Edit**: ✅ Manager (✅ Employee can also start)
- **Next States**: `BothInProgress`, `ManagerSubmitted`

#### 5. **BothInProgress** (Value: 4)
- **Purpose**: Both employee and manager actively filling questionnaires
- **Manager Actions**: Can continue filling, can submit when complete
- **Employee Actions**: Can continue filling, can submit when complete
- **Can Edit**: ✅ Both
- **Next States**: `EmployeeSubmitted`, `ManagerSubmitted`

#### 6. **EmployeeSubmitted** (Value: 5)
- **Purpose**: Employee completed and submitted, awaiting manager
- **Manager Actions**: Can continue filling, can submit when complete
- **Employee Actions**: None (read-only)
- **Can Edit**: ✅ Manager only (employee's questionnaire is locked)
- **Next States**: `BothSubmitted`, `Finalized` (if no manager review required)
- **Special Case**: If `RequiresManagerReview=false`, auto-transitions to `Finalized`

#### 7. **ManagerSubmitted** (Value: 6)
- **Purpose**: Manager completed and submitted, awaiting employee
- **Manager Actions**: None (read-only)
- **Employee Actions**: Can continue filling, can submit when complete
- **Can Edit**: ✅ Employee only (manager's questionnaire is locked)
- **Next States**: `BothSubmitted`

#### 8. **BothSubmitted** (Value: 7)
- **Purpose**: Both parties submitted, ready for review meeting
- **Manager Actions**: Can initiate review meeting
- **Employee Actions**: None (read-only)
- **Can Edit**: ❌ Both questionnaires locked
- **Next States**: `InReview`
- **Validation**: Both questionnaires must be 100% complete

#### 9. **InReview** (Value: 8)
- **Purpose**: Manager-led review meeting in progress
- **Duration**: From review initiation until manager finishes review
- **Manager Actions**:
  - Can edit answers (with audit trail)
  - Can add/edit/delete discussion notes
  - Can finish review with summary
- **Employee Actions**:
  - Read-only view of questionnaires
  - Can add/edit/delete discussion notes
- **Can Edit**: ✅ Manager can edit answers, ✅ Both can manage notes
- **Next States**: `ReviewFinished`
- **Special Feature**: Only state where submitted answers can be modified

#### 10. **ReviewFinished** (Value: 9)
- **Purpose**: Manager completed review, awaiting employee confirmation
- **Manager Actions**: None (waiting for employee)
- **Employee Actions**: Can confirm review outcome with optional comments
- **Can Edit**: ❌ Both questionnaires locked
- **Next States**: `EmployeeReviewConfirmed`

#### 11. **EmployeeReviewConfirmed** (Value: 10)
- **Purpose**: Employee confirmed review outcome, awaiting manager finalization
- **Manager Actions**: Can finalize with optional final notes
- **Employee Actions**: None (read-only)
- **Can Edit**: ❌ Both questionnaires locked
- **Next States**: `Finalized`

#### 12. **Finalized** (Value: 11)
- **Purpose**: Terminal state - questionnaire lifecycle complete
- **Manager Actions**: None (read-only)
- **Employee Actions**: None (read-only)
- **Can Edit**: ❌ Completely locked (archived)
- **Next States**: None (terminal state)
- **Special Feature**: **CANNOT be reopened** - must create new assignment for changes

---

## State Transition Rules

### Forward Transitions (Normal Flow)

```
Assigned → Initialized
    ├─ Trigger: Manager calls InitializeAssignment
    ├─ Required: Manager role (TeamLead/HR/HRLead/Admin)
    └─ Validation: Must be in Assigned state

Initialized → EmployeeInProgress | ManagerInProgress | BothInProgress
    ├─ Trigger: Employee or manager starts work (saves first response)
    ├─ Logic:
    │   └─ If employee starts → EmployeeInProgress
    │   └─ If manager starts → ManagerInProgress
    │   └─ If both start simultaneously → BothInProgress

EmployeeInProgress → BothInProgress
    ├─ Trigger: Manager starts work
    └─ Validation: Assignment not withdrawn

EmployeeInProgress → EmployeeSubmitted
    ├─ Trigger: Employee submits questionnaire
    └─ Validation: Employee questionnaire 100% complete

ManagerInProgress → BothInProgress
    ├─ Trigger: Employee starts work
    └─ Validation: Assignment not withdrawn

ManagerInProgress → ManagerSubmitted
    ├─ Trigger: Manager submits questionnaire
    └─ Validation: Manager questionnaire 100% complete

BothInProgress → EmployeeSubmitted
    ├─ Trigger: Employee submits first
    └─ Validation: Employee questionnaire 100% complete

BothInProgress → ManagerSubmitted
    ├─ Trigger: Manager submits first
    └─ Validation: Manager questionnaire 100% complete

EmployeeSubmitted → BothSubmitted
    ├─ Trigger: Manager submits
    └─ Validation: Manager questionnaire 100% complete

EmployeeSubmitted → Finalized (Auto-transition)
    ├─ Trigger: Employee submits AND RequiresManagerReview=false
    └─ Special: Skips entire review phase for simple questionnaires

ManagerSubmitted → BothSubmitted
    ├─ Trigger: Employee submits
    └─ Validation: Employee questionnaire 100% complete

BothSubmitted → InReview
    ├─ Trigger: Manager initiates review
    ├─ Required: Manager role
    └─ Validation: Both questionnaires 100% complete

InReview → ReviewFinished
    ├─ Trigger: Manager finishes review with summary
    ├─ Required: Manager role
    └─ Validation: Review summary provided

ReviewFinished → EmployeeReviewConfirmed
    ├─ Trigger: Employee confirms review outcome
    ├─ Required: Employee role
    └─ Validation: Must be assigned employee

EmployeeReviewConfirmed → Finalized
    ├─ Trigger: Manager finalizes with optional notes
    ├─ Required: Manager role
    └─ Validation: Employee has confirmed
```

### Backward Transitions (Reopening)

Backward transitions allow authorized users to reopen assignments for corrections. **Finalized state CANNOT be reopened.**

```
Initialized → Assigned
    ├─ Trigger: Admin/HR/TeamLead reopens to reset initialization
    ├─ Reason: "Reset initialization"
    └─ Allowed Roles: Admin, HR, TeamLead

EmployeeSubmitted → EmployeeInProgress
    ├─ Trigger: Admin/HR/TeamLead reopens for corrections
    ├─ Reason: "Employee questionnaire corrections"
    └─ Allowed Roles: Admin, HR, TeamLead

ManagerSubmitted → ManagerInProgress
    ├─ Trigger: Admin/HR/TeamLead reopens for corrections
    ├─ Reason: "Manager questionnaire corrections"
    └─ Allowed Roles: Admin, HR, TeamLead

BothSubmitted → BothInProgress
    ├─ Trigger: Admin/HR/TeamLead reopens for corrections
    ├─ Reason: "Both questionnaires corrections"
    └─ Allowed Roles: Admin, HR, TeamLead

ReviewFinished → InReview
    ├─ Trigger: Admin/HR/TeamLead reopens review meeting
    ├─ Reason: "Review meeting manager revisions"
    └─ Allowed Roles: Admin, HR, TeamLead

EmployeeReviewConfirmed → ReviewFinished
    ├─ Trigger: Admin/HR/TeamLead returns to employee signoff
    ├─ Reason: "Return to employee signoff"
    └─ Allowed Roles: Admin, HR, TeamLead

EmployeeReviewConfirmed → InReview
    ├─ Trigger: Admin/HR/TeamLead reopens review after confirmation
    ├─ Reason: "Review meeting after confirmation"
    └─ Allowed Roles: Admin, HR, TeamLead
```

**Important Rules:**
- Only specific roles can reopen assignments
- TeamLead can only reopen assignments for their own team members
- Reopening creates audit trail with reason
- Finalized state is **terminal** - no reopening allowed

---

## Authorization Model

### Role Hierarchy

```
Employee (0) < TeamLead (1) < HR (2) < HRLead (3) < Admin (4)
```

### Permissions by State

| State | Employee View | Employee Edit | Manager View | Manager Edit | Can Transition |
|-------|--------------|---------------|--------------|--------------|----------------|
| **Assigned** | ❌ No | ❌ No | ✅ Yes | ✅ Yes | Manager (init) |
| **Initialized** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | Both (start) |
| **EmployeeInProgress** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | Both |
| **ManagerInProgress** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | Both |
| **BothInProgress** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | Both |
| **EmployeeSubmitted** | ✅ Yes | ❌ No | ✅ Yes | ✅ Yes | Manager |
| **ManagerSubmitted** | ✅ Yes | ✅ Yes | ✅ Yes | ❌ No | Employee |
| **BothSubmitted** | ✅ Yes | ❌ No | ✅ Yes | ❌ No | Manager (review) |
| **InReview** | ✅ Yes | ❌ No* | ✅ Yes | ✅ Yes* | Manager (finish) |
| **ReviewFinished** | ✅ Yes | ❌ No | ✅ Yes | ❌ No | Employee (confirm) |
| **EmployeeReviewConfirmed** | ✅ Yes | ❌ No | ✅ Yes | ❌ No | Manager (finalize) |
| **Finalized** | ✅ Yes | ❌ No | ✅ Yes | ❌ No | None (terminal) |

*During InReview: Both can edit discussion notes, only manager can edit answers

### Reopen Permissions

| From State | To State | Allowed Roles |
|------------|----------|---------------|
| Initialized | Assigned | Admin, HR, TeamLead |
| EmployeeSubmitted | EmployeeInProgress | Admin, HR, TeamLead |
| ManagerSubmitted | ManagerInProgress | Admin, HR, TeamLead |
| BothSubmitted | BothInProgress | Admin, HR, TeamLead |
| ReviewFinished | InReview | Admin, HR, TeamLead |
| EmployeeReviewConfirmed | ReviewFinished | Admin, HR, TeamLead |
| EmployeeReviewConfirmed | InReview | Admin, HR, TeamLead |

---

## Auto-Initialization Feature

### Overview

Added: **2026-01-13**

The `AutoInitialize` flag on questionnaire templates controls whether assignments skip the manual initialization phase.

### Template Configuration

**AutoInitialize Flag:**
- **Type**: Boolean
- **Default**: `false` (requires manual initialization)
- **Location**: QuestionnaireTemplate aggregate
- **UI Control**: Checkbox in Questionnaire Builder > Basic Info Tab
- **Can Change**: Only when template is in Draft status AND no active assignments exist

### Separation of Concerns

The system now has two independent flags:

| Flag | Purpose | Default |
|------|---------|---------|
| `IsCustomizable` | Can managers add custom sections to this template? | `false` |
| `AutoInitialize` | Should assignments skip the initialization phase? | `false` |

### Valid Combinations

| IsCustomizable | AutoInitialize | Use Case | Behavior |
|----------------|----------------|----------|----------|
| `false` | `false` | Non-customizable but needs initialization | Manager must initialize (e.g., link predecessors, add notes) |
| `false` | `true` | Simple surveys | Auto-initialize, immediate employee access |
| `true` | `false` | Full customization workflow | Manager can add custom sections, must initialize |
| `true` | `true` | Customizable with auto-init | Manager can add custom sections, but auto-initialized |

### Auto-Initialization Behavior

**When `AutoInitialize = true`:**

1. **During Assignment Creation** (`CreateBulkAssignmentsCommandHandler`):
   ```csharp
   if (template.AutoInitialize && command.AssignedByEmployeeId.HasValue)
   {
       assignment.StartInitialization(
           command.AssignedByEmployeeId.Value,
           "Auto-initialized per template configuration");
   }
   ```

2. **State Flow**:
   ```
   Created → [Auto-Initialize] → Initialized → (ready for work)
   ```

3. **Result**:
   - Assignment immediately enters `Initialized` state
   - Employees can see and access assignment immediately
   - No manager initialization step required

**When `AutoInitialize = false`:**

1. **State Flow**:
   ```
   Created → Assigned → [Manager Initializes] → Initialized → (ready for work)
   ```

2. **Manager Actions**:
   - Navigate to `/assignments/{id}/initialize`
   - Optionally link predecessor questionnaire
   - Optionally add custom sections (if `IsCustomizable=true`)
   - Optionally add initialization notes
   - Complete initialization

3. **Result**:
   - Assignment remains in `Assigned` state until manager acts
   - Employees **cannot see** assignment until initialized
   - Manager must explicitly call `InitializeAssignmentCommand`

### Historical Context

**Before 2026-01-13:**
- Auto-initialization was coupled with `IsCustomizable`
- Logic: `if (!template.IsCustomizable)` → auto-initialize
- Problem: Non-customizable templates couldn't require initialization

**After 2026-01-13:**
- Auto-initialization is explicit via `AutoInitialize` flag
- Logic: `if (template.AutoInitialize)` → auto-initialize
- Solution: Non-customizable templates can require initialization for other purposes

---

## Workflow Phases

### Phase 1: Initialization Phase
**States**: `Assigned`, `Initialized`

**Purpose**: Manager preparation before employee access

**Activities**:
- Link predecessor questionnaire for goal tracking
- Add custom Assessment or TextQuestion sections (if `IsCustomizable=true`)
- Add initialization notes for employee (max 5000 characters)

**Key Rule**: Employees **CANNOT** see assignments in `Assigned` state

### Phase 2: Working Phase
**States**: `EmployeeInProgress`, `ManagerInProgress`, `BothInProgress`

**Purpose**: Both parties fill their respective questionnaires

**Activities**:
- Employee completes competency assessments, text responses, goal setting
- Manager completes assessments, evaluations, feedback
- Both can save partial progress (draft mode)

**Completion Validation**:
- All required sections must be 100% complete
- Assessment questions: All competencies rated
- Text questions: All sections filled
- Goal questions: All goals defined

### Phase 3: Submission Phase
**States**: `EmployeeSubmitted`, `ManagerSubmitted`, `BothSubmitted`

**Purpose**: Lock questionnaires after completion

**Rules**:
- Submitted questionnaires become read-only
- Cannot revert to in-progress without admin/HR intervention
- Auto-finalize if `RequiresManagerReview=false`

### Phase 4: Review Phase
**States**: `InReview`, `ReviewFinished`, `EmployeeReviewConfirmed`

**Purpose**: Manager-led review meeting and outcome confirmation

**Activities**:
- **InReview**:
  - Manager can edit answers (with audit trail)
  - Both can add/edit/delete discussion notes
  - Manager provides review summary
- **ReviewFinished**:
  - Employee reviews outcomes
  - Employee confirms with optional comments
- **EmployeeReviewConfirmed**:
  - Manager finalizes with optional notes

**Special Feature**: Only phase where submitted answers can be modified

### Phase 5: Finalization
**State**: `Finalized`

**Purpose**: Archive completed questionnaire

**Characteristics**:
- Terminal state - no further transitions
- Completely read-only
- **Cannot be reopened** - must create new assignment
- Permanent record for compliance and history

---

## State Transition Diagrams

### Simple Workflow (RequiresManagerReview = false)

```
┌──────────┐
│ Assigned │ (Manager-only)
└────┬─────┘
     │ Manager initializes (or auto-init)
     ▼
┌─────────────┐
│ Initialized │
└──────┬──────┘
       │ Employee starts
       ▼
┌────────────────────┐
│ EmployeeInProgress │
└─────────┬──────────┘
          │ Employee submits
          ▼
┌───────────────────┐
│ EmployeeSubmitted │
└─────────┬─────────┘
          │ Auto-finalize
          ▼
┌───────────┐
│ Finalized │ (Terminal)
└───────────┘
```

### Complex Workflow (RequiresManagerReview = true)

```
┌──────────┐
│ Assigned │ (Manager-only, employees can't see)
└────┬─────┘
     │ Manager initializes (or auto-init)
     ▼
┌─────────────┐
│ Initialized │ (First state visible to employees)
└──────┬──────┘
       │
       ├──────────────┬──────────────┐
       │              │              │
       ▼              ▼              ▼
┌────────────────┐ ┌─────────────┐ ┌───────────────┐
│ Employee       │ │ Manager     │ │ Both          │
│ InProgress     │ │ InProgress  │ │ InProgress    │
└────────┬───────┘ └──────┬──────┘ └───────┬───────┘
         │                │                 │
         │ Both can transition to BothInProgress
         │                │                 │
         └────────────────┴─────────────────┘
                          │
         ┌────────────────┼────────────────┐
         │                │                │
         ▼                ▼                ▼
┌────────────────┐ ┌─────────────┐ ┌──────────────┐
│ Employee       │ │ Manager     │ │ Both         │
│ Submitted      │ │ Submitted   │ │ Submitted    │
└────────┬───────┘ └──────┬──────┘ └──────┬───────┘
         │                │                │
         └────────────────┴────────────────┘
                          │
                          ▼
                  ┌───────────────┐
                  │ BothSubmitted │
                  └───────┬───────┘
                          │ Manager initiates review
                          ▼
                  ┌────────────┐
                  │  InReview  │
                  └──────┬─────┘
                         │ Manager finishes review
                         ▼
                  ┌──────────────────┐
                  │ ReviewFinished   │
                  └────────┬─────────┘
                           │ Employee confirms
                           ▼
                  ┌────────────────────────┐
                  │ EmployeeReviewConfirmed│
                  └──────────┬─────────────┘
                             │ Manager finalizes
                             ▼
                       ┌───────────┐
                       │ Finalized │ (Terminal)
                       └───────────┘
```

### Reopen Flow (Backward Transitions)

```
                    Admin/HR/TeamLead can reopen:

Initialized ←────────────────────── Assigned
    (Reset initialization)

EmployeeSubmitted ←────────────────── EmployeeInProgress
    (Employee corrections)

ManagerSubmitted ←────────────────── ManagerInProgress
    (Manager corrections)

BothSubmitted ←────────────────── BothInProgress
    (Both corrections)

ReviewFinished ←────────────────── InReview
    (Review revisions)

EmployeeReviewConfirmed ←────────────────── ReviewFinished
                        └────────────────── InReview
    (Return for re-confirmation or review reopening)

┌───────────┐
│ Finalized │ ← CANNOT BE REOPENED (create new assignment)
└───────────┘
```

---

## Implementation Details

### Domain Layer

**File**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/WorkflowState.cs`

```csharp
public enum WorkflowState
{
    Assigned = 0,
    Initialized = 1,
    EmployeeInProgress = 2,
    ManagerInProgress = 3,
    BothInProgress = 4,
    EmployeeSubmitted = 5,
    ManagerSubmitted = 6,
    BothSubmitted = 7,
    InReview = 8,
    ReviewFinished = 9,
    EmployeeReviewConfirmed = 10,
    Finalized = 11
}
```

**Explicit Values**: All enum members have explicit integer values to prevent serialization bugs across CQRS layers.

### State Machine

**File**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/WorkflowStateMachine.cs`

**Key Methods**:
- `IsValidTransition(from, to)` - Validates if transition is allowed
- `GetValidNextStates(currentState)` - Returns possible next states
- `CanReopen(currentState)` - Checks if state can be reopened
- `DetermineProgressStateFromStartedWork()` - Auto-transition logic when work begins
- `DetermineSubmissionState()` - Auto-transition logic on submission

### Transition Configuration

**File**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/WorkflowTransitions.cs`

**Two dictionaries**:
1. `ForwardTransitions` - Normal workflow progression
2. `BackwardTransitions` - Reopening for corrections (with role authorization)

### State Machine Tests

**File**: `Tests/ti8m.BeachBreak.Domain.Tests/WorkflowStateMachineTests.cs`

**Test Coverage**:
- Valid transition tests (forward)
- Invalid transition tests (backward without permission)
- Reopen permission tests
- State count validation
- Auto-transition logic tests

### Frontend State Helper

**File**: `05_Frontend/ti8m.BeachBreak.Client/Models/WorkflowStateHelper.cs`

**Provides**:
- `GetStateDisplayName()` - Localized state names
- `GetStateColor()` - CSS color for state badge
- `GetStateIcon()` - Material icon for state
- `CanEmployeeEdit()` - Employee edit permission check
- `CanManagerEdit()` - Manager edit permission check
- `GetNextActionForEmployee()` - Action button text for employee
- `GetNextActionForManager()` - Action button text for manager

### Domain Events

**Location**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/Events/`

**State Transition Events**:
- `AssignmentInitialized` - Assigned → Initialized
- `AssignmentWorkStarted` - Tracks first save (auto-transitions to InProgress states)
- `EmployeeQuestionnaireSubmitted` - → EmployeeSubmitted
- `ManagerQuestionnaireSubmitted` - → ManagerSubmitted
- `ReviewInitiated` - BothSubmitted → InReview
- `ManagerReviewMeetingFinished` - InReview → ReviewFinished
- `EmployeeSignedOffReviewOutcome` - ReviewFinished → EmployeeReviewConfirmed
- `ManagerFinalizedQuestionnaire` - EmployeeReviewConfirmed → Finalized
- `QuestionnaireAutoFinalized` - EmployeeSubmitted → Finalized (simple workflow)
- `WorkflowReopened` - Backward transitions with reason
- `WorkflowStateTransitioned` - Generic state change tracking

---

## Translation Keys

### Workflow State Names

| Key | German | English |
|-----|--------|---------|
| `workflow-states.assigned` | Zugewiesen | Assigned |
| `workflow-states.initialized` | Initialisiert | Initialized |
| `workflow-states.employee-in-progress` | Mitarbeiter in Bearbeitung | Employee In Progress |
| `workflow-states.manager-in-progress` | Manager in Bearbeitung | Manager In Progress |
| `workflow-states.both-in-progress` | Beide in Bearbeitung | Both In Progress |
| `workflow-states.employee-submitted` | Mitarbeiter eingereicht | Employee Submitted |
| `workflow-states.manager-submitted` | Manager eingereicht | Manager Submitted |
| `workflow-states.both-submitted` | Beide eingereicht | Both Submitted |
| `workflow-states.in-review` | In Überprüfung | In Review |
| `workflow-states.review-finished` | Überprüfung abgeschlossen | Review Finished |
| `workflow-states.employee-review-confirmed` | Mitarbeiter bestätigt | Employee Review Confirmed |
| `workflow-states.finalized` | Abgeschlossen | Finalized |

### Action Keys

Located in: `TestDataGenerator/test-translations.json`

**Employee Actions**:
- `actions.employee.start-completing-sections`
- `actions.employee.continue-completing-sections`
- `actions.employee.submit-questionnaire`
- `actions.employee.waiting-manager-submission`
- `actions.employee.waiting-review-initiation`
- `actions.employee.waiting-review-meeting`
- `actions.employee.confirm-review-outcome`
- `actions.employee.waiting-manager-finalization`
- `actions.employee.waiting-manager-initialization`

**Manager Actions**:
- `actions.manager.initialize-assignment`
- `actions.manager.start-completing-sections`
- `actions.manager.continue-completing-sections`
- `actions.manager.submit-questionnaire`
- `actions.manager.waiting-employee-submission`
- `actions.manager.initiate-review`
- `actions.manager.conduct-review-meeting`
- `actions.manager.waiting-employee-confirmation`
- `actions.manager.finalize-questionnaire`

---

## Validation Rules

### State-Level Validation

1. **Assigned State**:
   - Can only transition to `Initialized`
   - Only managers can initialize
   - Custom sections can only be added in this state (before initialization)

2. **Initialization Requirement**:
   - `Assigned → EmployeeInProgress` is **INVALID**
   - Must go through `Initialized` state first
   - This ensures manager has opportunity to prepare assignment

3. **Submission Validation**:
   - Employee questionnaire must be 100% complete before submission
   - Manager questionnaire must be 100% complete before submission
   - All required sections must be filled
   - All assessment competencies must be rated

4. **Review Phase Validation**:
   - Can only enter review if both questionnaires 100% complete
   - Manager must provide review summary to finish review
   - Employee must confirm before manager can finalize

5. **Finalization Rules**:
   - Simple workflow: Auto-finalize when employee submits (if `RequiresManagerReview=false`)
   - Complex workflow: Requires full review cycle completion
   - **Cannot reopen once finalized** - terminal state

### Cross-State Validation

**During InProgress States**:
- Cannot submit while any required section is incomplete
- Cannot transition to `BothSubmitted` unless both parties submitted
- Withdrawn assignments cannot transition (except reopen by admin)

**During Review Phase**:
- Employee questionnaire is read-only during InReview
- Manager can edit answers during InReview (with audit trail)
- Both can manage discussion notes during InReview

---

## Best Practices

### For Developers

1. **Always validate state transitions** before executing commands
2. **Use domain events** for all state changes (event sourcing)
3. **Check permissions** before allowing state transitions
4. **Log all transitions** with reason/trigger for audit trail
5. **Handle auto-transitions** consistently (e.g., auto-finalize logic)
6. **Test backward transitions** with proper role authorization

### For Managers

1. **Initialize assignments promptly** so employees can begin work
2. **Use initialization notes** to provide context to employees
3. **Link predecessors** for goal-based questionnaires
4. **Review submissions carefully** before finalizing
5. **Document review meetings** with comprehensive notes
6. **Consider auto-initialization** for simple surveys (`AutoInitialize=true`)

### For HR/Admins

1. **Monitor stuck assignments** (long time in single state)
2. **Use reopen carefully** - always provide clear reason
3. **Avoid reopening finalized** - create new assignment instead
4. **Configure templates** with appropriate `AutoInitialize` setting
5. **Train managers** on initialization workflow

---

## Change Log

### 2026-01-13: Auto-Initialize Feature
- Added `AutoInitialize` boolean flag to QuestionnaireTemplate
- Separated `IsCustomizable` from auto-initialization logic
- Updated `CreateBulkAssignmentsCommandHandler` to check `AutoInitialize` instead of `!IsCustomizable`
- Allows non-customizable templates to require initialization

### 2026-01-06: Initialized Workflow State
- Added `Initialized` state (value=1) between `Assigned` and `InProgress` states
- Manager-only initialization phase with optional customizations
- Employees cannot see assignments until `Initialized` state
- Breaking change: `Assigned → EmployeeInProgress` is now invalid

### Previous Versions
- Original workflow: 11 states (without Assigned/Initialized separation)
- Auto-finalize for simple questionnaires (no manager review)
- Review phase with note discussions

---

## References

- **CLAUDE.md**: Project-level documentation and patterns
- **IMPLEMENTATION_INITIALIZED_STATE.md**: Detailed implementation guide for Initialized state
- **Todo/InitStep.md**: Implementation plan for AutoInitialize feature
- **WorkflowStateMachine.cs**: Core state transition logic
- **WorkflowTransitions.cs**: Transition configuration
- **WorkflowStateMachineTests.cs**: Comprehensive test suite

---

*Last Updated: 2026-01-13*
*Document Version: 1.0*
