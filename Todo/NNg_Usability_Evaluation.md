# NN/g Usability Evaluation - ti8m BeachBreak
## Performance Management Platform

**Evaluation Date:** October 23, 2025
**Evaluator:** Based on NN/g Heuristics & User Context
**User Base:** 600 employees (age 18-65, skill level: user to expert)
**Primary Use Case:** Annual performance reviews (once/year) + ad-hoc questionnaires

---

## Executive Summary

### Strengths ✅
- Clean, modern visual design with good use of color
- Clear navigation structure with well-organized sections
- Good progress indicators and status badges
- Proper role-based access (Employee/Manager/HR)

### Critical Issues 🚨
1. **Missing dialog buttons** (EditAnswerDialog) - Prevents task completion
2. **Confusing banner text** in review mode - Unclear who is who
3. **Comment timing unclear** - Users don't understand when comments are created
4. **Inconsistent terminology** - "Categories" vs sections vs questions

### Priority Recommendations
1. Fix technical blockers (missing buttons)
2. Improve contextual clarity (who, when, why)
3. Reduce cognitive load through better information architecture
4. Add progressive disclosure for complex workflows

---

## Detailed Heuristic Evaluation

### 1. Visibility of System Status
*"The design should always keep users informed about what is going on"*

#### Screenshot 1: HR Dashboard
✅ **GOOD:**
- Clear metrics at top (200 employees, 1 manager, 3 assignments, 33.3% completion)
- Status breakdown (Pending: 0, In Progress: 1, Completed: 1, Overdue: 0)
- Color-coded cards (purple, pink, cyan, green) for quick scanning

❌ **ISSUES:**
- "Completion Rate 33.3%" - **What does this mean?** (of all employees? assignments? questionnaires?)
  - **NN/g Principle Violated:** Visibility - unclear what metric represents
  - **Impact:** Medium - HR can't assess overall progress accurately
  - **Recommendation:** Add tooltip or label "1 of 3 assignments completed"

- Created/Completed metrics (Last 7 Days) - **Missing time frame context in heading**
  - **Recommendation:** Move "Last 7 Days" to main heading, not just subtitle

#### Screenshot 4: Questionnaire Assignment
✅ **GOOD:**
- Dual-pane layout makes assignment clear (left: employees, right: questionnaire)
- "SELECT ALL FILTERED" button - clear affordance
- Real-time feedback with selected count

❌ **ISSUES:**
- No indication of **which employees already have this questionnaire**
  - **NN/g Principle Violated:** Error Prevention - could assign duplicates
  - **Impact:** High - wastes time, confuses employees
  - **Recommendation:** Add badge "Already assigned" or disable/gray out

- "Assignment Settings" at bottom - **easy to miss due to fold**
  - **Recommendation:** Make "Due Date" and "Notes" more prominent or add summary at top

#### Screenshot 10: Organization Questionnaires
✅ **GOOD:**
- Multiple filter options (Questionnaire, Organization, Status, Category, Date Range)
- Tabs for different views (Department Overview, Employee Status, Questionnaire Performance, Analytics)

❌ **ISSUES:**
- Filter by "All Questionnaires" - **What happens if I select one? Does the whole page filter?**
  - **NN/g Principle Violated:** Visibility - unclear scope of filter
  - **Recommendation:** Show "(Filtering by: Beach Break 2026)" when active

- "1000 - Engineering" accordion - **Why is the number in the name?**
  - **Impact:** Low - but confusing
  - **Recommendation:** Use as ID code or remove if not meaningful to users

### 2. Match Between System and Real World
*"The design should speak the users' language"*

#### Screenshot 3: Category Administration
❌ **CRITICAL ISSUE:**
- Page title: "Category Administration"
- Subtitle: "Manage questionnaire categories"
- But categories shown are: "Beach Break", "1. Jahresgespräch"

**Problem:** What's the difference between:
- "Questionnaire Template" (Screenshot 8)
- "Category" (Screenshot 3)
- "Questionnaire Type"
- "Assessment Type"

**NN/g Principle Violated:** Match between system and real world - technical jargon vs user mental model

**User Mental Model (Likely):**
- "Performance Review 2026" (the thing I fill out)
- "Self-Assessment" (a section within it)
- "Competency Rating" (a question type)

**Recommendation:**
- Rename "Category" → "Questionnaire Type" (e.g., "Annual Review", "Quarterly Check-in")
- Update throughout: "Filter by Category" → "Filter by Type"
- Add help text: "Types help organize questionnaires by purpose (e.g., Annual Reviews, Onboarding)"

#### Screenshot 5: Assessment Items
✅ **GOOD:**
- Competency names in German (matches user language)
- Clear descriptions under each competency

❌ **ISSUES:**
- "Assessment Items" heading - **Technical term**
  - **Recommendation:** "Rate Competencies" or "Competency Assessment"

- "Who completes this section?" with radio buttons (Employee/Manager/Both)
  - **Issue:** Shown during template creation but **not visible when filling**
  - **NN/g Principle Violated:** Visibility - users don't understand why sections differ
  - **Recommendation:** Add indicator in questionnaire view (e.g., "Your Assessment" vs "Manager's Assessment")

### 3. User Control and Freedom
*"Users need a clearly marked emergency exit"*

#### Screenshot 6: Questionnaire Editor
✅ **GOOD:**
- "CLONE" button - easy to duplicate templates
- "UNPUBLISH" button - clear reversal action
- Tabs for workflow (Basic Info → Build Sections → Review & Publish)

❌ **ISSUES:**
- "UNPUBLISH" button - **What happens to in-progress questionnaires?**
  - **NN/g Principle Violated:** Error Prevention - unclear consequences
  - **Recommendation:** Show warning dialog: "3 assignments are in progress. Unpublishing will... [explain impact]"

- **No "Save Draft" or "Save Progress" button visible**
  - **Impact:** High - users fear losing work
  - **Recommendation:** Add auto-save indicator (e.g., "Last saved 2 min ago") or explicit "Save Draft" button

#### Screenshot 11: Team Questionnaires (Empty State)
✅ **GOOD:**
- Clear empty state message: "No team members found"
- Helpful context: "Your team members will appear here once they are assigned"

❌ **MISSING:**
- **No action to take** - What should I do now?
  - **Recommendation:** Add button "Assign Questionnaires" or "View All Employees"

### 4. Consistency and Standards
*"Users should not have to wonder whether different words, situations, or actions mean the same thing"*

#### Terminology Inconsistencies

| Screen | Term Used | Meaning |
|--------|-----------|---------|
| Screenshot 3 | "Categories" | Types of questionnaires |
| Screenshot 8 | "Template Name" | Specific questionnaire |
| Screenshot 4 | "Select Questionnaire" | Same as template? |
| Screenshot 10 | "Questionnaire Filter" | Filter by template |

**NN/g Principle Violated:** Consistency - same concept, different terms

**Recommendation:** Standardize on:
- **Questionnaire Type:** Annual Review, Onboarding, Quarterly Check-in
- **Questionnaire Template:** Beach Break 2026, 1. Jahresgespräch
- **Assignment:** When a template is given to an employee
- **Response:** Employee's answers

Update all UI text to use consistent terminology.

#### Button Placement Inconsistencies

| Screen | Primary Action Location |
|--------|------------------------|
| Screenshot 3 | Top right ("+ ADD NEW CATEGORY") |
| Screenshot 4 | Bottom ("Assignment Settings → Create Assignment") |
| Screenshot 6 | Top right ("CLONE", "UNPUBLISH") |

**Recommendation:** Standardize primary actions to **top right** consistently (except multi-step wizards where "Next" goes bottom-right)

### 5. Error Prevention
*"Good error messages are important, but the best designs prevent errors from happening"*

#### Screenshot 4: Questionnaire Assignment
❌ **CRITICAL:**
- Can select employees who already have this questionnaire
  - **Recommendation:** Show badge "Assigned" and disable selection (or show warning)

- No indication of conflicting due dates
  - **Scenario:** Employee has 3 questionnaires due same week
  - **Recommendation:** Show warning if due date conflicts with other assignments

❌ **MISSING CONFIRMATION:**
- "SELECT ALL FILTERED" button - **No preview of how many selected**
  - **Recommendation:** Change button text dynamically: "SELECT ALL (156 employees)"

#### Screenshot 5: Assessment Items - Duplicate Prevention
✅ **GOOD:**
- "SORT ITEMS" button - helps organize
- Required checkbox visible

❌ **ISSUE:**
- Can create duplicate competency titles (e.g., two "Fachkenntnisse")
  - **Recommendation:** Warn "Competency with this name already exists. Continue?"

### 6. Recognition Rather Than Recall
*"Minimize the user's memory load"*

#### Screenshot 8: Manage Questionnaires Table
✅ **GOOD:**
- "Sections" count (2) and "Questions" count (2) visible in table
- "Published By" shows who created (Adele Vance)
- Color-coded status badges ("PUBLISHED" in green)

❌ **ISSUE:**
- **Duplicate dates:** "Created: 21.10.2025" and "Published: 21.10.2025"
  - **Recommendation:** Combine to one column "Published 21.10.2025 by Adele Vance"

- No preview of questionnaire content from table view
  - **Recommendation:** Add "Preview" icon button in Actions column

#### Screenshot 10: Organization Overview - Memory Load
❌ **HIGH COGNITIVE LOAD:**
- Filters at top (Questionnaire, Organization, Status, Category, Date Range)
- Tabs below (Department Overview, Employee Status, Questionnaire Performance, Analytics)
- Accordion sections below that (1000 - Engineering)
- Team member details in each accordion

**User must remember:**
- Which filters are active
- Which tab they're on
- Which accordion is expanded
- What they're looking for

**Recommendation:**
- Add "Active Filters" summary bar (e.g., "Showing: Beach Break 2026 | Engineering | In Progress")
- Add "Clear All Filters" button
- Show applied filters as chips/tags

### 7. Flexibility and Efficiency of Use
*"Shortcuts — hidden from novice users — may speed up the interaction for expert users"*

#### Screenshot 2: Role Management
✅ **GOOD:**
- Search box for large lists (200 employees)
- Sortable columns (First Name, Last Name, Job Role, Organization)

❌ **MISSING EFFICIENCY FEATURES:**
- No bulk actions (e.g., "Change 10 selected employees to Manager role")
  - **Recommendation:** Add checkbox column + "Bulk Edit Roles" button

- No keyboard shortcuts visible
  - **Recommendation:** Add Ctrl+K for search, Ctrl+S for save (show in tooltip)

#### Screenshot 4: Assignment Creation - No Saved Templates
❌ **ISSUE:**
- Must select employees + questionnaire + settings **every time**
  - **Scenario:** HR assigns "Beach Break 2026" to all Engineering team annually
  - **Recommendation:** Add "Save as Assignment Template" button to reuse configuration

### 8. Aesthetic and Minimalist Design
*"Interfaces should not contain information that is rarely needed"*

#### Screenshot 1: HR Dashboard - Information Overload
❌ **TOO MUCH AT ONCE:**
- 4 stat cards at top
- 4 status cards below
- 3 time-based metrics below that
- Department accordion list below

**Problem:** HR must scan 11+ metrics to understand "How are we doing?"

**Recommendation:**
- **Progressive Disclosure:** Show only critical metrics by default
  - At-a-glance: "67% of employees have completed their reviews (2 of 3)"
  - Details: Expand to see breakdown
- Move "Avg. Completion Time: 0.0 days" to separate "Analytics" section

#### Screenshot 6: Questionnaire Editor - Tab Navigation
✅ **GOOD:**
- 3-step wizard (Basic Info → Build Sections → Review & Publish)
- Current step highlighted
- Completed step shows checkmark

❌ **ISSUE:**
- "Published" badge at top - **always visible even when editing draft**
  - **Recommendation:** Only show status badge on tab bar, not header

### 9. Help Users Recognize, Diagnose, and Recover from Errors
*"Error messages should be expressed in plain language"*

#### General Observation
- Most error handling not visible in screenshots
- Based on code review: Generic error messages ("Failed to save")

**Recommendation:**
- Add specific error messages:
  - ❌ "Failed to save questionnaire"
  - ✅ "Unable to save questionnaire. Due date must be in the future. Please select a date after today."

- Add error summaries at top of forms
  - "3 errors prevent saving: [list]"

### 10. Help and Documentation
*"It's best if the system doesn't need documentation, but help should be available"*

#### Screenshot 7: Questionnaire Basic Info
❌ **MISSING HELP:**
- "Requires Manager Review" checkbox - **What does this mean?**
  - **Recommendation:** Add tooltip or help icon: "If checked, manager must review and approve employee responses before finalization"

#### Screenshot 5: Assessment Items
❌ **COMPLEX FEATURE WITHOUT GUIDANCE:**
- "Who completes this section?" (Employee/Manager/Both)
  - **Recommendation:** Add help icon: "Employee: Only employee fills this section. Manager: Only manager fills. Both: Both fill separately (shown side-by-side in review)"

**General Recommendation:**
- Add contextual help (? icon) next to complex fields
- Add "Help Center" or "User Guide" link in header
- Add onboarding tooltip tour for first-time users

---

## Priority Issues & Recommendations

### 🔴 Critical (Fix Immediately - Blocks Task Completion)

#### 1. Missing Dialog Buttons (EditAnswerDialog)
**Issue:** Save/Cancel buttons not visible in Edit Answer dialog during review
**Impact:** **CRITICAL** - Cannot edit answers during review meeting
**NN/g Violation:** Visibility of system status, User control
**Fix:** Already addressed in code - verify after restart

#### 2. Confusing Review Banner Text
**Issue:** "Employee: Debra Berger | You can view and edit..."
**Actual user:** Christie Cline (Manager)
**Impact:** HIGH - Creates confusion about identity/permissions
**NN/g Violation:** Match between system and real world
**Fix:** ✅ Already fixed - "Reviewing Employee: Debra Berger | As the manager, you can..."

#### 3. Unclear Comment/Summary Timing
**Issue:** Labels don't explain when comments are created:
- "Manager Review Summary" - **When is this added?**
- "Employee Comments" - **When is this added?**
**Impact:** HIGH - Users don't understand workflow
**NN/g Violation:** Visibility, Help users recognize system state
**Fix:** ✅ Already fixed - Added context "Added by [name] after finishing review meeting"

### 🟡 High Priority (Affects Completion Rate & Quality)

#### 4. No Indication of Already-Assigned Employees
**Issue:** Can assign same questionnaire twice to same employee
**Impact:** HIGH - Confusion, duplicate work, data quality issues
**NN/g Violation:** Error prevention
**Fix:**
```csharp
// In AssignQuestionnaireDialog.razor
// Add badge to employee cards
@if (employee.HasQuestionnaire(SelectedTemplateId))
{
    <RadzenBadge BadgeStyle="BadgeStyle.Warning" Text="Already Assigned" />
}
```

#### 5. Unclear Completion Percentage Meaning
**Issue:** "Completion Rate 33.3%" - of what?
**Impact:** MEDIUM - HR can't interpret metrics
**NN/g Violation:** Visibility
**Fix:** Change label to "Completion Rate (1 of 3 assignments completed)"

#### 6. No Auto-Save or Save Indicator
**Issue:** Users fear losing work in questionnaire editor
**Impact:** MEDIUM - Anxiety, potential data loss
**NN/g Violation:** Visibility, User control
**Fix:**
```html
<!-- Add to QuestionnaireEditor -->
<div class="auto-save-indicator">
    <RadzenIcon Icon="cloud_done" />
    Last saved 2 minutes ago
</div>
```

#### 7. Missing Preview Before Bulk Assignment
**Issue:** "SELECT ALL FILTERED" doesn't show count
**Impact:** MEDIUM - Could accidentally assign to wrong group
**NN/g Violation:** Error prevention
**Fix:** Update button text dynamically: "SELECT ALL (87 EMPLOYEES)"

### 🟢 Medium Priority (Improves Efficiency & Satisfaction)

#### 8. Inconsistent Terminology (Category vs Template vs Questionnaire)
**Fix:** Create terminology guide and update all UI text consistently

#### 9. No Saved Assignment Templates (for recurring assignments)
**Fix:** Add "Save as Template" feature for HR bulk assignments

#### 10. High Cognitive Load on Organization Overview Page
**Fix:** Add "Active Filters" summary bar and "Clear All" button

#### 11. Empty State Missing Actions
**Fix:** Add "Assign Questionnaires" button to Team Overview empty state

#### 12. No Bulk Edit for Roles
**Fix:** Add checkbox selection + "Change Role" dropdown for bulk editing

### 🔵 Low Priority (Nice to Have - Reduces Learning Curve)

#### 13. Missing Contextual Help
**Fix:** Add (?) tooltips next to complex fields

#### 14. No Keyboard Shortcuts
**Fix:** Add Ctrl+K for search, Ctrl+S for save (with tooltip hints)

#### 15. No Questionnaire Preview from Table
**Fix:** Add eye icon button in Actions column for quick preview

---

## Recommended Next Steps

### Phase 1: Critical Fixes (This Week)
1. ✅ Verify EditAnswerDialog buttons work after restart
2. ✅ Confirm review banner text is clear
3. ✅ Verify comment labels show timing/author
4. Test with real user to confirm fixes work

### Phase 2: High-Priority UX Improvements (Next 2 Weeks)
1. Add "Already Assigned" indicators to employee selection
2. Improve completion metrics labeling
3. Add auto-save indicator to editors
4. Add preview to bulk selection actions
5. Create terminology consistency guide and update UI

### Phase 3: Efficiency Improvements (Next Month)
1. Add saved assignment templates
2. Add active filter summary bars
3. Implement bulk edit for roles
4. Add empty state actions

### Phase 4: Long-term Enhancements (Next Quarter)
1. Add comprehensive contextual help system
2. Implement keyboard shortcuts
3. Add quick preview features
4. Consider first-time user onboarding tour

---

## User Testing Plan

### Recommended Test Scenarios

**Test 1: Employee Annual Review**
- Task: Complete your annual performance review
- Success criteria: Completes without asking questions
- Key observations: Do they understand which sections are theirs? Do they know progress?

**Test 2: Manager Review Meeting**
- Task: Review employee's self-assessment and provide feedback
- Success criteria: Can edit answers, add review summary, finish meeting
- Key observations: Do buttons work? Is dual-answer view clear? Do they understand next steps?

**Test 3: HR Bulk Assignment**
- Task: Assign Beach Break 2026 to all Engineering department
- Success criteria: Assigns to correct people without duplicates
- Key observations: Do they notice already-assigned? Do they verify selection before creating?

**Test 4: HR Analytics Review**
- Task: Find out how many employees haven't completed their review
- Success criteria: Finds correct metric quickly
- Key observations: Do they understand the dashboard? Can they filter effectively?

### Success Metrics to Track
- Task completion rate (target: >90%)
- Time to complete (benchmark against current system)
- Error rate (duplicate assignments, missed steps)
- User satisfaction (SUS score, target: >70)
- Support ticket volume (compare before/after)

---

## Conclusion

The ti8m BeachBreak application has a **solid foundation** with modern design and clear visual hierarchy. The main issues are:

1. **Critical technical bugs** (missing buttons) - mostly fixed
2. **Contextual clarity** (who, when, why) - significantly improved
3. **Terminology consistency** (needs standardization)
4. **Error prevention** (needs more guardrails)
5. **Efficiency features** (needs shortcuts for repeat tasks)

**Overall NN/g Compliance:** 6.5/10
**With Priority Fixes:** 8.5/10 (estimated)

The biggest wins will come from:
- Preventing errors (assignment duplicates, conflicting dates)
- Reducing cognitive load (consistent terms, active filter summary)
- Adding efficiency features (saved templates, bulk actions)

**Recommended Focus:** Fix critical bugs, then focus on error prevention before adding efficiency features.
