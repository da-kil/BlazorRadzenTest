# Progress Calculation Plan

## Problem Statement

Calculate accurate progress percentages for questionnaires where sections and questions have **role-based completion requirements**:

- **Employee-only sections**: Completed by employee only
- **Manager-only sections**: Completed by manager only
- **Both sections**: Both employee and manager must complete

**Challenge**: Progress must be calculated differently depending on the viewer's role:
- **Employee view**: Should only count questions they're responsible for
- **Manager view**: Should only count questions they're responsible for
- **Overall progress**: Combined progress across both roles

## Domain Model Analysis

### QuestionnaireTemplate Structure
```
QuestionnaireTemplate
├── Sections: List<QuestionSection>
│   ├── CompletionRole: enum { Employee, Manager, Both }
│   ├── Questions: List<QuestionItem>
│   │   ├── IsRequired: bool
│   │   ├── Type: QuestionType
│   │   └── Configuration: Dictionary<string, object>
```

### QuestionnaireResponse Structure
```
QuestionnaireResponse
└── SectionResponses: Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, object>>>
    Structure: SectionId -> Role -> QuestionId -> Answer
```

**Key Insight**: Responses are **role-separated**, meaning:
- Employee responses stored under `CompletionRole.Employee`
- Manager responses stored under `CompletionRole.Manager`
- For sections with `CompletionRole.Both`, responses stored under both keys

## Progress Calculation Strategy

### Approach: Role-Aware Question Counting

For each role (Employee/Manager), calculate:
```
Progress = (Answered Questions for Role / Total Questions for Role) × 100
```

### Step-by-Step Algorithm

#### 1. **Template Analysis** (from QuestionnaireTemplate)

For each section in the template:

**If section.CompletionRole == Employee:**
- Add all questions to Employee question count
- Manager question count unchanged

**If section.CompletionRole == Manager:**
- Add all questions to Manager question count
- Employee question count unchanged

**If section.CompletionRole == Both:**
- Add all questions to **both** Employee and Manager counts
- Each role must answer independently

#### 2. **Response Analysis** (from QuestionnaireResponse)

Count answered questions per role:

```csharp
var employeeAnsweredCount = 0;
var managerAnsweredCount = 0;

foreach (var (sectionId, roleResponses) in response.SectionResponses)
{
    // Find this section in the template
    var section = template.Sections.FirstOrDefault(s => s.Id == sectionId);
    if (section == null) continue;

    // Count employee answers
    if (roleResponses.TryGetValue(CompletionRole.Employee, out var employeeAnswers))
    {
        employeeAnsweredCount += CountValidAnswers(employeeAnswers, section.Questions);
    }

    // Count manager answers
    if (roleResponses.TryGetValue(CompletionRole.Manager, out var managerAnswers))
    {
        managerAnsweredCount += CountValidAnswers(managerAnswers, section.Questions);
    }
}
```

#### 3. **Valid Answer Detection**

A question is "answered" if:
- Response exists for the question ID
- Response is not null
- Response is not empty string (for text questions)
- Response has meaningful value (not default/placeholder)

```csharp
private int CountValidAnswers(
    Dictionary<Guid, object> answers,
    List<QuestionItem> questions)
{
    var count = 0;
    foreach (var question in questions)
    {
        if (answers.TryGetValue(question.Id, out var answer) &&
            IsValidAnswer(answer, question.Type))
        {
            count++;
        }
    }
    return count;
}

private bool IsValidAnswer(object answer, QuestionType type)
{
    if (answer == null) return false;

    return type switch
    {
        QuestionType.ShortText => !string.IsNullOrWhiteSpace(answer.ToString()),
        QuestionType.LongText => !string.IsNullOrWhiteSpace(answer.ToString()),
        QuestionType.SingleChoice => answer.ToString() is not null && answer.ToString() != "",
        QuestionType.MultipleChoice => answer is IEnumerable<object> list && list.Any(),
        QuestionType.Rating => int.TryParse(answer.ToString(), out var rating) && rating > 0,
        QuestionType.YesNo => answer is bool,
        QuestionType.Date => answer is DateTime or DateOnly or string,
        _ => answer != null
    };
}
```

#### 4. **Progress Percentage Calculation**

```csharp
public class ProgressCalculation
{
    public double EmployeeProgress { get; set; }
    public double ManagerProgress { get; set; }
    public double OverallProgress { get; set; }
}

public ProgressCalculation Calculate(
    QuestionnaireTemplate template,
    QuestionnaireResponse response)
{
    // Step 1: Count total questions per role from template
    var employeeTotalQuestions = 0;
    var managerTotalQuestions = 0;

    foreach (var section in template.Sections)
    {
        var questionCount = section.Questions.Count;

        switch (section.CompletionRole)
        {
            case CompletionRole.Employee:
                employeeTotalQuestions += questionCount;
                break;

            case CompletionRole.Manager:
                managerTotalQuestions += questionCount;
                break;

            case CompletionRole.Both:
                employeeTotalQuestions += questionCount;
                managerTotalQuestions += questionCount;
                break;
        }
    }

    // Step 2: Count answered questions per role from responses
    var (employeeAnsweredCount, managerAnsweredCount) =
        CountAnsweredQuestions(template, response);

    // Step 3: Calculate percentages
    var result = new ProgressCalculation();

    if (employeeTotalQuestions > 0)
    {
        result.EmployeeProgress =
            (double)employeeAnsweredCount / employeeTotalQuestions * 100;
    }

    if (managerTotalQuestions > 0)
    {
        result.ManagerProgress =
            (double)managerAnsweredCount / managerTotalQuestions * 100;
    }

    // Overall progress: average of both roles (if both have questions)
    if (employeeTotalQuestions > 0 && managerTotalQuestions > 0)
    {
        result.OverallProgress =
            (result.EmployeeProgress + result.ManagerProgress) / 2;
    }
    else if (employeeTotalQuestions > 0)
    {
        result.OverallProgress = result.EmployeeProgress;
    }
    else if (managerTotalQuestions > 0)
    {
        result.OverallProgress = result.ManagerProgress;
    }

    return result;
}
```

## Example Calculation

### Scenario

**Template Structure:**
- Section 1 (CompletionRole.Employee): 5 questions
- Section 2 (CompletionRole.Both): 3 questions
- Section 3 (CompletionRole.Manager): 4 questions

**Expected Totals:**
- Employee total: 5 + 3 = **8 questions**
- Manager total: 3 + 4 = **7 questions**

**Response Data:**
- Section 1 (Employee): 3 of 5 answered
- Section 2 (Employee part): 2 of 3 answered
- Section 2 (Manager part): 1 of 3 answered
- Section 3 (Manager): 4 of 4 answered

**Progress Calculation:**
- Employee progress: (3 + 2) / 8 = **62.5%**
- Manager progress: (1 + 4) / 7 = **71.4%**
- Overall progress: (62.5 + 71.4) / 2 = **66.95%**

### Scenario 2: Employee-Only Questionnaire

**Template Structure:**
- Section 1 (CompletionRole.Employee): 10 questions

**Expected Totals:**
- Employee total: **10 questions**
- Manager total: **0 questions**

**Response Data:**
- Section 1 (Employee): 7 of 10 answered

**Progress Calculation:**
- Employee progress: 7 / 10 = **70%**
- Manager progress: **N/A** (no manager questions)
- Overall progress: **70%** (falls back to employee progress only)

## Implementation Plan

### Phase 1: Create Progress Calculation Service

**File**: `02_Application/ti8m.BeachBreak.Application.Query/Services/ProgressCalculationService.cs`

```csharp
public interface IProgressCalculationService
{
    ProgressCalculation Calculate(
        QuestionnaireTemplate template,
        QuestionnaireResponse response);
}

public class ProgressCalculationService : IProgressCalculationService
{
    // Implementation as described above
}
```

**Register in DI**: `Application.Query.Extensions.cs`

### Phase 2: Update ResponsesController

**File**: `03_Infrastructure/ti8m.BeachBreak.QueryApi/Controllers/ResponsesController.cs`

**Current code (line 169):**
```csharp
ProgressPercentage = 0 // TODO: Calculate progress percentage
```

**Updated code:**
```csharp
ProgressPercentage = progressService.Calculate(template, response).OverallProgress
```

**Changes needed:**
1. Inject `IProgressCalculationService` in controller constructor
2. Fetch template alongside response (may need repository method)
3. Call service to calculate progress
4. Map to DTO

### Phase 3: Update Team Progress Query Handler

**File**: `02_Application/ti8m.BeachBreak.Application.Query/Queries/ManagerQueries/GetTeamProgressQueryHandler.cs`

**Current code (line 50):**
```csharp
// TODO: Calculate actual progress from responses when IQuestionnaireResponseRepository is available
```

**Updated approach:**
```csharp
foreach (var assignment in assignments)
{
    var response = await responseRepository.FindByAssignmentIdAsync(assignment.Id);
    var template = await templateRepository.GetByIdAsync(assignment.TemplateId);

    if (response != null && template != null)
    {
        var progress = progressService.Calculate(template, response);

        // Map to DTO with role-specific progress
        teamMember.EmployeeProgress = progress.EmployeeProgress;
        teamMember.ManagerProgress = progress.ManagerProgress;
        teamMember.OverallProgress = progress.OverallProgress;
    }
}
```

### Phase 4: Add Unit Tests

**File**: `Tests/ProgressCalculationServiceTests.cs`

**Test cases:**
1. Employee-only sections
2. Manager-only sections
3. Both-role sections
4. Mixed sections (all three types)
5. No responses (0% progress)
6. Partial responses
7. Complete responses (100% progress)
8. Empty template (edge case)
9. Invalid/null answers

## Edge Cases to Handle

1. **Template has no sections**: Return 0% progress
2. **Response exists but no answers**: Return 0% progress
3. **Template changed after assignment**: Use original template (snapshot)
4. **Question removed from template**: Ignore orphaned responses
5. **Required vs optional questions**: Current plan treats all questions equally
   - **Future enhancement**: Weight by `IsRequired` flag

## Required vs Optional Questions (Future Enhancement)

**Current approach**: All questions weighted equally

**Alternative approach**: Only count required questions
```csharp
var requiredQuestions = section.Questions.Where(q => q.IsRequired).ToList();
```

**Decision**: Start with **all questions** (simpler), can refine later based on product requirements.

## Performance Considerations

### Caching Strategy
- Template structure rarely changes → cache in memory
- Responses change frequently → fetch on-demand
- Calculate progress on-read (not stored)

### Optimization for Batch Calculations
When calculating progress for multiple assignments (team dashboard):

```csharp
public async Task<Dictionary<Guid, ProgressCalculation>> CalculateBatchAsync(
    List<Guid> assignmentIds)
{
    // 1. Batch fetch all responses
    var responses = await responseRepository.GetByAssignmentIdsAsync(assignmentIds);

    // 2. Batch fetch all unique templates
    var templateIds = responses.Select(r => r.TemplateId).Distinct();
    var templates = await templateRepository.GetByIdsAsync(templateIds);

    // 3. Calculate in parallel
    var results = new ConcurrentDictionary<Guid, ProgressCalculation>();
    Parallel.ForEach(responses, response =>
    {
        var template = templates.FirstOrDefault(t => t.Id == response.TemplateId);
        if (template != null)
        {
            results[response.AssignmentId] = Calculate(template, response);
        }
    });

    return results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}
```

## Files to Create/Modify

### Create:
1. `02_Application/ti8m.BeachBreak.Application.Query/Services/IProgressCalculationService.cs`
2. `02_Application/ti8m.BeachBreak.Application.Query/Services/ProgressCalculationService.cs`
3. `02_Application/ti8m.BeachBreak.Application.Query/Services/ProgressCalculation.cs` (DTO)

### Modify:
4. `02_Application/ti8m.BeachBreak.Application.Query/Extensions.cs` (register service)
5. `03_Infrastructure/ti8m.BeachBreak.QueryApi/Controllers/ResponsesController.cs` (inject + use)
6. `02_Application/ti8m.BeachBreak.Application.Query/Queries/ManagerQueries/GetTeamProgressQueryHandler.cs` (inject + use)

### Optional Enhancement:
7. `02_Application/ti8m.BeachBreak.Application.Query/Services/ProgressCalculationBatchService.cs` (for team dashboard)

## Estimated Effort

- **Phase 1** (Service): 2-3 hours
- **Phase 2** (ResponsesController): 1 hour
- **Phase 3** (Team Progress): 1-2 hours
- **Phase 4** (Testing): 2 hours
- **Total**: **6-8 hours**

## Success Criteria

✅ Employee sees progress for their questions only
✅ Manager sees progress for their questions only
✅ Overall progress reflects combined completion
✅ Handles all CompletionRole scenarios (Employee, Manager, Both)
✅ Zero-division protection (no questions → 0% progress)
✅ Performance acceptable for team dashboards (batch calculation)
✅ Unit tests cover all scenarios

## Next Steps

1. Review and approve this plan
2. Implement Phase 1 (ProgressCalculationService)
3. Test with sample data
4. Implement Phase 2 & 3 (integrate with controllers)
5. Add comprehensive unit tests
6. Test in Aspire environment with real questionnaires

---

**Questions for Review:**

1. Should required vs optional questions be weighted differently?
2. Should "Both" sections count as 2× weight in overall progress?
3. How to handle partially answered complex questions (e.g., multi-part questions)?
4. Should progress be stored (denormalized) or calculated on-demand?
