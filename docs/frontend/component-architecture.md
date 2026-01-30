# Frontend Component Architecture Guide

## Table of Contents
1. [Questionnaire Rendering Architecture](#questionnaire-rendering-architecture)
2. [Optimized Components System](#optimized-components-system)
3. [Component Lifecycle Rules](#component-lifecycle-rules)
4. [Validation Patterns](#validation-patterns)
5. [Code Review Guidelines](#code-review-guidelines)
6. [Historical Context](#historical-context)
7. [Troubleshooting](#troubleshooting)

---

## Questionnaire Rendering Architecture

**CRITICAL**: This pattern must be followed for ALL question rendering to prevent code duplication and data inconsistencies.

### Always Use Optimized Components

**NEVER** write inline question rendering logic. **ALWAYS** use the centralized Optimized components.

**Why This Rule Exists**:
- Prevents ~500+ lines of code duplication across components
- Ensures consistent data key formats (prevents data corruption bugs)
- Centralizes validation logic (prevents submit button bugs)
- Maintains single source of truth for question rendering

### Essential Requirements for CLAUDE.md Compliance

When using OptimizedQuestionRenderer, you **MUST** follow these core requirements:

- **Use OptimizedQuestionRenderer as master dispatcher** - Never write inline question rendering
- **Use correct data key format** (`"section_"` not `"text_"`) - Prevents data corruption
- **Initialize data in both `OnInitialized()` and `OnParametersSet()`** - Ensures proper component lifecycle
- **Call `UpdateProgress()` when responses change** - Enables proper validation and submit button state

---

## Optimized Components System

### Component Hierarchy

#### OptimizedQuestionRenderer.razor
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Components/Questions/`

**Purpose**: Master dispatcher for all question types
- Routes questions to appropriate specialized renderers
- Handles common question properties (title, description, required status)
- Manages progress tracking and validation callbacks
- Provides consistent layout and styling

**Usage**:
```razor
<OptimizedQuestionRenderer
    QuestionType="@section.Type"
    Configuration="@section.Configuration"
    SectionTitle="@section.Title"
    ResponseData="@GetResponseDataForSection(section.Id)"
    OnResponseChanged="@UpdateProgress"
    IsReadOnly="@isReadOnly" />
```

#### Specialized Question Components

**OptimizedAssessmentQuestion.razor**:
- Handles competency assessment questions with rating scales
- Parses `AssessmentConfiguration` for competencies and scale settings
- Renders rating controls with visual feedback
- Validates all competencies are rated before marking complete

**OptimizedTextQuestion.razor**:
- Handles text-based questions with single or multiple sections
- Parses `TextQuestionConfiguration` for section definitions
- Uses consistent data key format: `"section_{index}"`
- Supports rich text editing when configured

**OptimizedGoalQuestion.razor**:
- Handles goal achievement questions
- Dynamically creates goals based on predecessor questionnaire data
- Manages goal-achievement mapping and tracking

### Data Key Standards

**CRITICAL**: All components use standardized data key formats to prevent data corruption:

**Assessment Questions**:
- Key format: `"competency_{competencyId}"`
- Value: Integer rating (1-5 scale)

**Text Questions**:
- Key format: `"section_{index}"` (0-based indexing)
- Value: String text response
- **NEVER use** `"text_{index}"` format (legacy, causes data loss)

**Goal Questions**:
- Key format: `"goal_{goalId}"`
- Value: Achievement status or progress data

---

## Configuration Parsing

### Centralized Configuration Access

**NEVER** duplicate configuration parsing logic. If you need to parse question configuration:

1. **For rendering**: Use the Optimized components (they handle parsing internally)
2. **For validation**: Use the same key format as the Optimized components
3. **For custom logic**: Use shared services like `QuestionConfigurationService`

### Shared Configuration Services

**QuestionConfigurationService**:
```csharp
// ✅ GOOD: Use centralized service
var competencies = questionConfigurationService.GetCompetencies(section.Configuration);
var ratingScale = questionConfigurationService.GetRatingScale(section.Configuration);
```

**❌ AVOID: Duplicate parsing logic**:
```csharp
// BAD: Don't duplicate this across components
private List<CompetencyModel> GetCompetenciesFromConfiguration(IQuestionConfiguration config)
{
    if (config is AssessmentConfiguration assessmentConfig)
        return assessmentConfig.Evaluations;
    return new List<CompetencyModel>();
}
```

### Existing Duplications to Fix

**Known Problem Areas** (As of 2025-10-22):

If working with these components, **check for and fix duplications**:

- **QuestionnaireReviewMode.razor**: Has ~350 lines duplicate rendering logic, needs refactoring to use OptimizedQuestionRenderer
- **EditAnswerDialog.razor**: Has ~50 lines duplicate text question rendering, uses old "text_" key format
- **PreviewTab.razor**: Has ~100 lines duplicate configuration parsing, acceptable for preview purposes
- **DynamicQuestionnaire.razor**: Was refactored to use OptimizedQuestionRenderer, may still have some duplicate validation helpers

---

## Component Lifecycle Rules

### Initialization Pattern

**ALWAYS** initialize data in `OnInitialized()` in addition to `OnParametersSet()`:

```csharp
protected override void OnInitialized()
{
    InitializeResponseData();
    UpdateProgress();
}

protected override void OnParametersSet()
{
    if (hasParametersChanged())
    {
        InitializeResponseData();
        UpdateProgress();
    }
}
```

**Rationale**:
- `OnInitialized()`: Ensures data is ready on first render
- `OnParametersSet()`: Handles parameter updates from parent components
- Both are needed for proper lifecycle management

### State Management

**Response Data Updates**:
```csharp
private void OnResponseChanged(string key, object value)
{
    ResponseData[key] = value;
    OnResponseChanged?.Invoke(); // Notify parent component
    UpdateProgress(); // Recalculate completion status
    StateHasChanged(); // Trigger UI update
}
```

**Progress Calculation**:
- Called after every response change
- Uses same validation logic as submission
- Updates parent component's progress tracking
- Enables/disables submit buttons appropriately

---

## Validation Patterns

### Response Data Validation

**CRITICAL**: When validating question responses, **ALWAYS** match the data keys used by the Optimized components:

```csharp
// ✅ GOOD: Validation matches component key format
private bool ValidateAssessmentComplete(IQuestionConfiguration config, Dictionary<string, object> responseData)
{
    if (config is AssessmentConfiguration assessmentConfig)
    {
        foreach (var competency in assessmentConfig.Evaluations)
        {
            var key = $"competency_{competency.Id}"; // Matches OptimizedAssessmentQuestion
            if (!responseData.ContainsKey(key) || (int)responseData[key] == 0)
                return false;
        }
    }
    return true;
}
```

**❌ AVOID: Inconsistent key formats**:
```csharp
// BAD: Different key format than components use
var key = $"assessment_{competency.Id}"; // Component uses "competency_"
var key = $"text_{index}"; // Component uses "section_"
```

### Template Configuration vs Response Data

**CRITICAL**: Always validate response data AGAINST template configuration:

```csharp
// ✅ GOOD: Validates response data against expected template structure
public bool IsQuestionComplete(QuestionSection section, Dictionary<string, object> responseData)
{
    return section.Configuration switch
    {
        AssessmentConfiguration assessmentConfig =>
            ValidateAllCompetenciesRated(assessmentConfig.Evaluations, responseData),
        TextQuestionConfiguration textConfig =>
            ValidateAllTextSectionsComplete(textConfig.Sections, responseData),
        GoalConfiguration goalConfig =>
            ValidateAllGoalsAddressed(goalConfig, responseData),
        _ => false
    };
}
```

---

## Code Review Guidelines

### Code Review Checklist for Question Rendering

Before submitting any code that touches question rendering, verify:

- [ ] **Uses OptimizedQuestionRenderer** (not inline rendering)
- [ ] **Uses correct data key format** (`"section_"` not `"text_"`)
- [ ] **No duplicate configuration parsing** (GetCompetencies, GetRatingScale, etc.)
- [ ] **Validation matches data keys** used by Optimized components
- [ ] **Component lifecycle** properly initializes data in OnInitialized()
- [ ] **Progress/validation updates** when responses change (calls UpdateProgress())
- [ ] **Translation keys** properly defined for all UI text
- [ ] **Error handling** for missing or invalid configuration
- [ ] **Accessibility** attributes for screen readers
- [ ] **Responsive design** works on different screen sizes

### Red Flags During Review

**Immediate Fix Required**:
- Inline question rendering logic (copy of OptimizedQuestionRenderer code)
- Hardcoded configuration parsing (GetCompetencies, GetRatingScale methods)
- Wrong data key format ("text_" instead of "section_")
- Missing OnInitialized() initialization
- Hardcoded UI text (not using @T())

**Architectural Concern**:
- New question type added without corresponding OptimizedQuestion component
- Breaking changes to existing data key formats
- Bypassing validation patterns
- Component exceeding 200 lines (likely has inline logic that should be extracted)

---

## Historical Context

### Why These Rules Exist

These rules were established after discovering critical production bugs caused by code duplication:

#### 1. Submit Button Bug (Discovered 2025-10-15)
**Symptom**: Submit button stayed enabled when required fields were cleared
**Cause**: Validation logic duplicated across components, some copies not updating progress
**Impact**: Users could submit incomplete questionnaires
**Fix**: Centralized validation in OptimizedQuestionRenderer

#### 2. Data Key Mismatch Bug (Discovered 2025-10-18)
**Symptom**: Review mode couldn't read saved answers, showed blank fields
**Cause**: Components used "text_" keys while database had "section_" keys
**Impact**: Data loss appearance, user confusion
**Fix**: Standardized on "section_" key format across all components

#### 3. Edit Dialog Bug (Discovered 2025-10-20)
**Symptom**: Edit dialog overwrote existing answers with empty values
**Cause**: EditAnswerDialog used "text_" keys, overwrote "section_" data
**Impact**: Actual data corruption, lost user work
**Fix**: Updated EditAnswerDialog to use correct key format

#### 4. Code Duplication Problem (Ongoing)
**Symptom**: Same bug fixed in 5+ different places
**Cause**: ~500+ lines of duplicate rendering/validation logic
**Impact**: Maintenance burden, inconsistent behavior
**Solution**: OptimizedQuestionRenderer system

### The Refactoring Effort

**Scope** (October 2025):
- Analyzed 15+ components with question rendering logic
- Identified ~500 lines of duplicate code
- Created OptimizedQuestionRenderer system
- Refactored DynamicQuestionnaire.razor (primary success case)
- Established validation patterns
- Created comprehensive documentation

**Investment**: ~3 weeks of development time

**Results**:
- 80% reduction in question rendering code duplication
- Zero data key mismatch bugs since implementation
- Consistent validation across all question types
- Faster development of new question features

### When to Break These Rules

**NEVER**. If you think you need to break these rules:

1. **Ask the user first**
2. **Document the architectural decision** in an ADR
3. **Provide justification** for why existing components can't be used
4. **Add comprehensive tests** to prevent regressions

**Important**: The refactoring effort to create the Optimized components was significant. Don't waste it by reintroducing duplication.

---

## Troubleshooting

### Common Issues and Solutions

#### 1. Component Not Rendering Questions

**Symptoms**:
- Questions appear blank or don't load
- Console errors about missing configuration

**Common Causes**:
- Incorrect `QuestionType` parameter
- Invalid `Configuration` object
- Missing `ResponseData` initialization

**Debug Steps**:
```csharp
// Add debugging to parent component
Console.WriteLine($"QuestionType: {section.Type}");
Console.WriteLine($"Configuration: {JsonSerializer.Serialize(section.Configuration)}");
Console.WriteLine($"ResponseData keys: {string.Join(", ", ResponseData.Keys)}");
```

#### 2. Data Key Mismatch Errors

**Symptoms**:
- Saved answers don't appear when editing
- Validation fails unexpectedly
- Data appears lost between sessions

**Common Causes**:
- Using wrong key format (e.g., "text_" instead of "section_")
- Component not using OptimizedQuestionRenderer
- Custom validation logic with different key format

**Fix**:
- Verify component uses OptimizedQuestionRenderer
- Check data keys match expected format
- Update validation logic to match component keys

#### 3. Progress Tracking Not Working

**Symptoms**:
- Submit button enabled when questions incomplete
- Progress bar not updating
- Validation inconsistent

**Common Causes**:
- Missing `OnResponseChanged` callback
- `UpdateProgress()` not called after data changes
- Custom validation logic not matching component validation

**Fix**:
```razor
<OptimizedQuestionRenderer
    OnResponseChanged="@UpdateProgress"  <!-- Essential -->
    ... />
```

#### 4. Translation Keys Not Displaying

**Symptoms**:
- UI shows "sections.assessment-title" instead of "Assessment Title"
- Language switching doesn't affect question titles

**Common Causes**:
- Translation key missing from test-translations.json
- Incorrect key format or spelling
- @T() not used in component

**Fix**:
- Run validation script: `validate-translations.ps1`
- Add missing translations to test-translations.json
- Verify @T() usage in component

---

## Component Extension Guidelines

### Adding New Question Types

If you need to add a new question type:

1. **Create Question Configuration Class**:
   ```csharp
   public record NewQuestionConfiguration : IQuestionConfiguration
   {
       public QuestionType QuestionType => QuestionType.NewType;
       // Add specific properties
   }
   ```

2. **Create Optimized Component**:
   ```razor
   @* File: OptimizedNewQuestion.razor *@
   <div class="question-container">
       <!-- Implement question-specific UI -->
   </div>
   ```

3. **Update OptimizedQuestionRenderer**:
   ```razor
   @switch (QuestionType)
   {
       case QuestionType.NewType:
           <OptimizedNewQuestion ... />
           break;
   }
   ```

4. **Create Question Handler**:
   ```csharp
   public class NewQuestionHandler : IQuestionTypeHandler
   {
       // Implement validation and data processing
   }
   ```

5. **Add Validation Logic**:
   - Update validation services
   - Add tests for new question type
   - Update progress calculation logic

### Component Architecture Principles

**Single Responsibility**:
- OptimizedQuestionRenderer: Routing and common functionality
- OptimizedXxxQuestion: Question-type-specific rendering
- QuestionHandlers: Business logic and validation

**Dependency Flow**:
```
Parent Component
    ↓
OptimizedQuestionRenderer (routing)
    ↓
OptimizedXxxQuestion (rendering)
    ↓
QuestionHandler (validation)
```

**Data Flow**:
```
ResponseData (parent)
    ↓ (binding)
OptimizedQuestionRenderer
    ↓ (parameters)
OptimizedXxxQuestion
    ↓ (events)
OnResponseChanged callback
    ↓ (updates)
Parent component state
```

---

## Component Lifecycle Rules

### Initialization Requirements

**ALWAYS** initialize data in both lifecycle methods:

```csharp
protected override void OnInitialized()
{
    InitializeResponseData();
    UpdateProgress();
}

protected override void OnParametersSet()
{
    if (hasParametersChanged())
    {
        InitializeResponseData();
        UpdateProgress();
    }
}
```

**Why Both Are Needed**:
- `OnInitialized()`: First-time component setup
- `OnParametersSet()`: Handles parameter updates from parent
- Ensures data consistency in all scenarios

### State Management Pattern

**Component State Updates**:
```csharp
private async Task OnValueChanged(string key, object value)
{
    // 1. Update local state
    ResponseData[key] = value;

    // 2. Notify parent component
    await OnResponseChanged.InvokeAsync();

    // 3. Recalculate progress
    UpdateProgress();

    // 4. Trigger UI refresh
    StateHasChanged();
}
```

**Parameter Change Handling**:
```csharp
private bool hasParametersChanged()
{
    return !ReferenceEquals(_previousConfiguration, Configuration) ||
           !ReferenceEquals(_previousResponseData, ResponseData);
}
```

---

## Validation Patterns

### Response Data Validation

**Template-Driven Validation Pattern**:

When validating question responses, **ALWAYS** match the data keys used by the Optimized components:

```csharp
// ✅ GOOD: Extract expected structure from template, validate against response
private bool ValidateAssessmentComplete(AssessmentConfiguration config, Dictionary<string, object> responseData)
{
    // Get expected competencies from template configuration
    foreach (var competency in config.Evaluations)
    {
        var expectedKey = $"competency_{competency.Id}"; // Matches OptimizedAssessmentQuestion

        // Verify this specific competency is rated in response data
        if (!responseData.ContainsKey(expectedKey) ||
            !int.TryParse(responseData[expectedKey]?.ToString(), out var rating) ||
            rating == 0)
        {
            return false; // This specific competency is not rated
        }
    }
    return true; // All template-defined competencies are rated
}
```

**❌ AVOID: Response-only validation**:
```csharp
// BAD: Only checks response data, ignores template requirements
private bool ValidateAssessmentComplete(Dictionary<string, object> responseData)
{
    return responseData.Any(kvp => kvp.Key.StartsWith("competency_") && (int)kvp.Value > 0);
    // Problem: Doesn't verify ALL required competencies are rated
}
```

### Historical Bug Context

**Assessment Validation Bug** (Fixed 2025-11-10): Assessment validation originally only checked if ANY competency was rated (`Any(c => c.Rating > 0)`), rather than validating that ALL competencies defined in the template configuration were rated. This allowed incomplete assessments to be marked as complete, highlighting the importance of template-driven validation.

**Enum Serialization Bug** (Historical): QuestionType enum had different implicit ordering in CommandApi (Assessment, Goal, TextQuestion) vs other layers (Assessment, TextQuestion, Goal), causing Goal questions (saved as Type=1) to be deserialized as TextQuestion (Type=1) in other layers, rendering them incorrectly. This bug demonstrates why explicit enum values are critical in CQRS architectures.

### Progress Calculation Integration

**Consistent Progress Updates**:
```csharp
private void UpdateProgress()
{
    var isComplete = ValidateQuestionComplete();
    OnProgressChanged?.Invoke(SectionId, isComplete);

    // Update submit button state if this is the last incomplete section
    CheckSubmissionEligibility();
}
```

**Where This Applies**:
- `QuestionnaireResponse.GetCompletedSections()` - Domain aggregate validation (01_Domain)
- Frontend validation in question components - Must match domain validation logic
- Command handlers that validate before submission - Use domain aggregate methods

---

## Code Review Guidelines

### Pre-Submission Checklist

**Before submitting any code that touches question rendering**:

1. **Architecture Compliance**:
   - [ ] Uses OptimizedQuestionRenderer (not inline rendering)
   - [ ] No duplicate configuration parsing methods
   - [ ] Follows single responsibility principle

2. **Data Integrity**:
   - [ ] Uses correct data key format (`"section_"` not `"text_"`)
   - [ ] Validation matches data keys used by Optimized components
   - [ ] No hardcoded key formats

3. **Component Lifecycle**:
   - [ ] Component lifecycle properly initializes data in OnInitialized()
   - [ ] Handles parameter changes in OnParametersSet()
   - [ ] Calls StateHasChanged() after data updates

4. **Progress and Validation**:
   - [ ] Progress/validation updates when responses change
   - [ ] Calls UpdateProgress() after data changes
   - [ ] Submit button state reflects validation status

5. **User Experience**:
   - [ ] Translation keys properly defined for all UI text
   - [ ] Responsive design works on different screen sizes
   - [ ] Accessibility attributes for screen readers
   - [ ] Loading states and error handling

### Architectural Red Flags

**Immediate Fix Required**:
- New component with inline question rendering (> 50 lines of question logic)
- Duplicate methods: `GetCompetenciesFromConfiguration()`, `GetRatingScaleFromQuestion()`, etc.
- Hardcoded configuration parsing instead of using shared services
- Components that don't use OptimizedQuestionRenderer for question rendering

**Design Review Needed**:
- Component exceeding 300 lines (likely has responsibilities that should be split)
- Direct manipulation of ResponseData without going through OnResponseChanged
- Custom validation logic that differs from OptimizedQuestionRenderer validation
- Breaking changes to existing data key formats

### Code Quality Standards

**Component Structure**:
```csharp
// ✅ GOOD: Clean component structure
@page "/example"
@using Services
@inject QuestionConfigurationService ConfigService

<OptimizedQuestionRenderer ... />

@code {
    // Parameters
    [Parameter] public QuestionSection Section { get; set; }

    // State
    private Dictionary<string, object> responseData = new();

    // Lifecycle
    protected override void OnInitialized() { ... }
    protected override void OnParametersSet() { ... }

    // Event handlers
    private async Task OnResponseChanged() { ... }

    // Helper methods (if needed)
    private bool ValidateComplete() { ... }
}
```

**Maximum Component Size**:
- Question components: ~150 lines max
- Page components: ~300 lines max
- Dialog components: ~200 lines max
- Larger components should be split into sub-components

---

## Historical Context

### The Great Refactoring (October 2025)

**Problem Discovery**:
During a comprehensive architecture review, several critical issues were identified:
- ~500 lines of duplicate question rendering code across 5+ components
- Inconsistent data key formats causing data corruption
- Submit button validation bugs affecting user experience
- Maintenance burden: same fix needed in multiple places

**Investigation Process**:
1. **Code Analysis**: Identified all components with question rendering
2. **Bug Reproduction**: Reproduced data key mismatch and validation bugs
3. **Impact Assessment**: Quantified code duplication and maintenance costs
4. **Solution Design**: Designed OptimizedQuestionRenderer system
5. **Incremental Migration**: Refactored components one by one

**Key Decisions**:
- Centralize all question rendering in OptimizedQuestionRenderer
- Standardize on "section_" key format for text questions
- Eliminate duplicate configuration parsing
- Mandate comprehensive component lifecycle management

### Lessons Learned

**Technical Lessons**:
1. **Code duplication leads to bugs** - fixing bugs in one place doesn't fix them everywhere
2. **Data key consistency is critical** - small format differences cause data corruption
3. **Validation must be centralized** - distributed validation logic gets out of sync
4. **Component lifecycle matters** - missing OnInitialized() causes subtle bugs

**Process Lessons**:
1. **Regular architecture reviews** catch problems before they become critical
2. **Comprehensive refactoring** is better than piecemeal fixes
3. **Strong patterns prevent regressions** - rules must be enforced
4. **Documentation is essential** - patterns must be written down and followed

### Success Stories

**DynamicQuestionnaire.razor Refactoring**:
- **Before**: 400+ lines with inline question rendering
- **After**: 150 lines using OptimizedQuestionRenderer
- **Results**: 60% code reduction, zero bugs since refactoring

**AssessmentQuestion Consolidation**:
- **Before**: 3 different assessment rendering implementations
- **After**: Single OptimizedAssessmentQuestion component
- **Results**: Consistent rating behavior, unified validation logic

---

## Performance Considerations

### Component Rendering Performance

**Optimized Patterns**:
```csharp
// ✅ GOOD: Minimize re-renders with proper parameter comparison
protected override bool ShouldRender()
{
    return hasParametersChanged() || hasResponseDataChanged();
}

// ✅ GOOD: Efficient data key lookups
private object GetResponseValue(string key)
{
    return ResponseData.TryGetValue(key, out var value) ? value : null;
}
```

**Avoid Performance Anti-Patterns**:
```csharp
// ❌ BAD: Expensive operations in render methods
@foreach (var competency in GetCompetenciesFromConfiguration()) // Don't call in render
{
    // Render logic
}

// ✅ GOOD: Cache expensive operations
@foreach (var competency in cachedCompetencies) // Pre-computed in OnParametersSet
{
    // Render logic
}
```

### Memory Management

**ResponseData Management**:
- Use `Dictionary<string, object>` for flexibility
- Clear unused keys when configuration changes
- Don't hold references to old configurations

**Event Handler Cleanup**:
```csharp
public void Dispose()
{
    OnResponseChanged = null; // Prevent memory leaks
}
```

---

## Future Architectural Improvements

### Planned Enhancements

**1. Component Library Expansion**:
- Additional question types (file upload, signature, etc.)
- Rich text editor integration
- Advanced validation components

**2. Performance Optimizations**:
- Virtual scrolling for large questionnaires
- Lazy loading of question configurations
- Component-level caching strategies

**3. Developer Experience**:
- Visual Studio Code snippets for component creation
- Automated component generation from question type definitions
- Design system integration with Radzen components

### Architecture Evolution

**Current State** (2026-01-30):
- OptimizedQuestionRenderer system established
- Validation patterns standardized
- Data key formats unified

**Next Phase** (Planned):
- Extract shared UI patterns to design system
- Create component testing framework
- Implement automated accessibility testing

---

## References

### Component Locations

- **Master Renderer**: `05_Frontend/ti8m.BeachBreak.Client/Components/Questions/OptimizedQuestionRenderer.razor`
- **Question Components**: `05_Frontend/ti8m.BeachBreak.Client/Components/Questions/`
- **Configuration Services**: `05_Frontend/ti8m.BeachBreak.Client/Services/QuestionHandlers/`
- **Validation Services**: `05_Frontend/ti8m.BeachBreak.Client/Services/QuestionnaireValidationService.cs`

### Related Documentation

- **CLAUDE.md**: Core component architecture rules
- **docs/domain/questionnaire-workflows.md**: Workflow state integration
- **docs/implementation/translation-system.md**: Translation integration
- **Tests/README.md**: Component testing guidelines

### Key Files for Component Development

- `QuestionConfigurationService.cs`: Centralized configuration parsing
- `WorkflowStateHelper.cs`: State-based behavior logic
- `QuestionnaireValidationService.cs`: Validation logic
- `OptimizedQuestionRenderer.razor`: Master component template

---

*Last Updated: 2026-01-30*
*Document Version: 1.0*