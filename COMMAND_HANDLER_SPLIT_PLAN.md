# Command Handler Split - Detailed Plan

**Status:** 📋 PLANNING
**Estimated Effort:** 1 day (6-8 hours)
**Risk Level:** Medium - High traffic handlers, requires careful testing

---

## Current State

**File:** `02_Application/ti8m.BeachBreak.Application.Command/Commands/QuestionnaireAssignmentCommands/QuestionnaireAssignmentCommandHandler.cs`

- **Lines of Code:** 468 lines
- **Handlers:** 16 different command handlers in one class
- **Violations:** Single Responsibility Principle
- **Maintainability:** Low (hard to test, understand, and modify)

---

## Goal

Split the monolithic handler into **16 separate, focused handler classes**, each responsible for a single command.

### Benefits After Completion

✅ **Single Responsibility** - Each handler does one thing
✅ **Testability** - Easy to unit test individual handlers
✅ **Maintainability** - Clear, focused classes
✅ **Team Scalability** - Multiple developers can work on different handlers
✅ **Code Navigation** - Easy to find specific handler logic

---

## Phase 1: Preparation (30 minutes)

### Step 1.1: Create Backup
```bash
cp QuestionnaireAssignmentCommandHandler.cs QuestionnaireAssignmentCommandHandler.cs.backup
git add .
git commit -m "Backup before command handler split"
```

### Step 1.2: Verify All Tests Pass
```bash
dotnet test --filter "FullyQualifiedName~QuestionnaireAssignment"
```

### Step 1.3: Document Current DI Registration
Check where handlers are registered:
- File: `03_Infrastructure/ti8m.BeachBreak.CommandApi/Program.cs` (or DI configuration)
- Current registration pattern

---

## Phase 2: Handler Inventory (15 minutes)

### 16 Handlers to Extract

| # | Handler Class Name | Command | Lines | Complexity | Priority |
|---|-------------------|---------|-------|------------|----------|
| 1 | `CreateBulkAssignmentsCommandHandler` | CreateBulkAssignmentsCommand | ~40 | Medium | P2 |
| 2 | `StartAssignmentWorkCommandHandler` | StartAssignmentWorkCommand | ~20 | Low | P3 |
| 3 | `CompleteAssignmentWorkCommandHandler` | CompleteAssignmentWorkCommand | ~20 | Low | P3 |
| 4 | `ExtendAssignmentDueDateCommandHandler` | ExtendAssignmentDueDateCommand | ~20 | Low | P3 |
| 5 | `WithdrawAssignmentCommandHandler` | WithdrawAssignmentCommand | ~20 | Low | P2 |
| 6 | `CompleteSectionAsEmployeeCommandHandler` | CompleteSectionAsEmployeeCommand | ~25 | Medium | P1 |
| 7 | `CompleteBulkSectionsAsEmployeeCommandHandler` | CompleteBulkSectionsAsEmployeeCommand | ~20 | Low | P3 |
| 8 | `CompleteSectionAsManagerCommandHandler` | CompleteSectionAsManagerCommand | ~25 | Medium | P1 |
| 9 | `CompleteBulkSectionsAsManagerCommandHandler` | CompleteBulkSectionsAsManagerCommand | ~20 | Low | P3 |
| 10 | `SubmitEmployeeQuestionnaireCommandHandler` | SubmitEmployeeQuestionnaireCommand | ~25 | Medium | P1 |
| 11 | `SubmitManagerQuestionnaireCommandHandler` | SubmitManagerQuestionnaireCommand | ~25 | Medium | P1 |
| 12 | `InitiateReviewCommandHandler` | InitiateReviewCommand | ~20 | Low | P2 |
| 13 | `EditAnswerDuringReviewCommandHandler` | EditAnswerDuringReviewCommand | ~80 | **High** | P1 |
| 14 | `FinishReviewMeetingCommandHandler` | FinishReviewMeetingCommand | ~30 | Medium | P2 |
| 15 | `ConfirmReviewOutcomeAsEmployeeCommandHandler` | ConfirmReviewOutcomeAsEmployeeCommand | ~30 | Medium | P2 |
| 16 | `FinalizeQuestionnaireAsManagerCommandHandler` | FinalizeQuestionnaireAsManagerCommand | ~25 | Medium | P2 |

**Priority Levels:**
- **P1 (Critical Path)** - Core workflow handlers (6 handlers)
- **P2 (Important)** - Supporting workflow handlers (6 handlers)
- **P3 (Nice to Have)** - Utility/bulk handlers (4 handlers)

---

## Phase 3: Extraction Strategy (Choose One)

### Option A: Big Bang Approach ⚠️ (NOT RECOMMENDED)
- Extract all 16 handlers at once
- High risk, long deployment window
- Difficult to debug if issues arise

### Option B: Incremental Approach ✅ (RECOMMENDED)
- Extract handlers in 4 batches by priority
- Test and deploy after each batch
- Lower risk, easier rollback

### Option C: Strangler Fig Pattern 🌿 (SAFEST)
- Keep old handler, create new handlers alongside
- Route commands to new handlers one at a time
- Delete old handler only when all commands migrated
- Slowest but safest for production

**Recommended:** **Option B - Incremental Approach**

---

## Phase 4: Implementation Plan - Incremental (4-6 hours)

### Batch 1: Core Workflow Handlers (P1) - 2 hours

Extract the 6 most critical handlers first:

1. **CompleteSectionAsEmployeeCommandHandler** (lines 163-185)
2. **CompleteSectionAsManagerCommandHandler** (lines 204-226)
3. **SubmitEmployeeQuestionnaireCommandHandler** (lines 245-267)
4. **SubmitManagerQuestionnaireCommandHandler** (lines 268-290)
5. **EditAnswerDuringReviewCommandHandler** (lines 311-390) ⚠️ **Most Complex**
6. **InitiateReviewCommandHandler** (lines 291-310)

**Steps for Each Handler:**

#### Step 4.1: Create New Handler File
```csharp
// File: CompleteSectionAsEmployeeCommandHandler.cs
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class CompleteSectionAsEmployeeCommandHandler
    : ICommandHandler<CompleteSectionAsEmployeeCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly IQuestionnaireResponseAggregateRepository responseRepository;
    private readonly ILogger<CompleteSectionAsEmployeeCommandHandler> logger;

    public CompleteSectionAsEmployeeCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        IQuestionnaireResponseAggregateRepository responseRepository,
        ILogger<CompleteSectionAsEmployeeCommandHandler> logger)
    {
        this.repository = repository;
        this.responseRepository = responseRepository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(
        CompleteSectionAsEmployeeCommand command,
        CancellationToken cancellationToken = default)
    {
        // [COPY LOGIC FROM ORIGINAL HANDLER - lines 163-185]
    }
}
```

#### Step 4.2: Copy Handler Logic
- Copy the exact logic from the original handler method
- Preserve all logging statements
- Keep the same error handling

#### Step 4.3: Update DI Registration
```csharp
// In Program.cs or DI configuration
services.AddScoped<ICommandHandler<CompleteSectionAsEmployeeCommand, Result>,
                   CompleteSectionAsEmployeeCommandHandler>();
```

#### Step 4.4: Remove from Original Handler
- Delete the interface implementation from the class signature
- Delete the HandleAsync method
- DO NOT delete shared repositories yet

#### Step 4.5: Build and Test
```bash
dotnet build
dotnet test --filter "FullyQualifiedName~CompleteSectionAsEmployee"
```

#### Step 4.6: Commit
```bash
git add .
git commit -m "Extract CompleteSectionAsEmployeeCommandHandler"
```

**Repeat for all 6 handlers in Batch 1**

---

### Batch 2: Supporting Workflow Handlers (P2) - 1.5 hours

Extract 6 supporting handlers:

7. **WithdrawAssignmentCommandHandler** (lines 143-162)
8. **InitiateReviewCommandHandler** (lines 291-310)
9. **FinishReviewMeetingCommandHandler** (lines 391-413)
10. **ConfirmReviewOutcomeAsEmployeeCommandHandler** (lines 414-444)
11. **FinalizeQuestionnaireAsManagerCommandHandler** (lines 445-468)
12. **CreateBulkAssignmentsCommandHandler** (lines 39-81)

Follow same steps as Batch 1.

---

### Batch 3: Utility Handlers (P3) - 1 hour

Extract remaining 4 utility handlers:

13. **StartAssignmentWorkCommandHandler** (lines 82-101)
14. **CompleteAssignmentWorkCommandHandler** (lines 102-121)
15. **ExtendAssignmentDueDateCommandHandler** (lines 122-142)
16. **CompleteBulkSectionsAsEmployeeCommandHandler** (lines 186-203)
17. **CompleteBulkSectionsAsManagerCommandHandler** (lines 227-244)

Follow same steps as Batch 1.

---

### Batch 4: Cleanup (30 minutes)

#### Step 4.7: Delete Original Handler File
Once all 16 handlers are extracted and tested:

```bash
rm QuestionnaireAssignmentCommandHandler.cs
git add .
git commit -m "Remove monolithic QuestionnaireAssignmentCommandHandler - split complete"
```

#### Step 4.8: Update Documentation
- Update CLAUDE.md with new handler structure
- Update any architecture diagrams
- Update developer onboarding docs

#### Step 4.9: Final Verification
```bash
dotnet build
dotnet test
# Manual smoke test of questionnaire workflows
```

---

## Phase 5: DI Registration Pattern (15 minutes)

### Current Registration (Assumed)
```csharp
// Single registration for all 16 commands
services.AddScoped<QuestionnaireAssignmentCommandHandler>();
```

### New Registration (After Split)
```csharp
// Option A: Manual registration (explicit)
services.AddScoped<ICommandHandler<CompleteSectionAsEmployeeCommand, Result>,
                   CompleteSectionAsEmployeeCommandHandler>();
services.AddScoped<ICommandHandler<CompleteSectionAsManagerCommand, Result>,
                   CompleteSectionAsManagerCommandHandler>();
// ... repeat for all 16 handlers

// Option B: Convention-based registration (cleaner)
services.Scan(scan => scan
    .FromAssemblyOf<CompleteSectionAsEmployeeCommandHandler>()
    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());
```

**Recommendation:** Use **Option B** for cleaner DI configuration.

---

## Phase 6: Testing Strategy (1 hour)

### Unit Tests
For each extracted handler, create unit tests:

```csharp
// File: CompleteSectionAsEmployeeCommandHandlerTests.cs
public class CompleteSectionAsEmployeeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCommand_CompletesSection()
    {
        // Arrange
        var mockRepo = new Mock<IQuestionnaireAssignmentAggregateRepository>();
        var mockResponseRepo = new Mock<IQuestionnaireResponseAggregateRepository>();
        var mockLogger = new Mock<ILogger<CompleteSectionAsEmployeeCommandHandler>>();

        var handler = new CompleteSectionAsEmployeeCommandHandler(
            mockRepo.Object,
            mockResponseRepo.Object,
            mockLogger.Object);

        var command = new CompleteSectionAsEmployeeCommand(
            assignmentId: Guid.NewGuid(),
            sectionId: Guid.NewGuid(),
            expectedVersion: 1);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.Succeeded);
        mockRepo.Verify(r => r.SaveAsync(It.IsAny<QuestionnaireAssignment>(), default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AssignmentNotFound_ReturnsFailure()
    {
        // Test error cases
    }
}
```

### Integration Tests
Test the full command dispatch pipeline:

```csharp
[Fact]
public async Task CommandDispatcher_RoutesToCorrectHandler()
{
    // Verify DI correctly routes commands to new handlers
}
```

### Smoke Tests (Manual)
1. Create assignment
2. Complete employee section
3. Submit employee questionnaire
4. Complete manager section
5. Submit manager questionnaire
6. Initiate review
7. Edit answer during review
8. Finish review meeting
9. Confirm review outcome
10. Finalize questionnaire

---

## Phase 7: Rollout Strategy

### Development Environment
1. Extract Batch 1 → Test → Commit
2. Extract Batch 2 → Test → Commit
3. Extract Batch 3 → Test → Commit
4. Delete original handler → Test → Commit

### Staging Environment
1. Deploy with feature flag (if available)
2. Run smoke tests
3. Monitor logs for 24 hours

### Production Environment
1. Deploy during low-traffic window
2. Monitor error rates
3. Have rollback plan ready

---

## Phase 8: Rollback Plan

### If Issues Arise After Deployment

#### Option 1: Quick Rollback
```bash
git revert HEAD~4  # Revert last 4 commits (batches)
git push
# Redeploy previous version
```

#### Option 2: Partial Rollback
- Keep new handlers
- Re-add original monolithic handler temporarily
- Route problematic commands back to old handler
- Fix issues in new handlers
- Re-deploy

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| **DI registration errors** | Medium | High | Thorough testing, convention-based registration |
| **Logic differences after copy** | Low | High | Code review, comprehensive tests |
| **Performance degradation** | Very Low | Medium | Load testing, monitoring |
| **Missing dependencies** | Low | Medium | Build verification after each batch |
| **Breaking existing tests** | Medium | Low | Run tests after each extraction |

---

## Success Criteria

✅ All 16 handlers extracted into separate files
✅ Original 468-line file deleted
✅ All unit tests pass
✅ All integration tests pass
✅ Manual smoke tests pass
✅ Build succeeds with 0 errors
✅ No new warnings introduced
✅ Code coverage maintained or improved
✅ Performance metrics unchanged

---

## Estimated Timeline

| Phase | Duration | Cumulative |
|-------|----------|------------|
| **Preparation** | 30 min | 0.5 hrs |
| **Batch 1 (P1 - 6 handlers)** | 2 hrs | 2.5 hrs |
| **Batch 2 (P2 - 5 handlers)** | 1.5 hrs | 4 hrs |
| **Batch 3 (P3 - 5 handlers)** | 1 hr | 5 hrs |
| **Cleanup** | 30 min | 5.5 hrs |
| **Testing** | 1 hr | 6.5 hrs |
| **Documentation** | 30 min | 7 hrs |
| **Buffer (15%)** | 1 hr | **8 hrs** |

**Total Estimated Effort:** 1 full day (8 hours)

---

## Automation Script (Optional)

Create a script to automate the extraction:

```bash
#!/bin/bash
# extract_handler.sh

HANDLER_NAME=$1
START_LINE=$2
END_LINE=$3

# Extract handler method
sed -n "${START_LINE},${END_LINE}p" QuestionnaireAssignmentCommandHandler.cs > temp.txt

# Create new handler file
cat > "${HANDLER_NAME}.cs" <<EOF
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class ${HANDLER_NAME} : ICommandHandler<${COMMAND_NAME}, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly IQuestionnaireResponseAggregateRepository responseRepository;
    private readonly ILogger<${HANDLER_NAME}> logger;

    public ${HANDLER_NAME}(
        IQuestionnaireAssignmentAggregateRepository repository,
        IQuestionnaireResponseAggregateRepository responseRepository,
        ILogger<${HANDLER_NAME}> logger)
    {
        this.repository = repository;
        this.responseRepository = responseRepository;
        this.logger = logger;
    }

$(cat temp.txt)
}
EOF

rm temp.txt

echo "Created ${HANDLER_NAME}.cs"
```

---

## Post-Refactoring Benefits

### Before (Current)
```
QuestionnaireAssignmentCommandHandler.cs (468 lines)
├─ 16 different command handlers
├─ Hard to find specific logic
├─ Difficult to test in isolation
├─ Merge conflicts likely
└─ Violates SRP
```

### After (Target)
```
QuestionnaireAssignmentCommands/
├─ CompleteSectionAsEmployeeCommandHandler.cs (~30 lines)
├─ CompleteSectionAsManagerCommandHandler.cs (~30 lines)
├─ SubmitEmployeeQuestionnaireCommandHandler.cs (~30 lines)
├─ SubmitManagerQuestionnaireCommandHandler.cs (~30 lines)
├─ EditAnswerDuringReviewCommandHandler.cs (~80 lines)
├─ InitiateReviewCommandHandler.cs (~25 lines)
├─ ... (10 more handlers, each ~25-40 lines)
└─ [16 focused, testable handlers]
```

**Average Lines Per Handler:** ~29 lines (vs 468 lines)
**Maintainability Score:** Excellent
**Testability Score:** Excellent

---

## Next Steps

1. **Review this plan** with team
2. **Schedule the refactoring** (1 day time block)
3. **Create a feature branch** (`feature/split-command-handler`)
4. **Execute Batch 1** and validate
5. **Continue incrementally**
6. **Celebrate clean architecture!** 🎉

---

**Plan Created:** 2025-10-20
**Plan Author:** Claude Code
**Status:** Ready for Implementation
