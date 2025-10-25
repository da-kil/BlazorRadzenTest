# Questionnaire Workflow State Machine - Visual Documentation

## 1. Complete State Transition Diagram

This diagram shows all 13 workflow states and their valid transitions.

```mermaid
stateDiagram-v2
    [*] --> Assigned: Assignment Created

    %% Phase 0: Initial Work Phase
    Assigned --> EmployeeInProgress: Employee starts filling sections
    Assigned --> ManagerInProgress: Manager starts filling sections
    Assigned --> BothInProgress: Both start filling sections
    Assigned --> Finalized: Auto-finalize (no manager review required)

    EmployeeInProgress --> BothInProgress: Manager starts filling sections
    EmployeeInProgress --> EmployeeSubmitted: Employee submits questionnaire

    ManagerInProgress --> BothInProgress: Employee starts filling sections
    ManagerInProgress --> ManagerSubmitted: Manager submits questionnaire

    BothInProgress --> EmployeeSubmitted: Employee submits first
    BothInProgress --> ManagerSubmitted: Manager submits first

    %% Phase 1: Submission Phase (Read-Only)
    EmployeeSubmitted --> BothSubmitted: Manager submits questionnaire
    ManagerSubmitted --> BothSubmitted: Employee submits questionnaire

    %% Phase 2: Review Phase
    BothSubmitted --> InReview: Manager initiates review meeting

    InReview --> ManagerReviewConfirmed: Manager finishes review meeting

    %% Phase 3: Post-Review Confirmation
    ManagerReviewConfirmed --> EmployeeReviewConfirmed: Employee confirms review outcome

    EmployeeReviewConfirmed --> Finalized: Manager finalizes questionnaire

    Finalized --> [*]: Questionnaire Complete (Locked)

    %% Styling for different phases
    classDef workPhase fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    classDef submissionPhase fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    classDef reviewPhase fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
    classDef confirmPhase fill:#e8f5e9,stroke:#388e3c,stroke-width:2px
    classDef finalPhase fill:#e0e0e0,stroke:#424242,stroke-width:3px

    class Assigned,EmployeeInProgress,ManagerInProgress,BothInProgress workPhase
    class EmployeeSubmitted,ManagerSubmitted,BothSubmitted submissionPhase
    class InReview reviewPhase
    class ManagerReviewConfirmed,EmployeeReviewConfirmed confirmPhase
    class Finalized finalPhase
```

---

## 2. Workflow Phases Overview

```mermaid
graph TB
    subgraph "Phase 0: Work In Progress"
        A[Assigned]
        EIP[EmployeeInProgress]
        MIP[ManagerInProgress]
        BIP[BothInProgress]
    end

    subgraph "Phase 1: Submission (Read-Only)"
        ES[EmployeeSubmitted]
        MS[ManagerSubmitted]
        BS[BothSubmitted]
    end

    subgraph "Phase 2: Review Meeting"
        IR[InReview]
    end

    subgraph "Phase 3: Post-Review Confirmation"
        MRC[ManagerReviewConfirmed]
        ERC[EmployeeReviewConfirmed]
    end

    subgraph "Phase 4: Final (Locked)"
        F[Finalized]
    end

    A --> EIP & MIP & BIP
    EIP & MIP & BIP --> ES & MS
    ES & MS --> BS
    BS --> IR
    IR --> MRC
    MRC --> ERC
    ERC --> F

    A -.Auto-finalize.-> F

    style A fill:#e3f2fd
    style F fill:#e0e0e0,stroke:#424242,stroke-width:3px
```

---

## 3. Actor Permissions by State

```mermaid
graph LR
    subgraph "Employee Can Edit"
        A1[Assigned]
        EIP1[EmployeeInProgress]
        BIP1[BothInProgress]
        MS1[ManagerSubmitted]
    end

    subgraph "Manager Can Edit"
        A2[Assigned]
        MIP2[ManagerInProgress]
        BIP2[BothInProgress]
        ES2[EmployeeSubmitted]
    end

    subgraph "Manager Special Edit (During Review)"
        IR2[InReview]
    end

    subgraph "Both Read-Only"
        ES3[EmployeeSubmitted]
        MS3[ManagerSubmitted]
        BS3[BothSubmitted]
        MRC3[ManagerReviewConfirmed]
        ERC3[EmployeeReviewConfirmed]
        F3[Finalized]
    end

    style IR2 fill:#f3e5f5,stroke:#7b1fa2,stroke-width:3px
    style F3 fill:#e0e0e0,stroke:#424242,stroke-width:3px
```

---

## 4. Detailed State Characteristics Table

| State | Employee Can Edit? | Manager Can Edit? | Read-Only? | Can Submit? | Special Notes |
|-------|-------------------|-------------------|------------|-------------|---------------|
| **Assigned** | ✅ Yes | ✅ Yes | ❌ No | ❌ No | Initial state, no progress yet |
| **EmployeeInProgress** | ✅ Yes | ❌ No | ❌ No | ✅ Employee | Employee has started, manager hasn't |
| **ManagerInProgress** | ❌ No | ✅ Yes | ❌ No | ✅ Manager | Manager has started, employee hasn't |
| **BothInProgress** | ✅ Yes | ✅ Yes | ❌ No | ✅ Both | Both parties actively working |
| **EmployeeSubmitted** | ❌ No | ✅ Yes | ⚠️ Partial | ✅ Manager | Employee locked, manager can continue |
| **ManagerSubmitted** | ✅ Yes | ❌ No | ⚠️ Partial | ✅ Employee | Manager locked, employee can continue |
| **BothSubmitted** | ❌ No | ❌ No | ✅ Yes | ❌ No | Waiting for review initiation |
| **InReview** | ❌ No | ✅ Yes (All sections) | ⚠️ Partial | ❌ No | Manager-led review meeting in progress |
| **ManagerReviewConfirmed** | ❌ No | ❌ No | ✅ Yes | ❌ No | Waiting for employee confirmation |
| **EmployeeReviewConfirmed** | ❌ No | ❌ No | ✅ Yes | ❌ No | Waiting for manager finalization |
| **Finalized** | ❌ No | ❌ No | ✅ Yes | ❌ No | **Terminal state - Permanently locked** |

---

## 5. State Transition Rules Matrix

```mermaid
graph TD
    A[Current State] --> B{Is Finalized?}
    B -->|Yes| C[❌ REJECT: Terminal State]
    B -->|No| D{Target State in Valid Transitions?}
    D -->|No| E[❌ REJECT: Invalid Transition]
    D -->|Yes| F{Business Rules Met?}
    F -->|No| G[❌ REJECT: Preconditions Failed]
    F -->|Yes| H[✅ ALLOW: Raise WorkflowStateTransitioned Event]
    H --> I[Update Aggregate State]

    style C fill:#ffcdd2,stroke:#c62828
    style E fill:#ffcdd2,stroke:#c62828
    style G fill:#ffcdd2,stroke:#c62828
    style H fill:#c8e6c9,stroke:#2e7d32
    style I fill:#c8e6c9,stroke:#2e7d32
```

---

## 6. Section Completion Flow (Triggers State Changes)

```mermaid
sequenceDiagram
    participant E as Employee
    participant Agg as QuestionnaireAssignment
    participant SM as WorkflowStateMachine
    participant ES as Event Store

    E->>Agg: CompleteSectionAsEmployee(sectionId)
    Agg->>ES: Raise EmployeeSectionCompleted
    Agg->>Agg: Apply(EmployeeSectionCompleted)
    Agg->>Agg: UpdateWorkflowState()
    Agg->>SM: DetermineProgressState(hasEmployee, hasManager, current)
    SM-->>Agg: Return new state

    alt State Changed
        Agg->>SM: CanTransition(current, newState)
        SM-->>Agg: Valid ✅
        Agg->>ES: Raise WorkflowStateTransitioned
        Agg->>Agg: Apply(WorkflowStateTransitioned)
    else State Unchanged
        Agg->>Agg: No state transition needed
    end
```

---

## 7. Submission Flow (Both Parties)

```mermaid
sequenceDiagram
    participant E as Employee
    participant M as Manager
    participant Agg as QuestionnaireAssignment
    participant SM as WorkflowStateMachine

    Note over Agg: Initial State: BothInProgress

    E->>Agg: SubmitEmployeeQuestionnaire()
    Agg->>Agg: Raise EmployeeQuestionnaireSubmitted
    Agg->>SM: DetermineSubmissionState(empSubmit=true, mgrSubmit=false)
    SM-->>Agg: EmployeeSubmitted
    Agg->>Agg: Raise WorkflowStateTransitioned
    Note over Agg: State: EmployeeSubmitted

    M->>Agg: SubmitManagerQuestionnaire()
    Agg->>Agg: Raise ManagerQuestionnaireSubmitted
    Agg->>SM: DetermineSubmissionState(empSubmit=true, mgrSubmit=true)
    SM-->>Agg: BothSubmitted
    Agg->>Agg: Raise WorkflowStateTransitioned
    Note over Agg: State: BothSubmitted (Ready for Review)
```

---

## 8. Review Meeting Flow

```mermaid
sequenceDiagram
    participant M as Manager
    participant E as Employee
    participant Agg as QuestionnaireAssignment
    participant Resp as QuestionnaireResponse

    Note over Agg: State: BothSubmitted

    M->>Agg: InitiateReview()
    Agg->>Agg: Raise ReviewInitiated
    Agg->>Agg: Transition to InReview
    Note over Agg: State: InReview

    loop During Review Meeting
        M->>Resp: EditAnswerDuringReview(questionId, newValue)
        Resp->>Resp: Raise AnswerEditedDuringReview
        Note over E: Employee can view changes in real-time
    end

    M->>Agg: FinishReviewMeeting(summary)
    Agg->>Agg: Raise ManagerReviewMeetingFinished
    Agg->>Agg: Transition to ManagerReviewConfirmed
    Note over Agg: State: ManagerReviewConfirmed

    E->>Agg: ConfirmReviewOutcomeAsEmployee(comments)
    Agg->>Agg: Raise EmployeeConfirmedReviewOutcome
    Agg->>Agg: Transition to EmployeeReviewConfirmed
    Note over Agg: State: EmployeeReviewConfirmed

    M->>Agg: FinalizeAsManager(finalNotes)
    Agg->>Agg: Raise ManagerFinalizedQuestionnaire
    Agg->>Agg: Transition to Finalized
    Note over Agg: State: Finalized 🔒
```

---

## 9. Auto-Finalization Flow (No Manager Review Required)

```mermaid
graph TD
    A[QuestionnaireAssignment Created] --> B{RequiresManagerReview?}
    B -->|false| C[State: Assigned]
    B -->|true| D[Normal Workflow]

    C --> E[Employee fills sections]
    E --> F[State: EmployeeInProgress]
    F --> G[Employee submits]
    G --> H{RequiresManagerReview?}
    H -->|false| I[Auto-transition to Finalized]
    H -->|true| J[Normal submission flow]

    I --> K[Raise QuestionnaireAutoFinalized]
    K --> L[State: Finalized 🔒]
    L --> M[No review meeting needed]

    style I fill:#c8e6c9,stroke:#2e7d32,stroke-width:3px
    style L fill:#e0e0e0,stroke:#424242,stroke-width:3px
```

---

## 10. Invalid Transition Examples (Will Throw Exceptions)

```mermaid
graph LR
    A[Assigned] -.X.-> IR[InReview]
    ES[EmployeeSubmitted] -.X.-> IR2[InReview]
    IR3[InReview] -.X.-> F[Finalized]
    F2[Finalized] -.X.-> A2[Assigned]
    F3[Finalized] -.X.-> ANY[Any State]

    style A fill:#ffcdd2,stroke:#c62828
    style ES fill:#ffcdd2,stroke:#c62828
    style IR3 fill:#ffcdd2,stroke:#c62828
    style F2 fill:#ffcdd2,stroke:#c62828
    style F3 fill:#ffcdd2,stroke:#c62828
```

**Examples of Invalid Transitions:**
- ❌ `Assigned` → `InReview` (Must go through submission phase)
- ❌ `EmployeeSubmitted` → `InReview` (Manager must also submit)
- ❌ `InReview` → `Finalized` (Must confirm with employee)
- ❌ `Finalized` → `Assigned` (Terminal state, cannot revert)
- ❌ `Finalized` → `Any State` (Locked forever)

---

## 11. State Machine Implementation Architecture

```mermaid
graph TB
    subgraph "Domain Layer (01_Domain)"
        A[WorkflowState Enum]
        B[WorkflowTransitions<br/>Static Transition Matrix]
        C[WorkflowStateMachine<br/>Domain Service]
        D[QuestionnaireAssignment<br/>Aggregate Root]
        E[WorkflowStateTransitioned<br/>Domain Event]
        F[InvalidWorkflowTransitionException]
    end

    subgraph "Application Layer (02_Application)"
        G[Command Handlers]
        H[Query Handlers]
        I[GetWorkflowHistoryQuery]
    end

    subgraph "Infrastructure Layer (03_Infrastructure)"
        J[Event Store<br/>Marten]
        K[Read Models]
    end

    D -->|Uses| C
    D -->|Raises| E
    C -->|Validates| B
    C -->|Throws| F
    G -->|Commands| D
    H -->|Queries| J
    I -->|Reads| E
    E -->|Persisted in| J

    style C fill:#e3f2fd,stroke:#1976d2,stroke-width:3px
    style B fill:#fff3e0,stroke:#f57c00,stroke-width:3px
    style E fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
```

---

## 12. Key Business Rules Summary

### 🔵 Work Phase (Assigned → BothInProgress)
- Employee and Manager can work independently or simultaneously
- Completing sections transitions state automatically
- No explicit submission required in this phase

### 🟠 Submission Phase (EmployeeSubmitted → BothSubmitted)
- Once submitted, that party's answers are locked (Phase 1 Read-Only)
- Other party can continue working
- Both must submit before review can begin

### 🟣 Review Phase (InReview)
- Manager-led meeting
- Manager has elevated permissions (can edit all sections)
- Employee has read-only access during review
- Changes are tracked via `AnswerEditedDuringReview` events

### 🟢 Confirmation Phase (ManagerReviewConfirmed → EmployeeReviewConfirmed)
- Manager finishes first, provides summary
- Employee reviews and confirms
- Both parties acknowledge review outcome

### ⚫ Final Phase (Finalized)
- **Terminal State** - No transitions allowed
- Permanently locked
- Complete audit trail preserved

---

## 13. State Machine Validation Flow

```mermaid
flowchart TD
    Start([Command Received]) --> Load[Load QuestionnaireAssignment Aggregate]
    Load --> Check1{Is Withdrawn?}
    Check1 -->|Yes| Reject1[❌ Throw: Assignment is withdrawn]
    Check1 -->|No| Check2{Is Finalized?}
    Check2 -->|Yes| Reject2[❌ Throw: Assignment is locked]
    Check2 -->|No| Execute[Execute Business Logic]
    Execute --> DetermineState[Determine Target State]
    DetermineState --> Validate[WorkflowStateMachine.CanTransition]
    Validate --> Valid{Valid Transition?}
    Valid -->|No| Reject3[❌ Throw: InvalidWorkflowTransitionException]
    Valid -->|Yes| Raise[Raise WorkflowStateTransitioned Event]
    Raise --> Apply[Apply Event to Aggregate]
    Apply --> Persist[Persist to Event Store]
    Persist --> Success([✅ Success])

    style Reject1 fill:#ffcdd2,stroke:#c62828
    style Reject2 fill:#ffcdd2,stroke:#c62828
    style Reject3 fill:#ffcdd2,stroke:#c62828
    style Success fill:#c8e6c9,stroke:#2e7d32
    style Validate fill:#e3f2fd,stroke:#1976d2,stroke-width:3px
```

---

## 14. Testing Strategy Matrix

```mermaid
graph TB
    subgraph "Unit Tests"
        UT1[WorkflowStateMachine.CanTransition<br/>All valid transitions]
        UT2[WorkflowStateMachine.CanTransition<br/>All invalid transitions]
        UT3[DetermineProgressState<br/>Logic verification]
        UT4[DetermineSubmissionState<br/>Logic verification]
    end

    subgraph "Integration Tests"
        IT1[QuestionnaireAssignment<br/>State transition flows]
        IT2[Event sourcing<br/>WorkflowStateTransitioned persistence]
        IT3[Command handlers<br/>End-to-end workflows]
    end

    subgraph "E2E Tests"
        E2E1[Complete employee workflow<br/>Assigned → Finalized]
        E2E2[Review meeting flow<br/>BothSubmitted → Finalized]
        E2E3[Auto-finalization flow<br/>No manager review]
        E2E4[Invalid transition attempts<br/>Exception handling]
    end

    style UT1 fill:#e3f2fd
    style IT1 fill:#fff3e0
    style E2E1 fill:#f3e5f5
```

---

## 15. Transition Validation Rules (Detailed)

| From State | To State | Validation Rules | Can Fail? |
|------------|----------|------------------|-----------|
| **Assigned** | EmployeeInProgress | Employee completes at least 1 section | ✅ Yes (no sections) |
| **Assigned** | ManagerInProgress | Manager completes at least 1 section | ✅ Yes (no sections) |
| **Assigned** | BothInProgress | Both complete sections | ✅ Yes (not both) |
| **Assigned** | Finalized | `RequiresManagerReview == false` | ✅ Yes (requires review) |
| **EmployeeInProgress** | BothInProgress | Manager starts filling | ❌ No (automatic) |
| **EmployeeInProgress** | EmployeeSubmitted | Employee explicitly submits | ✅ Yes (validation fails) |
| **BothSubmitted** | InReview | Manager initiates review | ✅ Yes (not manager) |
| **InReview** | ManagerReviewConfirmed | Manager finishes meeting | ✅ Yes (validation fails) |
| **ManagerReviewConfirmed** | EmployeeReviewConfirmed | Employee confirms | ✅ Yes (not employee) |
| **EmployeeReviewConfirmed** | Finalized | Manager finalizes | ✅ Yes (not manager) |
| **Finalized** | Any | **Never allowed** | ❌ Always fails |

---

## 16. Event Sourcing Event Flow

```mermaid
sequenceDiagram
    participant Cmd as Command Handler
    participant Agg as Aggregate
    participant SM as State Machine
    participant ES as Event Store
    participant Proj as Projection

    Cmd->>Agg: ExecuteBusinessLogic()
    Agg->>Agg: DetermineTargetState()
    Agg->>SM: CanTransition(current, target)

    alt Valid Transition
        SM-->>Agg: ValidationResult.Valid
        Agg->>Agg: RaiseEvent(WorkflowStateTransitioned)
        Agg->>Agg: Apply(WorkflowStateTransitioned)
        Note over Agg: State updated in memory
        Agg-->>Cmd: Success
        Cmd->>ES: SaveEvents([WorkflowStateTransitioned, ...])
        ES-->>Proj: Notify projections
        Proj->>Proj: Update read models
    else Invalid Transition
        SM-->>Agg: ValidationResult.Invalid + reason
        Agg->>Agg: throw InvalidWorkflowTransitionException
        Agg-->>Cmd: Exception
        Note over Cmd: Transaction rolled back
    end
```

---

## 17. Performance Considerations

### State Machine Validation Performance
- ✅ **Dictionary lookups** - O(1) complexity
- ✅ **In-memory validation** - No database calls
- ✅ **Fail-fast design** - Exceptions thrown immediately
- ✅ **Zero external dependencies** - Pure domain logic

### Event Store Impact
- ⚠️ **New event type** `WorkflowStateTransitioned` adds ~1KB per transition
- ✅ **Event count** - Average 10-15 transitions per questionnaire lifecycle
- ✅ **Query optimization** - Can index on AssignmentId for history queries
- ✅ **Projection updates** - Read models updated asynchronously

---

## 18. Migration Strategy

### Existing Questionnaires Without WorkflowStateTransitioned Events

```mermaid
graph TD
    A[Existing Assignment] --> B{Has WorkflowStateTransitioned events?}
    B -->|Yes| C[Use Event-Sourced State]
    B -->|No| D[Reconstruct from other events]
    D --> E[Analyze EmployeeSectionCompleted]
    D --> F[Analyze EmployeeQuestionnaireSubmitted]
    D --> G[Analyze ManagerQuestionnaireSubmitted]
    D --> H[Analyze ReviewInitiated]
    D --> I[Build State Timeline]
    I --> J[Backfill WorkflowStateTransitioned events]
    J --> K[Mark as migrated]

    style J fill:#fff3e0,stroke:#f57c00,stroke-width:2px
```

---

## 19. Backward Transitions (Reopen Functionality)

### Overview
Certain states can be reopened to allow corrections before finalization. Reopening requires special permissions and is tracked for audit purposes.

### Authorization Rules
- **Admin**: Can reopen ALL non-finalized states
- **HR**: Can reopen ALL non-finalized states
- **TeamLead**: Can reopen ALL non-finalized states for their team members (includes review states)
- **Finalized state CANNOT be reopened** - must create new assignment

### Reopen State Diagram

```mermaid
stateDiagram-v2
    EmployeeSubmitted --> EmployeeInProgress: Reopen (Admin/HR/TeamLead)
    ManagerSubmitted --> ManagerInProgress: Reopen (Admin/HR/TeamLead)
    BothSubmitted --> BothInProgress: Reopen (Admin/HR/TeamLead)
    ManagerReviewConfirmed --> InReview: Reopen (Admin/HR/TeamLead)
    EmployeeReviewConfirmed --> InReview: Reopen (Admin/HR/TeamLead)

    note right of EmployeeSubmitted: Reopening submitted states<br/>allows corrections before review
    note right of ManagerReviewConfirmed: Reopening review states<br/>allows reworking review meeting

    Finalized: ❌ Cannot Reopen
    note right of Finalized: Terminal state - permanently locked<br/>Create new assignment if changes needed

    classDef reopenable fill:#fff9c4,stroke:#f57f17,stroke-width:2px
    classDef terminal fill:#ffcdd2,stroke:#c62828,stroke-width:3px

    class EmployeeSubmitted,ManagerSubmitted,BothSubmitted,ManagerReviewConfirmed,EmployeeReviewConfirmed reopenable
    class Finalized terminal
```

### Reopenable States Table

| From State | To State | Allowed Roles | Reason |
|------------|----------|---------------|---------|
| **EmployeeSubmitted** | EmployeeInProgress | Admin, HR, TeamLead | Employee questionnaire corrections |
| **ManagerSubmitted** | ManagerInProgress | Admin, HR, TeamLead | Manager questionnaire corrections |
| **BothSubmitted** | BothInProgress | Admin, HR, TeamLead | Both questionnaires need corrections |
| **ManagerReviewConfirmed** | InReview | Admin, HR, TeamLead | Rework review meeting (manager) |
| **EmployeeReviewConfirmed** | InReview | Admin, HR, TeamLead | Rework review meeting (after employee confirmation) |
| **Finalized** | N/A | ❌ None | Terminal state - cannot reopen |

### Reopen Command Flow

```mermaid
sequenceDiagram
    participant User as Admin/HR/TeamLead
    participant Cmd as ReopenQuestionnaireCommand
    participant Agg as QuestionnaireAssignment
    participant SM as WorkflowStateMachine
    participant Svc as EmployeeHierarchyService
    participant ES as Event Store

    User->>Cmd: ReopenQuestionnaire(assignmentId, toState, reason)
    Cmd->>Agg: Load aggregate from events

    alt Authorization Check (TeamLead)
        Cmd->>Svc: IsManagerOf(teamLeadId, employeeId)
        Svc-->>Cmd: true/false
        alt Not Authorized
            Cmd-->>User: 403 Forbidden
        end
    end

    Cmd->>SM: CanReopenToState(current, target, role)
    SM-->>Cmd: ValidationResult

    alt Invalid Reopen
        SM-->>Cmd: Invalid (wrong role or invalid transition)
        Cmd-->>User: 400 Bad Request
    else Valid Reopen
        SM-->>Cmd: Valid
        Cmd->>Agg: Reopen(toState, reason, role, userId)
        Agg->>Agg: RaiseEvent(WorkflowReopened)
        Agg->>Agg: Apply(WorkflowReopened)
        Cmd->>ES: SaveEvents([WorkflowReopened])
        Cmd-->>User: 200 OK (Audit trail recorded)
    end
```

### Reopen Audit Trail

Every reopen action is tracked with:
- **LastReopenedDate**: When the reopen occurred
- **LastReopenedByEmployeeId**: Who performed the reopen
- **LastReopenedByRole**: Which role was used (Admin/HR/TeamLead)
- **LastReopenReason**: Why the questionnaire was reopened

### Business Rules for Reopening

1. **Data Scoping for TeamLead**:
   - TeamLead can ONLY reopen assignments for their direct reports
   - Checked via `EmployeeHierarchyService.IsManagerOf()`
   - Fail-closed security: On error, deny access

2. **State Resets on Reopen**:
   - Reopening to `EmployeeInProgress` clears employee submission flags
   - Reopening to `ManagerInProgress` clears manager submission flags
   - Reopening to `BothInProgress` clears both submission flags
   - Reopening to `InReview` clears review confirmation flags

3. **Terminal State Protection**:
   - `Finalized` state cannot be reopened under any circumstances
   - If changes needed after finalization, create a new assignment

### Example Use Cases

**Use Case 1: Correct Employee Answer Before Review**
```
State: EmployeeSubmitted
Action: TeamLead notices error in employee's self-assessment
Result: Reopen to EmployeeInProgress, employee fixes answer, resubmits
```

**Use Case 2: Rework Review Meeting**
```
State: ManagerReviewConfirmed
Action: TeamLead realizes they need to add more feedback
Result: Reopen to InReview, TeamLead edits responses, finishes review again
```

**Use Case 3: Post-Employee Confirmation Adjustment**
```
State: EmployeeReviewConfirmed
Action: HR discovers compliance issue during final review
Result: Reopen to InReview, manager addresses issue, completes flow again
```

---

## Summary Statistics

### Workflow Complexity Metrics
- **Total States**: 13
- **Terminal States**: 1 (Finalized)
- **Phases**: 5 (Work, Submission, Review, Confirmation, Final)
- **Total Possible Transitions**: 15
- **Average Transitions to Complete**: 8-10
- **Max Path Length**: 10 transitions (Assigned → Finalized with review)
- **Min Path Length**: 1 transition (Assigned → Finalized via auto-finalize)

### State Machine Benefits
- ✅ **Type Safety**: Compile-time guarantees
- ✅ **Testability**: Isolated validation logic
- ✅ **Maintainability**: Centralized transition rules
- ✅ **Auditability**: Complete transition history
- ✅ **Documentation**: Self-documenting code
- ✅ **Extensibility**: Easy to add new states/transitions

---

## Next Steps

1. ✅ **Review this documentation** with stakeholders
2. ⏳ **Implement Phase 1**: State Machine Foundation
3. ⏳ **Write comprehensive tests**
4. ⏳ **Migrate existing assignments**
5. ⏳ **Deploy to staging**
6. ⏳ **Monitor transition events**
7. ⏳ **Implement advanced features** (history queries, visualization)

---

**Document Version**: 1.1
**Last Updated**: 2025-10-25
**Author**: Claude Code (Senior Software Architect)
**Status**: Ready for Implementation ✅
**Recent Changes**: Added Section 19 documenting reopen functionality with TeamLead authorization for review states
