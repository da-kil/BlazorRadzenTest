# Questionnaire Workflow State Diagram

```mermaid
stateDiagram-v2
    [*] --> Assigned: Assignment Created

    Assigned --> EmployeeInProgress: Employee starts work
    Assigned --> ManagerInProgress: Manager starts work

    EmployeeInProgress --> BothInProgress: Manager starts work
    ManagerInProgress --> BothInProgress: Employee starts work

    EmployeeInProgress --> EmployeeSubmitted: Employee submits
    ManagerInProgress --> ManagerSubmitted: Manager submits
    BothInProgress --> EmployeeSubmitted: Employee submits
    BothInProgress --> ManagerSubmitted: Manager submits

    EmployeeSubmitted --> BothSubmitted: Manager submits
    ManagerSubmitted --> BothSubmitted: Employee submits

    note right of BothSubmitted
        PHASE 1 READ-ONLY
        Both questionnaires submitted
        Waiting for review meeting
    end note

    BothSubmitted --> InReview: Manager initiates review

    note right of InReview
        EDITING ALLOWED
        Collaborative adjustments
        during review meeting
    end note

    InReview --> EmployeeReviewConfirmed: Employee confirms review
    InReview --> ManagerReviewConfirmed: Manager confirms review

    EmployeeReviewConfirmed --> ManagerReviewConfirmed: Manager confirms
    ManagerReviewConfirmed --> ManagerReviewConfirmed: Manager can finalize

    ManagerReviewConfirmed --> Finalized: Manager finalizes

    note right of Finalized
        PHASE 2 READ-ONLY
        Permanently locked
        No further changes allowed
    end note

    Finalized --> [*]

    classDef readOnly fill:#ffcccc,stroke:#cc0000,stroke-width:2px
    classDef editable fill:#ccffcc,stroke:#00cc00,stroke-width:2px
    classDef review fill:#cce5ff,stroke:#0066cc,stroke-width:2px

    class BothSubmitted,EmployeeSubmitted,ManagerSubmitted,Finalized readOnly
    class Assigned,EmployeeInProgress,ManagerInProgress,BothInProgress,InReview editable
    class EmployeeReviewConfirmed,ManagerReviewConfirmed review
```

## State Descriptions

### Initial States
- **Assigned**: Questionnaire assigned to employee, both parties can start

### Work In Progress States (Editable)
- **EmployeeInProgress**: Employee has started working on their sections
- **ManagerInProgress**: Manager has started working on their sections
- **BothInProgress**: Both employee and manager are working on their respective sections

### Submission States (Phase 1 Read-Only)
- **EmployeeSubmitted**: Employee has submitted their questionnaire, waiting for manager
- **ManagerSubmitted**: Manager has submitted their questionnaire, waiting for employee
- **BothSubmitted**: Both parties have submitted, ready for review meeting

### Review States
- **InReview**: Review meeting initiated, editing allowed for collaborative adjustments (Editable)
- **EmployeeReviewConfirmed**: Employee has confirmed the review results
- **ManagerReviewConfirmed**: Manager has confirmed the review results

### Final State (Phase 2 Read-Only - Permanent)
- **Finalized**: Questionnaire finalized and permanently locked by manager

## Actions by State

| State | Employee Actions | Manager Actions |
|-------|------------------|-----------------|
| Assigned | Start working on sections | Start working on sections |
| EmployeeInProgress | Complete sections, Submit | Start working on sections |
| ManagerInProgress | Start working on sections | Complete sections, Submit |
| BothInProgress | Complete sections, Submit | Complete sections, Submit |
| EmployeeSubmitted | ❌ Wait for manager | Complete sections, Submit |
| ManagerSubmitted | Complete sections, Submit | ❌ Wait for employee |
| BothSubmitted | ❌ Wait for review | Initiate Review Meeting |
| InReview | Edit answers, Confirm Review | Edit answers, Confirm Review |
| EmployeeReviewConfirmed | ❌ Wait for finalization | Confirm Review, Finalize |
| ManagerReviewConfirmed | ❌ Wait for finalization | Finalize |
| Finalized | ❌ Read-only | ❌ Read-only |

## Read-Only Phases

### Phase 1 Read-Only (Temporary)
Occurs after submission, before review meeting:
- `EmployeeSubmitted`
- `ManagerSubmitted`
- `BothSubmitted`

**Purpose**: Lock questionnaire for review preparation, ensure no changes before meeting

**Recovery**: Manager initiates review → transitions to `InReview` → editing allowed again

### Phase 2 Read-Only (Permanent)
Occurs after finalization:
- `Finalized`

**Purpose**: Permanent archive of performance review, no changes allowed

**Recovery**: None - this is the final state

## Key Workflow Features

1. **Parallel Completion**: Employee and manager can work independently on their sections
2. **Submit vs Confirm**: Single action to submit/approve questionnaire (not two separate steps)
3. **Review Meeting Flexibility**: During `InReview`, both parties can make collaborative adjustments
4. **Manager Control**: Manager controls key transitions (initiate review, finalize)
5. **Two-Phase Read-Only**: Temporary read-only before review, permanent read-only after finalization
