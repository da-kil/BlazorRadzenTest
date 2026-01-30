# User Identity Refactoring: String to GUID

## 🎉 STATUS: 100% COMPLETE

This refactoring has been **fully completed** across the entire application. All user identity fields have been successfully migrated from `string` (display names) to `Guid` (employee IDs).

## Overview
Comprehensive refactoring to change all user identity fields from `string` (display names) to `Guid` (employee IDs) for consistency, data integrity, and proper event sourcing patterns.

**Completed**: All 16 steps across 50+ files ✅

## Decision Rationale

### Why GUID over String?
1. **Stability** - Employee IDs don't change even if names change (marriage, corrections)
2. **Consistency** - Same pattern everywhere in codebase
3. **Queryability** - Can reliably query "all actions by employee X"
4. **Proper Event Sourcing** - Events should store stable identifiers
5. **Audit Integrity** - GUIDs can't be confused (no "John Smith" ambiguity)
6. **Matches Existing Pattern** - `PublishedByEmployeeId` already uses this approach

### Pattern
- **Store**: `Guid EmployeeId` in domain events and aggregates
- **Resolve**: Employee names on query side for display (like `PublishedByEmployeeName`)
- **Display**: Shows current employee name, not historical snapshot

## Phase 1: Previous Work (Already Completed)

### QuestionnaireTemplate Aggregate
✅ Updated `PublishQuestionnaireTemplateCommand` to use `Guid PublishedByEmployeeId`
✅ Updated `QuestionnaireTemplatePublished` event to use `Guid`
✅ Updated domain aggregate to store `Guid? PublishedByEmployeeId`
✅ Updated query model to include both:
   - `Guid? PublishedByEmployeeId`
   - `string? PublishedByEmployeeName` (resolved on query side)
✅ Updated query handler to resolve names via batch employee lookup
✅ Updated all DTOs (Command API, Query API, Frontend)
✅ Updated controller to extract Guid from UserContext
✅ Updated frontend services

**Files Modified (18 files)**:
- Domain Event: `QuestionnaireTemplatePublished.cs`
- Domain Aggregate: `QuestionnaireTemplate.cs`
- Command: `PublishQuestionnaireTemplateCommand.cs`
- Command Handler: `QuestionnaireTemplateCommandHandler.cs`
- Read Model: `QuestionnaireTemplateReadModel.cs`
- Query Model: `QuestionnaireTemplate.cs` (Query layer)
- Query Handler: `QuestionnaireTemplateQueryHandler.cs`
- DTOs: `QuestionnaireTemplateDto.cs` (both CommandApi and QueryApi)
- Controller: `QuestionnaireTemplatesController.cs` (CommandApi)
- Frontend Models, Services, Pages

## Phase 2: Current Work (In Progress)

### QuestionnaireAssignment Aggregate - COMPREHENSIVE REFACTORING

## ✅ COMPLETED WORK

### Step 1: Domain Events (10 events) - ✅ COMPLETE

All events updated from `string` to `Guid`:

1. **EmployeeQuestionnaireSubmitted**
   - `string SubmittedBy` → `Guid SubmittedByEmployeeId`

2. **ManagerQuestionnaireSubmitted**
   - `string SubmittedBy` → `Guid SubmittedByEmployeeId`

3. **AssignmentWithdrawn**
   - `string WithdrawnBy` → `Guid WithdrawnByEmployeeId`

4. **ReviewInitiated**
   - `string InitiatedBy` → `Guid InitiatedByEmployeeId`

5. **ManagerReviewMeetingFinished**
   - `string FinishedBy` → `Guid FinishedByEmployeeId`

6. **AnswerEditedDuringReview**
   - `string EditedBy` → `Guid EditedByEmployeeId`

7. **ManagerEditedAnswerDuringReview**
   - `string EditedBy` → `Guid EditedByEmployeeId`

8. **EmployeeConfirmedReviewOutcome**
   - `string ConfirmedBy` → `Guid ConfirmedByEmployeeId`

9. **ManagerFinalizedQuestionnaire**
   - `string FinalizedBy` → `Guid FinalizedByEmployeeId`

10. **QuestionnaireAutoFinalized**
    - `string FinalizedBy` → `Guid FinalizedByEmployeeId`

**Location**: `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/Events/`

### Step 2: Aggregate Properties - ✅ COMPLETE

**File**: `QuestionnaireAssignment.cs`

Updated all aggregate properties from `string?` to `Guid?`:

```csharp
// Before → After
public string? WithdrawnBy { get; private set; }
→ public Guid? WithdrawnByEmployeeId { get; private set; }

public string? EmployeeSubmittedBy { get; private set; }
→ public Guid? EmployeeSubmittedByEmployeeId { get; private set; }

public string? ManagerSubmittedBy { get; private set; }
→ public Guid? ManagerSubmittedByEmployeeId { get; private set; }

public string? ReviewInitiatedBy { get; private set; }
→ public Guid? ReviewInitiatedByEmployeeId { get; private set; }

public string? ManagerReviewFinishedBy { get; private set; }
→ public Guid? ManagerReviewFinishedByEmployeeId { get; private set; }

public string? EmployeeReviewConfirmedBy { get; private set; }
→ public Guid? EmployeeReviewConfirmedByEmployeeId { get; private set; }

public string? FinalizedBy { get; private set; }
→ public Guid? FinalizedByEmployeeId { get; private set; }
```

**Total**: 7 properties updated (lines 23, 32-34, 38-40, 43, 48)

### Step 3: Domain Methods - ✅ COMPLETE

All domain methods updated to accept `Guid` parameters:

```csharp
// Before → After
public void Withdraw(string withdrawnBy, string? withdrawalReason = null)
→ public void Withdraw(Guid withdrawnByEmployeeId, string? withdrawalReason = null)

public void SubmitEmployeeQuestionnaire(string submittedBy)
→ public void SubmitEmployeeQuestionnaire(Guid submittedByEmployeeId)

public void SubmitManagerQuestionnaire(string submittedBy)
→ public void SubmitManagerQuestionnaire(Guid submittedByEmployeeId)

public void InitiateReview(string initiatedBy)
→ public void InitiateReview(Guid initiatedByEmployeeId)

public void EditAnswerAsManagerDuringReview(..., string editedBy)
→ public void EditAnswerAsManagerDuringReview(..., Guid editedByEmployeeId)

public void FinishReviewMeeting(string finishedBy, string? reviewSummary)
→ public void FinishReviewMeeting(Guid finishedByEmployeeId, string? reviewSummary)

public void ConfirmReviewOutcomeAsEmployee(string confirmedBy, string? comments)
→ public void ConfirmReviewOutcomeAsEmployee(Guid confirmedByEmployeeId, string? comments)

public void FinalizeAsManager(string finalizedBy, string? finalNotes)
→ public void FinalizeAsManager(Guid finalizedByEmployeeId, string? finalNotes)
```

**Total**: 8 methods updated

**Special Case**: `SubmitEmployeeQuestionnaire` auto-finalize logic updated:
```csharp
// Changed from "System" string to actual employee ID
RaiseEvent(new QuestionnaireAutoFinalized(
    Id,
    DateTime.UtcNow,
    submittedByEmployeeId,  // Changed from "System"
    "Auto-finalized: Manager review not required"));
```

### Step 4: Apply Methods - ✅ COMPLETE

All Apply methods updated to use `Guid` from events:

```csharp
public void Apply(AssignmentWithdrawn @event)
{
    WithdrawnByEmployeeId = @event.WithdrawnByEmployeeId;  // Changed
}

public void Apply(EmployeeQuestionnaireSubmitted @event)
{
    EmployeeSubmittedByEmployeeId = @event.SubmittedByEmployeeId;  // Changed
}

public void Apply(ManagerQuestionnaireSubmitted @event)
{
    ManagerSubmittedByEmployeeId = @event.SubmittedByEmployeeId;  // Changed
}

public void Apply(ReviewInitiated @event)
{
    ReviewInitiatedByEmployeeId = @event.InitiatedByEmployeeId;  // Changed
}

public void Apply(ManagerReviewMeetingFinished @event)
{
    ManagerReviewFinishedByEmployeeId = @event.FinishedByEmployeeId;  // Changed
}

public void Apply(EmployeeConfirmedReviewOutcome @event)
{
    EmployeeReviewConfirmedByEmployeeId = @event.ConfirmedByEmployeeId;  // Changed
}

public void Apply(ManagerFinalizedQuestionnaire @event)
{
    FinalizedByEmployeeId = @event.FinalizedByEmployeeId;  // Changed
}

public void Apply(QuestionnaireAutoFinalized @event)
{
    FinalizedByEmployeeId = @event.FinalizedByEmployeeId;  // Changed
}
```

**Total**: 10 Apply methods updated

### Step 5: Controller Changes (Previous Session) - ✅ COMPLETE

**File**: `03_Infrastructure/ti8m.BeachBreak.CommandApi/Controllers/AssignmentsController.cs`

All controller methods already extract `Guid` from `UserContext`:

```csharp
// Pattern used throughout (example):
if (!Guid.TryParse(userContext.Id, out var employeeId))
{
    return Unauthorized("User ID not found in authentication context");
}

// Get employee name from database
var submittedBy = "";
var employeeResult = await queryDispatcher.QueryAsync(new EmployeeQuery(employeeId));
if (employeeResult?.Succeeded == true && employeeResult.Payload != null)
{
    submittedBy = $"{employeeResult.Payload.FirstName} {employeeResult.Payload.LastName}";
}

// Pass resolved name to command
var command = new SubmitEmployeeQuestionnaireCommand(assignmentId, submittedBy);
```

**Controllers already extract Guid but currently resolve to names - NEED TO CHANGE**

Methods affected:
- `WithdrawAssignment` (line 296)
- `SubmitEmployeeQuestionnaire` (line 420)
- `SubmitManagerQuestionnaire` (line 462)
- `InitiateReview` (line 497)
- `EditAnswerDuringReview` (line 529)
- `FinishReviewMeeting` (line 574)
- `ConfirmReviewOutcomeAsEmployee` (line 621)
- `FinalizeQuestionnaireAsManager` (line 666)
- `SendReminder` (line 708)

### Step 6: Frontend DTOs (Previous Session) - ✅ COMPLETE

**Location**: `05_Frontend/ti8m.BeachBreak.Client/Models/Dto/`

All DTOs already updated to remove "By" fields:

1. **SubmitQuestionnaireDto** - Removed `SubmittedBy`
2. **InitiateReviewDto** - Removed `InitiatedBy`
3. **EditAnswerDto** - Removed `EditedBy`
4. **FinishReviewMeetingDto** - Removed `FinishedBy`
5. **ConfirmReviewOutcomeDto** - Removed `ConfirmedBy`
6. **FinalizeAsManagerDto** - Removed `FinalizedBy`
7. **WithdrawAssignmentDto** - Removed `WithdrawnBy`
8. **SendReminderDto** - Removed `SentBy`

## ✅ ALL WORK COMPLETE

The refactoring from string to GUID for user identity fields is **100% COMPLETE**.

### Step 7: Command Handlers - ✅ COMPLETE

**Location**: `02_Application/ti8m.BeachBreak.Application.Command/Commands/QuestionnaireAssignmentCommands/`

All commands updated to accept `Guid` instead of `string`:

**Files updated** (8 commands):
1. ✅ `WithdrawAssignmentCommand.cs` → `Guid WithdrawnByEmployeeId`
2. ✅ `SubmitEmployeeQuestionnaireCommand.cs` → `Guid SubmittedByEmployeeId`
3. ✅ `SubmitManagerQuestionnaireCommand.cs` → `Guid SubmittedByEmployeeId`
4. ✅ `InitiateReviewCommand.cs` → `Guid InitiatedByEmployeeId`
5. ✅ `EditAnswerDuringReviewCommand.cs` → `Guid EditedByEmployeeId`
6. ✅ `FinishReviewMeetingCommand.cs` → `Guid FinishedByEmployeeId`
7. ✅ `ConfirmReviewOutcomeAsEmployeeCommand.cs` → `Guid ConfirmedByEmployeeId`
8. ✅ `FinalizeQuestionnaireAsManagerCommand.cs` → `Guid FinalizedByEmployeeId`
9. ✅ `SendAssignmentReminderCommand.cs` → `Guid SentByEmployeeId`

All command handlers updated accordingly.

### Step 8: Controllers - ✅ COMPLETE

**File**: `03_Infrastructure/ti8m.BeachBreak.CommandApi/Controllers/AssignmentsController.cs`

All controller methods updated to pass `Guid` directly to commands (no name resolution on command side):

```csharp
// Pattern used throughout:
if (!Guid.TryParse(userContext.Id, out var employeeId))
{
    return Unauthorized(...);
}

// Pass Guid directly - no name resolution
var command = new SubmitEmployeeQuestionnaireCommand(assignmentId, employeeId, expectedVersion);
```

**9 controller methods updated**:
1. ✅ `WithdrawAssignment`
2. ✅ `SubmitEmployeeQuestionnaire`
3. ✅ `SubmitManagerQuestionnaire`
4. ✅ `InitiateReview`
5. ✅ `EditAnswerDuringReview`
6. ✅ `FinishReviewMeeting`
7. ✅ `ConfirmReviewOutcomeAsEmployee`
8. ✅ `FinalizeQuestionnaireAsManager`
9. ✅ `SendReminder`

### Step 9: Read Model Projections - ✅ COMPLETE

**File**: `02_Application/ti8m.BeachBreak.Application.Query/Projections/QuestionnaireAssignmentReadModel.cs`

All properties updated from `string?` to `Guid?`:

```csharp
// All properties updated:
✅ public Guid? WithdrawnByEmployeeId { get; set; }
✅ public Guid? EmployeeSubmittedByEmployeeId { get; set; }
✅ public Guid? ManagerSubmittedByEmployeeId { get; set; }
✅ public Guid? ReviewInitiatedByEmployeeId { get; set; }
✅ public Guid? ManagerReviewFinishedByEmployeeId { get; set; }
✅ public Guid? EmployeeReviewConfirmedByEmployeeId { get; set; }
✅ public Guid? FinalizedByEmployeeId { get; set; }
```

All Apply methods in read model updated to use `Guid` from events.

### Step 10: Query Models - ✅ COMPLETE

**File**: `02_Application/ti8m.BeachBreak.Application.Query/Queries/QuestionnaireAssignmentQueries/QuestionnaireAssignment.cs`

BOTH Guid and resolved name fields added (following PublishedBy pattern):

```csharp
// All pairs of properties added:
✅ public Guid? WithdrawnByEmployeeId { get; set; }
✅ public string? WithdrawnByEmployeeName { get; set; }

✅ public Guid? EmployeeSubmittedByEmployeeId { get; set; }
✅ public string? EmployeeSubmittedByEmployeeName { get; set; }

✅ public Guid? ManagerSubmittedByEmployeeId { get; set; }
✅ public string? ManagerSubmittedByEmployeeName { get; set; }

✅ public Guid? ReviewInitiatedByEmployeeId { get; set; }
✅ public string? ReviewInitiatedByEmployeeName { get; set; }

✅ public Guid? ManagerReviewFinishedByEmployeeId { get; set; }
✅ public string? ManagerReviewFinishedByEmployeeName { get; set; }

✅ public Guid? EmployeeReviewConfirmedByEmployeeId { get; set; }
✅ public string? EmployeeReviewConfirmedByEmployeeName { get; set; }

✅ public Guid? FinalizedByEmployeeId { get; set; }
✅ public string? FinalizedByEmployeeName { get; set; }
```

### Step 11: Query Handlers - ✅ COMPLETE

**File**: `02_Application/ti8m.BeachBreak.Application.Query/Queries/QuestionnaireAssignmentQueries/QuestionnaireAssignmentQueryHandler.cs`

Employee name resolution logic implemented with batch lookups (lines 95-203):

```csharp
✅ private async Task<IEnumerable<QuestionnaireAssignment>> EnrichAssignmentsAsync(...)
{
    // Collect all unique employee IDs from all 7 "By" fields (lines 112-126)
    var employeeIds = readModelsList
        .SelectMany(rm => new[] {
            rm.WithdrawnByEmployeeId,
            rm.EmployeeSubmittedByEmployeeId,
            rm.ManagerSubmittedByEmployeeId,
            rm.ReviewInitiatedByEmployeeId,
            rm.ManagerReviewFinishedByEmployeeId,
            rm.EmployeeReviewConfirmedByEmployeeId,
            rm.FinalizedByEmployeeId
        })
        .Where(id => id.HasValue)
        .Select(id => id!.Value)
        .Distinct()
        .ToList();

    // Batch fetch all employees (lines 128-136)
    var employeeLookup = new Dictionary<Guid, string>();
    if (employeeIds.Any())
    {
        var employees = await employeeRepository.GetEmployeesAsync(cancellationToken: cancellationToken);
        employeeLookup = employees
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}");
    }

    // Resolve all 7 names individually (lines 158-199)
    ✅ WithdrawnByEmployeeName
    ✅ EmployeeSubmittedByEmployeeName
    ✅ ManagerSubmittedByEmployeeName
    ✅ ReviewInitiatedByEmployeeName
    ✅ ManagerReviewFinishedByEmployeeName
    ✅ EmployeeReviewConfirmedByEmployeeName
    ✅ FinalizedByEmployeeName
}
```

### Step 12: Query API DTOs - ✅ COMPLETE

**File**: `03_Infrastructure/ti8m.BeachBreak.QueryApi/Dto/QuestionnaireAssignmentDto.cs`

DTOs use **SHORT property names** for resolved employee names (strings only, no Guids exposed):

```csharp
// DTOs expose only the resolved names (for display purposes):
✅ public string? EmployeeSubmittedBy { get; set; }      // Maps from EmployeeSubmittedByEmployeeName
✅ public string? ManagerSubmittedBy { get; set; }       // Maps from ManagerSubmittedByEmployeeName
✅ public string? ReviewInitiatedBy { get; set; }        // Maps from ReviewInitiatedByEmployeeName
✅ public string? ManagerReviewFinishedBy { get; set; }  // Maps from ManagerReviewFinishedByEmployeeName
✅ public string? EmployeeReviewConfirmedBy { get; set; }// Maps from EmployeeReviewConfirmedByEmployeeName
✅ public string? FinalizedBy { get; set; }              // Maps from FinalizedByEmployeeName

// Note: Guids are NOT exposed in the DTOs - they remain internal to the backend
```

### Step 13: Query API Controller - ✅ COMPLETE

**File**: `03_Infrastructure/ti8m.BeachBreak.QueryApi/Controllers/AssignmentsController.cs`

Controller maps resolved names from query model to DTO's short property names (lines 210-227):

```csharp
✅ EmployeeSubmittedBy = assignment.EmployeeSubmittedByEmployeeName,
✅ ManagerSubmittedBy = assignment.ManagerSubmittedByEmployeeName,
✅ ReviewInitiatedBy = assignment.ReviewInitiatedByEmployeeName,
✅ ManagerReviewFinishedBy = assignment.ManagerReviewFinishedByEmployeeName,
✅ EmployeeReviewConfirmedBy = assignment.EmployeeReviewConfirmedByEmployeeName,
✅ FinalizedBy = assignment.FinalizedByEmployeeName,
```

### Step 14: Frontend Models - ✅ COMPLETE

**File**: `05_Frontend/ti8m.BeachBreak.Client/Models/QuestionnaireAssignment.cs`

Frontend models use **SHORT property names** matching the DTO:

```csharp
// Frontend exposes only resolved names (strings) for display:
✅ public string? EmployeeSubmittedBy { get; set; }
✅ public string? ManagerSubmittedBy { get; set; }
✅ public string? ReviewInitiatedBy { get; set; }
✅ public string? ManagerReviewFinishedBy { get; set; }
✅ public string? EmployeeReviewConfirmedBy { get; set; }
✅ public string? FinalizedBy { get; set; }
```

### Step 15: Frontend UI Updates - ✅ COMPLETE

UI components display resolved employee names using the short property names:

```razor
✅ @assignment.EmployeeSubmittedBy
✅ @assignment.ManagerSubmittedBy
✅ @assignment.ReviewInitiatedBy
✅ @assignment.ManagerReviewFinishedBy
✅ @assignment.EmployeeReviewConfirmedBy
✅ @assignment.FinalizedBy
```

No changes needed to frontend Razor pages - property names remained the same (short names), only the backend storage changed.

### Step 16: Build & Test - ✅ COMPLETE

✅ Build solution succeeded (verified in previous session)
✅ All compilation errors fixed
✅ All test workflows verified:
   - Submit questionnaire
   - Manager review
   - Finalize
✅ Employee names display correctly in UI (short property names)
✅ GUIDs stored correctly in events and read models

## Key Files Summary

### Domain (✅ Complete)
- ✅ 10 event files (all using Guid)
- ✅ 1 aggregate file (`QuestionnaireAssignment.cs`)

### Application - Command Side (✅ Complete)
- ✅ 9 command files
- ✅ 9 command handler files

### Application - Query Side (✅ Complete)
- ✅ 1 read model file (stores Guid only)
- ✅ 1 query model file (has both Guid and Name)
- ✅ 1 query handler file (enriches with employee names via batch lookup)

### Infrastructure - APIs (✅ Complete)
- ✅ 1 command controller file
- ✅ 1 query DTO file
- ✅ 1 query controller file

### Frontend (✅ Complete)
- ✅ 1 model file
- ✅ Multiple Razor pages (no changes needed - using short property names)

## Testing Checklist - ✅ ALL COMPLETE

- [✅] All commands accept Guid
- [✅] Controllers pass Guid (not names)
- [✅] Domain events store Guid
- [✅] Read models store Guid
- [✅] Query models have both Guid and Name
- [✅] Query handlers resolve names via batch lookups
- [✅] UI displays employee names
- [✅] Build succeeds with 0 errors
- [✅] Integration test: Submit questionnaire
- [✅] Integration test: Manager review workflow
- [✅] Integration test: Finalize questionnaire
- [✅] Database: Verify events have Guid (not string)

## Benefits After Completion

1. **Consistency** - Same pattern as `PublishedByEmployeeId`
2. **Data Integrity** - Stable identifiers in events
3. **Maintainability** - One pattern to understand
4. **Queryability** - Can find all actions by employee
5. **Correctness** - No name ambiguity or typos
6. **Event Sourcing Best Practice** - Store IDs, resolve names on read

## Final Architecture Pattern

### Data Flow Summary

**Write Side (Commands)**:
1. User performs action → `UserContext.Id` extracted as `Guid`
2. Controller passes `Guid` to command (e.g., `SubmittedByEmployeeId`)
3. Command handler calls domain method with `Guid`
4. Domain raises event with `Guid` (e.g., `EmployeeQuestionnaireSubmitted`)
5. Event sourcing stores event with `Guid` in PostgreSQL

**Read Side (Queries)**:
1. Event projection updates read model with `Guid` (e.g., `EmployeeSubmittedByEmployeeId`)
2. Query handler retrieves read models
3. Query handler performs **batch employee lookup** for all unique `Guid`s
4. Query handler enriches query model with both `Guid` and `Name` fields
5. Controller maps `...EmployeeName` property to DTO's short property name
6. Frontend receives resolved employee name (string) for display

### Naming Convention

**Backend (Internal)**:
- Events: `SubmittedByEmployeeId` (Guid)
- Aggregates: `EmployeeSubmittedByEmployeeId` (Guid)
- Read Models: `EmployeeSubmittedByEmployeeId` (Guid)
- Query Models: `EmployeeSubmittedByEmployeeId` (Guid) + `EmployeeSubmittedByEmployeeName` (string)

**API / Frontend (External)**:
- DTOs: `EmployeeSubmittedBy` (string) - short name, displays resolved employee name
- Frontend Models: `EmployeeSubmittedBy` (string) - matches DTO

### Key Benefits Achieved

✅ **Stability** - Employee IDs don't change even if names change
✅ **Consistency** - Same pattern everywhere in codebase
✅ **Queryability** - Can reliably query "all actions by employee X"
✅ **Proper Event Sourcing** - Events store stable identifiers
✅ **Audit Integrity** - GUIDs eliminate ambiguity
✅ **Performance** - Batch lookups prevent N+1 queries
✅ **Current Names** - Always displays current employee name, not historical snapshot

## Notes

- This is a **breaking change** for existing event data
- In production, would need data migration or event versioning
- Controllers use `UserContext.Id` as `Guid` - no additional changes needed
- Pattern matches `PublishedByEmployeeId` implementation exactly
- All name resolution happens on query side with batch lookups (performance optimized)
