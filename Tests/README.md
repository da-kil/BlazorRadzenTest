# Test Infrastructure Status

## Current Status (Phase 5 - Testing)

### Unit Tests Updated ✅
The unit test file `ti8m.BeachBreak.Domain.Tests/WorkflowStateMachineTests.cs` has been updated to include tests for the new **Initialized** workflow state:

**New Test Cases Added:**
- `Assigned → Initialized` transition (manager completes initialization)
- `Initialized → EmployeeInProgress` transition (employee starts working)
- `Initialized → ManagerInProgress` transition (manager starts working)
- `Initialized → BothInProgress` transition (both start working)
- Invalid transitions from `Assigned` directly to in-progress states (must initialize first)
- Invalid backwards transition from `Initialized` to `Assigned`
- `Initialized` state marked as non-reopenable
- Updated `GetValidNextStates()` tests: `Assigned` now has 1 valid next state (Initialized only)
- Updated `DetermineProgressState()` tests: Default state is now `Initialized` (not `Assigned`)

**Test Coverage:**
- ✅ Forward transition validation
- ✅ Invalid transition detection
- ✅ State counting logic
- ✅ Non-reopenable state validation
- ✅ Auto-transition logic with Initialized as starting point

### Test Infrastructure Setup Required ⚠️

**Issue**: The test files exist but are not integrated into the build system:
- No `.csproj` file exists for the test project
- Tests are not referenced in `ti8m.BeachBreak.sln`
- Cannot run tests via `dotnet test`

**To Set Up Test Infrastructure:**

1. **Create Test Project File:**
   ```bash
   cd Tests/ti8m.BeachBreak.Domain.Tests
   dotnet new xunit
   ```

2. **Add Domain Project Reference:**
   ```bash
   dotnet add reference ../../01_Domain/ti8m.BeachBreak.Domain/ti8m.BeachBreak.Domain.csproj
   ```

3. **Add Test Project to Solution:**
   ```bash
   dotnet sln ../../ti8m.BeachBreak.sln add ti8m.BeachBreak.Domain.Tests.csproj
   ```

4. **Run Tests:**
   ```bash
   dotnet test ../../ti8m.BeachBreak.sln
   ```

### Integration Tests Pending

The following integration tests should be added once test infrastructure is set up:

**InitializeAssignment Command Tests:**
- ✅ Test Assigned → Initialized transition
- ✅ Test initialization notes are stored
- ✅ Test custom sections are added during initialization
- ✅ Test authorization (only managers/HR can initialize)
- ✅ Test validation (can only initialize from Assigned state)

**AddCustomSections Command Tests:**
- ✅ Test custom sections with `IsInstanceSpecific = true`
- ✅ Test Assessment question configuration
- ✅ Test TextQuestion configuration
- ✅ Test validation (only in Assigned state)
- ✅ Test Goal questions are rejected

### Manual Testing Checklist

Until automated tests are configured, perform manual E2E testing:

#### Initialization Flow
- [ ] Manager creates assignment (state: Assigned)
- [ ] Manager sees "Initialize Assignment" button
- [ ] Employee does NOT see assignment in MyQuestionnaires
- [ ] Manager clicks "Initialize Assignment" button
- [ ] Manager navigates to initialization page (`/assignments/{id}/initialize`)
- [ ] Manager can optionally link predecessor questionnaire
- [ ] Manager can optionally add custom Assessment questions
- [ ] Manager can optionally add custom TextQuestion sections
- [ ] Manager cannot add Goal questions (validation message shown)
- [ ] Manager can add initialization notes (max 5000 chars)
- [ ] Manager clicks "Complete Initialization"
- [ ] Assignment transitions to Initialized state
- [ ] "Initialize Assignment" button disappears

#### Post-Initialization Flow
- [ ] Employee now sees assignment in MyQuestionnaires
- [ ] Assignment shows "Initialized" state with info icon
- [ ] Employee can click assignment and start working
- [ ] Custom questions appear seamlessly with template questions
- [ ] No visual distinction between template and custom sections
- [ ] Both employee and manager can complete sections
- [ ] Normal workflow continues: InProgress → Submitted → Review → Finalized

#### Translation Verification
- [ ] Switch language to German
- [ ] All 46 new translation keys display correctly
- [ ] German umlauts render properly (ü, ä, ö, ß)
- [ ] Workflow state badge shows "Initialisiert"
- [ ] Action labels show German translations

#### Authorization Tests
- [ ] Employee cannot access `/assignments/{id}/initialize` (403 Access Denied)
- [ ] TeamLead can access initialization page
- [ ] HR can access initialization page
- [ ] Admin can access initialization page

#### Edge Cases
- [ ] Try to initialize already-initialized assignment (validation error)
- [ ] Try to add custom sections after initialization (validation error)
- [ ] Try to initialize with >5000 char notes (validation error)
- [ ] Database restart preserves Initialized state (event sourcing)

### Test Status Summary

| Test Type | Status | Notes |
|-----------|--------|-------|
| Unit Tests (Domain) | ✅ Updated | WorkflowStateMachineTests.cs updated with Initialized state |
| Test Infrastructure | ⚠️ Missing | No .csproj, not in solution |
| Integration Tests | ⏸️ Pending | Awaiting test infrastructure setup |
| Frontend Tests | ⏸️ Pending | Awaiting test infrastructure setup |
| Manual E2E Tests | ⏸️ Pending | Checklist provided above |

### Recommendation

**Option 1: Set Up Test Infrastructure Now**
- Create test project files
- Add to solution
- Run updated unit tests
- Add integration tests

**Option 2: Defer Testing to Later**
- Proceed with Phase 6 (Documentation)
- Add test infrastructure as separate task
- Rely on manual testing for now

**Option 3: Hybrid Approach**
- Complete Phase 6 (Documentation)
- Set up test infrastructure
- Return to run automated tests before merging to main

### References

- Test File: `Tests/ti8m.BeachBreak.Domain.Tests/WorkflowStateMachineTests.cs`
- Implementation Plan: `Todo/AddNewWorkflowState.md`
- CLAUDE.md: No specific testing patterns documented

---

**Last Updated**: 2026-01-06 (Phase 5 - Testing)
