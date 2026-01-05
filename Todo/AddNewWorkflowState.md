# TODO: Add "Initialized" Workflow State

**Status**: 🔴 Not Started
**Priority**: High
**Estimated Effort**: 13-16 days
**Branch**: `feature/initialized-workflow-state`

## Overview

Add a new "Initialized" workflow state (value = 1) between "Assigned" and "EmployeeInProgress" that enables manager-only initialization tasks before the questionnaire becomes available to employees.

**Key Changes:**
- Renumber all WorkflowState enum values after Assigned (1-10 → 2-11)
- Manager-only access during Assigned and Initialized states
- Optional initialization tasks: link predecessor, add custom questions
- Custom questions locked after initialization
- Database wipe required (no backward compatibility)

---

## Phase 1: Domain Foundation (3-4 days)

### 1.1 Database Migration
- [ ] **Backup test data** (if any critical data exists)
- [ ] **Drop database**: `DROP DATABASE beachbreak_dev CASCADE;`
- [ ] **Create database**: `CREATE DATABASE beachbreak_dev;`
- [ ] **Verify Aspire auto-migration** works

**Files**: None (database operation)

---

### 1.2 Update WorkflowState Enums
- [ ] **Update Domain enum**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/WorkflowState.cs`
  - [ ] Add `Initialized = 1`
  - [ ] Renumber all states: EmployeeInProgress=2, ManagerInProgress=3, BothInProgress=4, EmployeeSubmitted=5, ManagerSubmitted=6, BothSubmitted=7, InReview=8, ReviewFinished=9, EmployeeReviewConfirmed=10, Finalized=11
  - [ ] Verify ALL values are explicit (CRITICAL for CQRS)

- [ ] **Update Frontend enum**: `05_Frontend/ti8m.BeachBreak.Client/Models/WorkflowState.cs`
  - [ ] Apply same changes as domain enum
  - [ ] Ensure synchronization with domain

**Verification**: Run application and check no serialization errors

---

### 1.3 Create Domain Events

- [ ] **Create AssignmentInitialized event**
  - **File**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/Events/AssignmentInitialized.cs`
  - [ ] Properties: AggregateId, InitializedDate, InitializedByEmployeeId, InitializationNotes
  - [ ] Implement IDomainEvent
  - [ ] Add to QuestionnaireAssignment.Apply() method

- [ ] **Create CustomSectionsAddedToAssignment event**
  - **File**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/Events/CustomSectionsAddedToAssignment.cs`
  - [ ] Properties: AssignmentId, CustomSections (List<QuestionSectionData>), AddedDate, AddedByEmployeeId
  - [ ] Create QuestionSectionData nested record if needed
  - [ ] Implement IDomainEvent
  - [ ] Add to QuestionnaireAssignment.Apply() method

---

### 1.4 Update QuestionSection Model

- [ ] **Update Domain QuestionSection**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/QuestionSection.cs`
  - [ ] Add property: `public bool IsInstanceSpecific { get; private set; } = false`
  - [ ] Add CreateCustomSection() factory method
  - [ ] Ensure MapToData/MapFromData handle IsInstanceSpecific

- [ ] **Update Frontend QuestionSection**: `05_Frontend/ti8m.BeachBreak.Client/Models/QuestionSection.cs`
  - [ ] Add property: `public bool IsInstanceSpecific { get; set; } = false`
  - [ ] Ensure serialization works

**Purpose**: IsInstanceSpecific=true marks custom questions to exclude from reports

---

### 1.5 Update QuestionnaireAssignment Aggregate

**File**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/QuestionnaireAssignment.cs`

- [ ] **Add properties**:
  - [ ] `public DateTime? InitializedDate { get; private set; }`
  - [ ] `public Guid? InitializedByEmployeeId { get; private set; }`
  - [ ] `public string? InitializationNotes { get; private set; }`
  - [ ] `private readonly List<QuestionSection> _customSections = new()`
  - [ ] `public IReadOnlyList<QuestionSection> CustomSections => _customSections.AsReadOnly()`

- [ ] **Add business methods**:
  - [ ] `StartInitialization(Guid startedBy)` - Validates state is Assigned, raises AssignmentInitialized event
  - [ ] `AddCustomSections(List<QuestionSection> sections, Guid addedBy)` - Validates state is Initialized, sections are instance-specific, no Goals allowed
  - [ ] Add MapToData/MapFromData helper methods for QuestionSectionData

- [ ] **Update authorization methods**:
  - [ ] `CanEmployeeEdit()` - Exclude Initialized state
  - [ ] `CanManagerEdit()` - Include Initialized state
  - [ ] Update `LinkPredecessorQuestionnaire()` - Allow during Initialized state for managers

- [ ] **Add Apply methods**:
  - [ ] `Apply(AssignmentInitialized @event)` - Set InitializedDate, InitializedByEmployeeId, InitializationNotes, WorkflowState=Initialized
  - [ ] `Apply(CustomSectionsAddedToAssignment @event)` - Add sections to _customSections

**Tests**: Write unit tests for new methods

---

### 1.6 Update WorkflowTransitions

**File**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/WorkflowTransitions.cs`

- [ ] **Add ForwardTransitions**:
  - [ ] `[WorkflowState.Assigned]` - Add `new(WorkflowState.Initialized, "transitions.manager-starts-initialization")`
  - [ ] `[WorkflowState.Initialized]` - NEW entry with transitions to EmployeeInProgress and ManagerInProgress

- [ ] **Add BackwardTransitions**:
  - [ ] `[WorkflowState.Initialized]` - NEW entry with transition to Assigned (Admin/HR/TeamLead)

**Translation Keys** (add to test-translations.json):
- [ ] `transitions.manager-starts-initialization` (EN/DE)
- [ ] `transitions.employee-starts-filling` (update existing)
- [ ] `transitions.manager-starts-filling` (update existing)
- [ ] `reopen.reset-initialization` (EN/DE)

---

### 1.7 Update WorkflowStateMachine

**File**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/WorkflowStateMachine.cs`

- [ ] **Review DetermineProgressState()** - Ensure it doesn't auto-transition from Assigned/Initialized
- [ ] **Check for hardcoded state checks** - Update any methods that explicitly check states
- [ ] **Test all transition validations** still work

---

### 1.8 Write Domain Unit Tests

**Location**: `01_Domain/ti8m.BeachBreak.Domain.Tests/`

- [ ] **Create InitializationTests.cs**:
  - [ ] Test Assigned → Initialized transition
  - [ ] Test StartInitialization() validation (only from Assigned state)
  - [ ] Test manager authorization
  - [ ] Test invalid state transitions

- [ ] **Create CustomSectionsTests.cs**:
  - [ ] Test AddCustomSections() success
  - [ ] Test validation: only during Initialized state
  - [ ] Test validation: all sections must have IsInstanceSpecific=true
  - [ ] Test validation: no Goal type questions allowed
  - [ ] Test custom sections are locked after initialization

- [ ] **Update WorkflowTransitionsTests.cs**:
  - [ ] Update all existing tests for renumbered enums
  - [ ] Add tests for new Initialized state transitions

---

## Phase 2: Application Layer (2 days)

### 2.1 Create Initialize Assignment Command

- [ ] **Create command**: `02_Application/ti8m.BeachBreak.Application.Command/Commands/QuestionnaireAssignmentCommands/InitializeAssignmentCommand.cs`
  - [ ] Properties: AssignmentId, InitializedByEmployeeId, InitializationNotes
  - [ ] Result type: Result

- [ ] **Create handler**: `InitializeAssignmentCommandHandler.cs`
  - [ ] Load assignment from repository
  - [ ] Validate state is Assigned
  - [ ] Call `assignment.StartInitialization()`
  - [ ] Persist aggregate
  - [ ] Add logging call

- [ ] **Add logging definition**: `LoggerMessageDefinitions.cs`
  - [ ] `LogAssignmentInitialized(Guid assignmentId, Guid employeeId)` with EventId 6100

**Tests**: Create command handler unit tests

---

### 2.2 Create Add Custom Sections Command

- [ ] **Create command**: `02_Application/ti8m.BeachBreak.Application.Command/Commands/QuestionnaireAssignmentCommands/AddCustomSectionsCommand.cs`
  - [ ] Properties: AssignmentId, Sections (List<CommandQuestionSection>), AddedByEmployeeId
  - [ ] Result type: Result

- [ ] **Create handler**: `AddCustomSectionsCommandHandler.cs`
  - [ ] Load assignment from repository
  - [ ] Validate state is Initialized
  - [ ] Map CommandQuestionSection DTOs to domain QuestionSection entities
  - [ ] Call `assignment.AddCustomSections()`
  - [ ] Persist aggregate
  - [ ] Add logging call

- [ ] **Add logging definition**: `LoggerMessageDefinitions.cs`
  - [ ] `LogCustomSectionsAdded(Guid assignmentId, Guid employeeId)` with EventId 6101

**Tests**: Create command handler unit tests

---

### 2.3 Update Read Model Projections

**File**: `02_Application/ti8m.BeachBreak.Application.Query/Projections/QuestionnaireAssignmentReadModel.cs`

- [ ] **Add properties**:
  - [ ] `public DateTime? InitializedDate { get; set; }`
  - [ ] `public Guid? InitializedByEmployeeId { get; set; }`
  - [ ] `public string? InitializationNotes { get; set; }`
  - [ ] `public List<QuestionSectionReadModel> CustomSections { get; set; } = new()`

- [ ] **Add Apply methods**:
  - [ ] `Apply(AssignmentInitialized @event)` - Set properties, WorkflowState=Initialized
  - [ ] `Apply(CustomSectionsAddedToAssignment @event)` - Add to CustomSections list

- [ ] **Create QuestionSectionReadModel** (if doesn't exist)

**Verification**: Check projections rebuild correctly

---

### 2.4 Create Custom Sections Query

- [ ] **Create query**: `02_Application/ti8m.BeachBreak.Application.Query/Queries/QuestionnaireAssignmentQueries/GetAssignmentCustomSectionsQuery.cs`
  - [ ] Property: AssignmentId
  - [ ] Result: List<QuestionSectionDto>

- [ ] **Create handler**: `GetAssignmentCustomSectionsQueryHandler.cs`
  - [ ] Load assignment read model
  - [ ] Return CustomSections as DTOs

**Tests**: Create query handler tests

---

## Phase 3: Infrastructure Layer (2 days)

### 3.1 Create CommandApi DTOs

**Location**: `03_Infrastructure/ti8m.BeachBreak.CommandApi/Dto/`

- [ ] **Create InitializeAssignmentDto.cs**:
  - [ ] `string? InitializationNotes`

- [ ] **Create AddCustomSectionsDto.cs**:
  - [ ] `List<QuestionSectionDto> Sections`

---

### 3.2 Add CommandApi Endpoints

**File**: `03_Infrastructure/ti8m.BeachBreak.CommandApi/Controllers/AssignmentsController.cs`

- [ ] **POST /api/v1/assignments/{id}/initialize**:
  - [ ] `[Authorize(Policy = "TeamLead")]`
  - [ ] Create InitializeAssignmentCommand from DTO
  - [ ] Use UserContext.Id for InitializedByEmployeeId
  - [ ] Dispatch command
  - [ ] Return `CreateResponse(result)` (NOT Ok/BadRequest)

- [ ] **POST /api/v1/assignments/{id}/custom-sections**:
  - [ ] `[Authorize(Policy = "TeamLead")]`
  - [ ] Map AddCustomSectionsDto to command
  - [ ] Use UserContext.Id for AddedByEmployeeId
  - [ ] Dispatch command
  - [ ] Return `CreateResponse(result)`

**Tests**: Create API integration tests

---

### 3.3 Add QueryApi Endpoint

**File**: `03_Infrastructure/ti8m.BeachBreak.QueryApi/Controllers/AssignmentsController.cs` (or similar)

- [ ] **GET /api/v1/assignments/{id}/custom-sections**:
  - [ ] `[Authorize]`
  - [ ] Create GetAssignmentCustomSectionsQuery
  - [ ] Dispatch query
  - [ ] Return Ok(sections)

**Tests**: Create API integration tests

---

## Phase 4: Frontend Layer (3-4 days)

### 4.1 Update WorkflowStateHelper

**File**: `05_Frontend/ti8m.BeachBreak.Client/Models/WorkflowStateHelper.cs`

- [ ] **Update StateOrder list** - Insert Initialized after Assigned

- [ ] **Add to GetStateDisplayName()**: `WorkflowState.Initialized => "workflow-states.initialized"`

- [ ] **Add to GetStateColor()**: `WorkflowState.Initialized => "var(--rz-info)"`

- [ ] **Add to GetStateIcon()**: `WorkflowState.Initialized => "settings"`

- [ ] **Update CanEmployeeEdit()** - Exclude Initialized state

- [ ] **Update CanManagerEdit()** - Include Initialized state

- [ ] **Add new helper methods**:
  - [ ] `CanEmployeeView(QuestionnaireAssignment)` - Return false for Assigned/Initialized
  - [ ] `CanManagerInitialize(QuestionnaireAssignment)` - Return true for Assigned/Initialized
  - [ ] `CanAddCustomSections(QuestionnaireAssignment)` - Return true only for Initialized

- [ ] **Update GetNextActionForEmployee()** - Add Assigned and Initialized cases

- [ ] **Update GetNextActionForManager()** - Add Assigned and Initialized cases

**Translation Keys** (~15 keys):
- [ ] `workflow-states.initialized` (EN/DE)
- [ ] `actions.employee.waiting-manager-initialization` (EN/DE)
- [ ] `actions.manager.initialize-assignment` (EN/DE)
- [ ] `actions.manager.complete-initialization` (EN/DE)

**Tests**: Update WorkflowStateHelper unit tests

---

### 4.2 Create Frontend DTOs

**Location**: `05_Frontend/ti8m.BeachBreak.Client/Models/Dto/`

- [ ] **Create InitializeAssignmentDto.cs**:
  - [ ] `string? InitializationNotes`

- [ ] **Create AddCustomSectionsDto.cs**:
  - [ ] `List<QuestionSection> Sections`

---

### 4.3 Update AssignmentApiService

**File**: `05_Frontend/ti8m.BeachBreak.Client/Services/AssignmentApiService.cs`

- [ ] **Add InitializeAssignmentAsync()**:
  - [ ] POST to `/c/api/v1/assignments/{id}/initialize`
  - [ ] Return Result

- [ ] **Add AddCustomSectionsAsync()**:
  - [ ] POST to `/c/api/v1/assignments/{id}/custom-sections`
  - [ ] Return Result

- [ ] **Add GetCustomSectionsAsync()**:
  - [ ] GET from `/q/api/v1/assignments/{id}/custom-sections`
  - [ ] Return List<QuestionSection>

---

### 4.4 Create Initialization Page

**File**: `05_Frontend/ti8m.BeachBreak.Client/Pages/InitializeAssignment.razor`

- [ ] **Setup route**: `@page "/assignments/{assignmentId:guid}/initialize"`

- [ ] **Add authorization**: `<AuthorizeView Policy="TeamLead">`

- [ ] **Display assignment details**:
  - [ ] Employee name
  - [ ] Template name
  - [ ] Due date
  - [ ] Category

- [ ] **Initialization tasks section**:
  - [ ] Link predecessor questionnaire (optional)
    - [ ] Button opens LinkPredecessorDialog
    - [ ] Show "Linked" status if predecessor linked
  - [ ] Add custom questions (optional)
    - [ ] Button opens AddCustomQuestionDialog
    - [ ] Display list of added custom questions
    - [ ] Edit/delete custom questions

- [ ] **Initialization notes**:
  - [ ] RadzenTextArea for manager notes

- [ ] **Complete Initialization button**:
  - [ ] Calls InitializeAssignmentAsync()
  - [ ] Validation: notes optional
  - [ ] Navigate back on success

- [ ] **Cancel button**: Navigate back to dashboard

**Translation Keys** (~20 keys):
- [ ] Page title, section headings
- [ ] Button labels (initialize, cancel, link predecessor, add question)
- [ ] Help text, descriptions
- [ ] Validation messages

**Component Dependencies**:
- [x] Reuse `LinkPredecessorDialog.razor` (already exists)
- [ ] Create `AddCustomQuestionDialog.razor` (next task)

---

### 4.5 Create Add Custom Question Dialog

**File**: `05_Frontend/ti8m.BeachBreak.Client/Components/Dialogs/AddCustomQuestionDialog.razor`

- [ ] **Question type selector**:
  - [ ] Assessment (with radio button)
  - [ ] TextQuestion (with radio button)
  - [ ] NO Goal option

- [ ] **Reuse question builder components**:
  - [ ] Use SectionCard.razor for editing
  - [ ] Use existing configuration editors (Assessment/Text)

- [ ] **Validation**:
  - [ ] Title required (English + German)
  - [ ] Description required (English + German)
  - [ ] For Assessment: at least one evaluation item
  - [ ] For TextQuestion: at least one text section

- [ ] **Custom questions list**:
  - [ ] Display added questions
  - [ ] Edit button (reopens editor)
  - [ ] Delete button
  - [ ] Reorder buttons (up/down)

- [ ] **Dialog buttons**:
  - [ ] Add Question (adds to list)
  - [ ] Confirm (returns list, closes dialog)
  - [ ] Cancel (closes dialog)

**Translation Keys** (~10 keys):
- [ ] Dialog title
- [ ] Question type labels
- [ ] Button labels
- [ ] Validation messages

**Component Reuse**:
- [ ] `QuestionnaireBuilder/SectionCard.razor`
- [ ] `QuestionnaireBuilder/AssessmentConfiguration.razor`
- [ ] `QuestionnaireBuilder/TextQuestionConfiguration.razor`

---

### 4.6 Update DynamicQuestionnaire

**File**: `05_Frontend/ti8m.BeachBreak.Client/Pages/DynamicQuestionnaire.razor`

- [ ] **Load custom sections** in OnInitializedAsync():
  - [ ] Call `AssignmentApiService.GetCustomSectionsAsync()`
  - [ ] Merge with template sections
  - [ ] Sort by Order property

- [ ] **Display custom section badge**:
  - [ ] Check `section.IsInstanceSpecific`
  - [ ] Show RadzenBadge with "labels.custom-question" text
  - [ ] Use BadgeStyle.Info (blue)

- [ ] **Render custom sections**: Use existing OptimizedQuestionRenderer (no changes needed)

**Translation Key**:
- [ ] `labels.custom-question` (EN: "Custom Question", DE: "Individuelle Frage")

---

### 4.7 Update MyQuestionnaires Page

**File**: `05_Frontend/ti8m.BeachBreak.Client/Pages/MyQuestionnaires.razor`

- [ ] **Filter assignments for employees**:
  - [ ] Use `WorkflowStateHelper.CanEmployeeView()` to filter
  - [ ] Employees don't see Assigned or Initialized assignments

- [ ] **Navigation logic**:
  - [ ] Check CanEmployeeView before navigation
  - [ ] Show notification if questionnaire not available

**Translation Key**:
- [ ] `messages.questionnaire-not-available-yet` (EN/DE)

---

### 4.8 Update ManagerDashboard Page

**File**: `05_Frontend/ti8m.BeachBreak.Client/Pages/ManagerDashboard.razor`

- [ ] **Add initialization indicator**:
  - [ ] Show "Initialize" button for Assigned state assignments
  - [ ] Show "Continue Initialization" button for Initialized state
  - [ ] Use ButtonStyle.Info

- [ ] **Navigation logic**:
  - [ ] Navigate to `/assignments/{id}/initialize` for Assigned/Initialized states
  - [ ] Navigate to normal questionnaire page for other states

**Translation Key**:
- [ ] `buttons.initialize` (EN: "Initialize", DE: "Initialisieren")

---

### 4.9 Filter Custom Sections from Reports

**Location**: Report generation services/components

- [ ] **Identify report generation locations**:
  - [ ] Organization reports
  - [ ] Team reports
  - [ ] Aggregate reports

- [ ] **Add filtering logic**:
  - [ ] Filter sections where `IsInstanceSpecific = true`
  - [ ] Ensure custom questions excluded from aggregate data

**Design Decision**:
- [ ] Discuss: Should individual employee reports include custom sections?

---

### 4.10 Add All Translation Keys

**File**: `TestDataGenerator/test-translations.json`

**Workflow States** (2 keys):
- [ ] `workflow-states.initialized` (EN/DE)

**Transitions** (4 keys):
- [ ] `transitions.manager-starts-initialization` (EN/DE)
- [ ] `transitions.employee-starts-filling` (update if needed)
- [ ] `transitions.manager-starts-filling` (update if needed)
- [ ] `reopen.reset-initialization` (EN/DE)

**Actions** (6 keys):
- [ ] `actions.employee.waiting-manager-initialization` (EN/DE)
- [ ] `actions.manager.initialize-assignment` (EN/DE)
- [ ] `actions.manager.complete-initialization` (EN/DE)

**Initialization Page** (~20 keys):
- [ ] Page title (e.g., "Initialize Assignment")
- [ ] Section headings ("Initialization Tasks", "Optional Tasks")
- [ ] Task labels ("Link Predecessor Questionnaire", "Add Custom Questions")
- [ ] Button labels ("Complete Initialization", "Cancel", "Add Question")
- [ ] Help text/descriptions
- [ ] Validation messages

**Dialogs** (~10 keys):
- [ ] Add custom question dialog title
- [ ] Question type labels (Assessment, Text)
- [ ] Dialog button labels
- [ ] Validation messages

**Labels** (3 keys):
- [ ] `labels.custom-question` (EN/DE)
- [ ] `buttons.initialize` (EN/DE)
- [ ] `messages.questionnaire-not-available-yet` (EN/DE)

**Total**: ~45-50 translation keys (English + German)

- [ ] **Run validation**: `powershell -ExecutionPolicy Bypass -File TestDataGenerator\validate-translations.ps1`

---

## Phase 5: Testing (2-3 days)

### 5.1 Run All Unit Tests

- [ ] Run domain tests: `dotnet test 01_Domain/ti8m.BeachBreak.Domain.Tests/`
- [ ] Run application tests: `dotnet test 02_Application/`
- [ ] Fix any failing tests
- [ ] Verify code coverage

---

### 5.2 Run Integration Tests

- [ ] Run API tests
- [ ] Test initialize endpoint
- [ ] Test add custom sections endpoint
- [ ] Test get custom sections endpoint
- [ ] Fix any failures

---

### 5.3 Manual Testing - Manager Flow

- [ ] Create assignment → verify starts in Assigned state
- [ ] Verify employee cannot see assignment (check MyQuestionnaires)
- [ ] Navigate to initialization page as manager
- [ ] Link predecessor questionnaire (optional)
  - [ ] Verify LinkPredecessorDialog opens
  - [ ] Select and link predecessor
  - [ ] Verify "Linked" status shown
- [ ] Add custom Assessment question
  - [ ] Open AddCustomQuestionDialog
  - [ ] Select Assessment type
  - [ ] Add evaluation items
  - [ ] Configure rating scale
  - [ ] Save question
  - [ ] Verify appears in custom questions list
- [ ] Add custom TextQuestion
  - [ ] Open dialog again
  - [ ] Select TextQuestion type
  - [ ] Add text sections
  - [ ] Save question
  - [ ] Verify appears in list
- [ ] Attempt to add Goal question → verify rejected
- [ ] Complete initialization
  - [ ] Enter notes (optional)
  - [ ] Click Complete Initialization
  - [ ] Verify state becomes Initialized
- [ ] Attempt to add more custom questions → verify rejected/locked
- [ ] Start filling questionnaire (as manager)
  - [ ] Complete first section
  - [ ] Verify state transitions to ManagerInProgress
- [ ] Verify employee can now see assignment

---

### 5.4 Manual Testing - Employee Flow

- [ ] Login as employee
- [ ] Verify cannot see Assigned state assignments
- [ ] Verify cannot see Initialized state assignments
- [ ] Wait for manager to complete initialization
- [ ] Verify assignment appears after state becomes Initialized (waiting for first edit)
- [ ] Open questionnaire
- [ ] Fill sections including custom questions
  - [ ] Verify custom sections show badge ("Custom Question")
  - [ ] Verify custom Assessment question renders correctly
  - [ ] Verify custom TextQuestion renders correctly
- [ ] Complete first section
- [ ] Verify state transitions to EmployeeInProgress or BothInProgress
- [ ] Submit questionnaire

---

### 5.5 Manual Testing - Reports

- [ ] Generate organization report
- [ ] Verify custom sections NOT included (IsInstanceSpecific=true filtered)
- [ ] Generate team report
- [ ] Verify custom sections NOT included
- [ ] (Optional) Generate individual employee report
- [ ] Discuss: Should custom sections appear in individual reports?

---

### 5.6 Manual Testing - Authorization

- [ ] Verify TeamLead can initialize assignments for their team
- [ ] Verify HR can initialize any assignment
- [ ] Verify Admin can initialize any assignment
- [ ] Verify Employee cannot access initialization page
- [ ] Verify authorization checks at API level (try direct API calls)

---

### 5.7 End-to-End Workflow Test

- [ ] Complete full workflow from assignment to finalization
  1. HR creates assignment → Assigned
  2. Manager initializes (link predecessor + add custom questions) → Initialized
  3. Manager starts filling → ManagerInProgress
  4. Employee starts filling → BothInProgress
  5. Both submit → BothSubmitted
  6. Manager initiates review → InReview
  7. Manager finishes review → ReviewFinished
  8. Employee signs off → EmployeeReviewConfirmed
  9. Manager finalizes → Finalized

- [ ] Verify custom questions appear throughout workflow
- [ ] Verify custom questions excluded from reports

---

## Phase 6: Documentation (1 day)

### 6.1 Update CLAUDE.md

**File**: `C:\projects\BlazorRadzenTest\CLAUDE.md`

- [ ] **Add section**: "Workflow State Pattern - Initialized State"
  - [ ] Document Initialized state behavior
  - [ ] Document manager-only access rules
  - [ ] Document initialization tasks (predecessor, custom questions)
  - [ ] Document state transition logic

- [ ] **Update section**: "Authorization Pattern" (if needed)
  - [ ] Document CanEmployeeView() pattern
  - [ ] Document CanManagerInitialize() pattern

- [ ] **Add section**: "Custom Questions Pattern"
  - [ ] Document IsInstanceSpecific flag
  - [ ] Document custom question constraints (no Goal type)
  - [ ] Document locking after initialization
  - [ ] Document report filtering

---

### 6.2 Update Architecture Documentation

**Check if exists**: `/docu` directory

- [ ] **Create/Update**: "Workflow States.md"
  - [ ] Document all 12 workflow states
  - [ ] Document state transition rules
  - [ ] Document authorization by state
  - [ ] Include state diagram (if applicable)

- [ ] **Create/Update**: "Custom Questions.md"
  - [ ] Document custom questions feature
  - [ ] Document manager initialization workflow
  - [ ] Document how custom questions differ from template questions
  - [ ] Document report exclusion logic

---

### 6.3 Update API Documentation

- [ ] **Document new endpoints**:
  - [ ] POST /api/v1/assignments/{id}/initialize
  - [ ] POST /api/v1/assignments/{id}/custom-sections
  - [ ] GET /api/v1/assignments/{id}/custom-sections

- [ ] **Update Swagger annotations** (if used)

---

### 6.4 Create Migration Guide

**File**: `Todo/InitializedStateMigration.md` (or similar)

- [ ] **Document breaking changes**:
  - [ ] Enum renumbering (1-10 → 2-11)
  - [ ] Database wipe required
  - [ ] No backward compatibility

- [ ] **Document database migration steps**

- [ ] **Document testing strategy**

---

## Success Criteria Checklist

Before marking this feature complete, verify ALL criteria:

### Core Functionality
- [ ] Enum values consistent across Domain and Frontend layers
- [ ] Employee cannot view/access Assigned or Initialized assignments
- [ ] Manager can initialize assignment and complete initialization tasks
- [ ] Manager can link predecessor questionnaire during Initialized state
- [ ] Manager can add custom Assessment and TextQuestion sections
- [ ] Goal type questions rejected for custom sections
- [ ] Custom sections locked after initialization (cannot edit in later states)

### UI & UX
- [ ] Custom sections appear in questionnaire with "Custom Question" badge
- [ ] Initialization page is intuitive and easy to use
- [ ] Manager dashboard shows initialization status clearly
- [ ] Employee dashboard hides uninitialized assignments

### Data & Reports
- [ ] Custom sections excluded from aggregate reports (IsInstanceSpecific flag)
- [ ] Custom sections stored in assignment (not template)
- [ ] Custom sections persist through workflow

### Workflow
- [ ] State stays in Initialized until first section edit
- [ ] First edit triggers normal progress state logic (EmployeeInProgress/ManagerInProgress)
- [ ] All existing workflows still function correctly
- [ ] Predecessor linking works in Initialized state

### Technical
- [ ] Database cleanly migrated (wiped and recreated)
- [ ] All tests pass (unit, integration, E2E)
- [ ] All translations added (English + German)
- [ ] No serialization errors with CQRS
- [ ] Logging works for all new commands

### Documentation
- [ ] CLAUDE.md updated with new patterns
- [ ] Architecture documentation updated
- [ ] API documentation updated
- [ ] Migration guide created

---

## Risk Mitigation Tracking

| Risk | Status | Notes |
|------|--------|-------|
| Enum value mismatch between layers | ⚠️ Monitor | Use explicit values; test thoroughly |
| Data migration errors | ✅ Mitigated | Wiped dev database |
| Authorization bypass | ⚠️ Monitor | Multi-layer checks implemented |
| Custom sections appear in reports | ⚠️ Monitor | IsInstanceSpecific filtering |
| Predecessor linking breaks | ⚠️ Monitor | Allowed during Initialized state |
| Employee sees uninitialized assignments | ⚠️ Monitor | CanEmployeeView() filtering |
| Custom questions editable after init | ⚠️ Monitor | Domain validation in AddCustomSections |

---

## Notes

### Key Design Decisions
- **No backward compatibility**: Database wipe approved by user
- **Optional tasks**: Both predecessor linking and custom questions are optional
- **State transition**: Stays in Initialized until first edit (not auto-transition)
- **Custom questions locked**: After initialization completes
- **Report exclusion**: Custom questions excluded from aggregate reports via IsInstanceSpecific flag

### Architecture Patterns
- **Enum explicit values**: Per CLAUDE.md Section 8 (CQRS requirement)
- **Strongly-typed config**: Per CLAUDE.md Section 11 (AssessmentConfiguration, TextQuestionConfiguration)
- **Authorization pattern**: Per CLAUDE.md Section 10 (AuthorizeView, multi-layer checks)
- **Translation system**: Per CLAUDE.md guidelines (test-translations.json)
- **Controller response**: Per CLAUDE.md Section 1 (CreateResponse, not Ok/BadRequest)
- **Logging pattern**: Per CLAUDE.md Section 2 (compile-time LoggerMessageAttribute)

### Related Files
- **Plan**: `C:\Users\zud\.claude\plans\buzzing-moseying-beacon.md`
- **Main Todo**: `C:\projects\BlazorRadzenTest\Todo\Todo.txt`

---

**Last Updated**: 2026-01-05
**Status**: 🔴 Ready to start implementation
