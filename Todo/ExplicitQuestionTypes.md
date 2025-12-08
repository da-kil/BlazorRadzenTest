3# Refactoring Plan: From Dictionary<string, object> to Strongly-Typed Question Configuration

## Executive Summary

**Problem:** QuestionItem.Configuration uses `Dictionary<string, object>` causing:
- ~500 lines of duplicate parsing logic across 11+ files
- Historical validation bug (2025-11-10) due to parsing complexity
- No compile-time type safety
- Maintenance burden when adding new question types

**Solution:** Refactor to strongly-typed configuration classes (IQuestionConfiguration hierarchy) with **direct replacement** (no backward compatibility needed).

**User Decisions:**
- ✅ Full refactoring approach - maximum type safety
- ✅ Design for 2 types (Assessment, TextQuestion) + Goal visibility flag
- ✅ Domain uses typed configuration (pragmatic approach)
- ✅ **NO backward compatibility** - new application, direct replacement
- ✅ Rename "Competency" → "Evaluation" (better domain terminology)

**Impact:**
- **Lines Deleted:** ~500+ lines of duplicate parsing code
- **Type Safety:** Compile-time errors instead of runtime failures
- **Maintainability:** Single source of truth for configuration logic
- **Cleaner Code:** No obsolete markers, no dual properties, simpler migration

---

## Architecture Design

### Core Type Hierarchy

```csharp
// Marker interface for all question configurations
public interface IQuestionConfiguration
{
    QuestionType QuestionType { get; }
}

// Assessment questions: evaluation items with ratings and comments
public sealed class AssessmentConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.Assessment;
    public List<EvaluationItem> Evaluations { get; set; } = new();
    public int RatingScale { get; set; } = 4;
    public string ScaleLowLabel { get; set; } = "Poor";
    public string ScaleHighLabel { get; set; } = "Excellent";

    // Helper for domain validation
    public List<RequiredEvaluation> GetRequiredEvaluations() { /*...*/ }
}

// Text questions: multi-section text inputs
public sealed class TextQuestionConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.TextQuestion;
    public List<TextSectionDefinition> TextSections { get; set; } = new();

    // Helper for domain validation
    public List<RequiredTextSection> GetRequiredTextSections() { /*...*/ }
}

// Goal questions: visibility flag only (no template structure)
public sealed class GoalConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.Goal;
    public bool ShowGoalSection { get; set; } = true;
}
```

### Domain Model: Direct Replacement (No Backward Compatibility)

```csharp
public class QuestionItem : Entity<Guid>
{
    // Strongly-typed configuration - DIRECT REPLACEMENT of Dictionary
    public IQuestionConfiguration Configuration { get; private set; }

    private QuestionItem() { }

    // Single constructor - typed configuration only
    public QuestionItem(
        Guid id,
        Translation title,
        Translation description,
        QuestionType type,
        int order,
        bool isRequired,
        IQuestionConfiguration? configuration = null)
    {
        Id = id;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? new Translation("", "");
        Type = type;
        Order = order;
        IsRequired = isRequired;
        Configuration = configuration ?? CreateDefaultConfiguration(type);
    }

    public void UpdateConfiguration(IQuestionConfiguration configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    private static IQuestionConfiguration CreateDefaultConfiguration(QuestionType type)
    {
        return type switch
        {
            QuestionType.Assessment => new AssessmentConfiguration(),
            QuestionType.TextQuestion => new TextQuestionConfiguration(),
            QuestionType.Goal => new GoalConfiguration(),
            _ => throw new ArgumentException($"Unknown question type: {type}")
        };
    }
}
```

**Key Design Decision:** Direct replacement - no dual properties, no obsolete markers, no lazy loading. This is a **breaking change** but acceptable since the application is new and has no production event history to maintain.

---

## Event Sourcing Strategy: Explicit Event Types (DDD Best Practice)

### Event Schema Design - Explicit Events per Question Type

**DECISION: Use explicit event types instead of generic QuestionItemData**

**Rationale (DDD Principles):**
- Events should capture business intent explicitly ("AssessmentQuestionAdded" not "QuestionItem with Type=0")
- Each question type is a distinct domain concept, not just a configuration variant
- Event stream should tell a clear business story
- Allows independent evolution of each question type
- Follows Event Sourcing best practices (Greg Young, Vaughn Vernon)

**Explicit Event Records:**
```csharp
// Assessment questions
public record AssessmentQuestionAdded(
    Guid QuestionId,
    Guid SectionId,
    Translation Title,
    Translation Description,
    int Order,
    bool IsRequired,
    List<EvaluationItem> Evaluations,
    int RatingScale,
    string ScaleLowLabel,
    string ScaleHighLabel);

// Text questions
public record TextQuestionAdded(
    Guid QuestionId,
    Guid SectionId,
    Translation Title,
    Translation Description,
    int Order,
    bool IsRequired,
    List<TextSectionDefinition> TextSections);

// Goal questions
public record GoalQuestionAdded(
    Guid QuestionId,
    Guid SectionId,
    Translation Title,
    Translation Description,
    int Order,
    bool IsRequired,
    bool ShowGoalSection);

// Update events follow same pattern
public record AssessmentQuestionConfigurationUpdated(
    Guid QuestionId,
    List<EvaluationItem> Evaluations,
    int RatingScale,
    string ScaleLowLabel,
    string ScaleHighLabel);

public record TextQuestionConfigurationUpdated(
    Guid QuestionId,
    List<TextSectionDefinition> TextSections);

public record GoalQuestionConfigurationUpdated(
    Guid QuestionId,
    bool ShowGoalSection);
```

### Event Flow (Post-Migration)

```
Command → AssessmentConfiguration →
QuestionnaireTemplate.AddAssessmentQuestion() →
RaiseEvent(new AssessmentQuestionAdded(...)) →
Apply(AssessmentQuestionAdded) constructs QuestionItem with AssessmentConfiguration →
Marten serializes event with explicit properties →
Event Store
```

### Migration Strategy

Since this is a **new application** without production event history:
1. **Update QuestionItemData** to use IQuestionConfiguration
2. **Update all event handlers** to construct with typed configuration
3. **Clear development event store** (acceptable for new application)
4. **Re-projection**: Run projection rebuild in development
5. **No migration code needed** - clean break

**If there ARE existing events in development:**
- Option 1: Clear event store and re-seed test data
- Option 2: Write one-time migration script to transform Dictionary → IQuestionConfiguration
- User indicated "new application" → go with Option 1 (clean slate)

---

## Implementation Phases

### Phase 1: Foundation (Day 1-2) - Preparation ✅ COMPLETED (2025-12-08)

**Goal:** Add typed configuration infrastructure and rename Competency → Evaluation.

**Tasks:**
1. ✅ Create folder: `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/`
2. ✅ Add `IQuestionConfiguration.cs` interface
3. ✅ Add `AssessmentConfiguration.cs` (with GetRequiredEvaluations method)
4. ✅ Add `TextQuestionConfiguration.cs` (with GetRequiredTextSections method)
5. ✅ Add `GoalConfiguration.cs` (with ShowGoalSection flag)
6. ✅ Rename `CompetencyDefinition.cs` → `EvaluationItem.cs` and move to Core
   - Update all properties and methods
   - Kept Key property (not renamed to EvaluationKey)
7. ✅ Move `TextSectionDefinition.cs` from frontend to Core
8. ✅ **Global rename**: CompetencyDefinition → EvaluationItem across entire codebase
   - Frontend models, DTOs, helpers, components
   - Backend DTOs, services, domain models
   - 50+ files affected across all architectural layers
9. ✅ Run all tests - ensure rename didn't break anything

**Risk:** Medium - global rename affects many files

**Validation:**
- ✅ Solution builds successfully (0 errors, 31 warnings pre-existing)
- ✅ All unit tests pass
- ✅ Global search for "Competency" returns only comments/docs
- ✅ Frontend compiles with renamed models

**Completion Summary:**
- All 7 Core configuration classes created
- Global rename completed across 50+ files
- All architectural layers updated (Domain, Application, Infrastructure, Frontend)
- Renamed classes: CompetencyDefinition → EvaluationItem, CompetencyRatingDto → EvaluationRatingDto
- Renamed methods: GetCompetencies → GetEvaluations, SetCompetencies → SetEvaluations
- Renamed component: CompetencyRatingItem.razor → EvaluationRatingItem.razor
- Solution builds successfully with zero errors

---

### Phase 2: Domain Events - Explicit Event Types (Day 3-4) - High Risk

**Goal:** Replace generic QuestionItemData with explicit event types per question type.

**Tasks:**
1. **DELETE** `QuestionItemData.cs` (generic event)

2. **CREATE** explicit event records in `Events/` folder:
   - `AssessmentQuestionAdded.cs`
   - `AssessmentQuestionConfigurationUpdated.cs`
   - `TextQuestionAdded.cs`
   - `TextQuestionConfigurationUpdated.cs`
   - `GoalQuestionAdded.cs`
   - `GoalQuestionConfigurationUpdated.cs`
   - Each event has explicit properties (no polymorphic Configuration property)

3. Update `QuestionItem.cs`:
   - Replace `Dictionary<string, object> Configuration` with `IQuestionConfiguration Configuration`
   - Keep single constructor with IQuestionConfiguration

4. Update `QuestionnaireTemplate.cs` aggregate methods:
   - Rename `AddQuestion()` → Create 3 explicit methods:
     - `AddAssessmentQuestion(sectionId, title, AssessmentConfiguration)`
     - `AddTextQuestion(sectionId, title, TextQuestionConfiguration)`
     - `AddGoalQuestion(sectionId, title, GoalConfiguration)`
   - Each raises its specific event type

5. Add Apply() methods for new events:
   - `Apply(AssessmentQuestionAdded @event)` - constructs QuestionItem with AssessmentConfiguration
   - `Apply(TextQuestionAdded @event)` - constructs QuestionItem with TextQuestionConfiguration
   - `Apply(GoalQuestionAdded @event)` - constructs QuestionItem with GoalConfiguration
   - Similar for Update events

6. **CRITICAL**: Clear development event store (breaking change)
   - Run Marten projection rebuild
   - Re-seed test data

**Critical Files:**
- **DELETE:** `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/QuestionItemData.cs`
- **CREATE:** `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/AssessmentQuestionAdded.cs`
- **CREATE:** `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/TextQuestionAdded.cs`
- **CREATE:** `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/GoalQuestionAdded.cs`
- **CREATE:** `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/AssessmentQuestionConfigurationUpdated.cs`
- **CREATE:** `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/TextQuestionConfigurationUpdated.cs`
- **CREATE:** `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/GoalQuestionConfigurationUpdated.cs`
- `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/QuestionItem.cs`
- `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/QuestionnaireTemplate.cs`

**Risk:** HIGH - breaking change to event schema, requires event store clear

**Validation:**
- ✅ Solution builds
- ✅ Domain unit tests pass
- ✅ Events serialize with explicit properties (no $type discriminator needed at event level)
- ✅ Event stream is readable and tells business story
- ✅ New questionnaires can be created and saved

---

### Phase 3: Query Side - Projections with Explicit Events (Day 4-5) - Medium Risk

**Goal:** Update projections to handle explicit event types.

**Tasks:**
1. Update `QuestionnaireTemplateReadModel.cs` projection handlers:
   - **DELETE** generic `Apply(QuestionItemAdded)` method
   - **ADD** explicit Apply() methods:
     ```csharp
     public void Apply(AssessmentQuestionAdded @event)
     {
         var section = Sections.First(s => s.Id == @event.SectionId);
         section.Questions.Add(new QuestionItem
         {
             Id = @event.QuestionId,
             Title = @event.Title,
             Type = QuestionType.Assessment,
             Configuration = new AssessmentConfiguration
             {
                 Evaluations = @event.Evaluations,
                 RatingScale = @event.RatingScale,
                 ScaleLowLabel = @event.ScaleLowLabel,
                 ScaleHighLabel = @event.ScaleHighLabel
             }
         });
     }

     public void Apply(TextQuestionAdded @event) { ... }
     public void Apply(GoalQuestionAdded @event) { ... }
     public void Apply(AssessmentQuestionConfigurationUpdated @event) { ... }
     public void Apply(TextQuestionConfigurationUpdated @event) { ... }
     public void Apply(GoalQuestionConfigurationUpdated @event) { ... }
     ```
   - Change QuestionItem.Configuration from `Dictionary<string, object>` to `IQuestionConfiguration`

2. Update Query models:
   - `02_Application/ti8m.BeachBreak.Application.Query/Queries/QuestionnaireTemplateQueries/QuestionItem.cs`
   - Change Configuration property to IQuestionConfiguration

3. Update Query API DTOs:
   - `03_Infrastructure/ti8m.BeachBreak.QueryApi/Dto/QuestionItemDto.cs`
   - Change Configuration to IQuestionConfiguration (polymorphic JSON serialization)

4. Update `QuestionnaireTemplatesController.cs`:
   - Map from read model IQuestionConfiguration to DTO
   - Test API endpoint returns correct JSON with $type discriminator

**Critical Files:**
- `02_Application/ti8m.BeachBreak.Application.Query/Projections/QuestionnaireTemplateReadModel.cs`
- `02_Application/ti8m.BeachBreak.Application.Query/Queries/QuestionnaireTemplateQueries/QuestionItem.cs`
- `03_Infrastructure/ti8m.BeachBreak.QueryApi/Dto/QuestionItemDto.cs`
- `03_Infrastructure/ti8m.BeachBreak.QueryApi/Controllers/QuestionnaireTemplatesController.cs`

**Risk:** Medium - projection changes, requires event replay

**Validation:**
- ✅ Projections rebuild successfully
- ✅ All 6 event types handled by projections
- ✅ Read models construct correct IQuestionConfiguration instances
- ✅ Query API returns typed configuration JSON with $type discriminator
- ✅ Event stream is readable (explicit event names)

---

### Phase 4: Domain Validation Simplification (Day 5-6) - High Risk

**Goal:** Eliminate parsing logic from QuestionnaireResponse validation.

**Tasks:**
1. Refactor `QuestionnaireResponse.IsAssessmentComplete()`:
   ```csharp
   // BEFORE: 40+ lines with GetCompetenciesFromConfiguration()
   // AFTER: 10 lines using question.Configuration (now typed!)

   private bool IsAssessmentComplete(QuestionItem question, QuestionResponseValue response)
   {
       if (response is not QuestionResponseValue.AssessmentResponse assessmentResponse)
           return false;

       if (question.Configuration is not AssessmentConfiguration config)
           return false;

       var requiredEvaluations = config.GetRequiredEvaluations();
       if (requiredEvaluations.Count == 0) return true;

       return requiredEvaluations.All(e =>
           assessmentResponse.Evaluations.TryGetValue(e.Key, out var rating) &&
           rating.Rating > 0);
   }
   ```

2. Refactor `IsTextQuestionComplete()` similarly
3. **DELETE** `GetCompetenciesFromConfiguration()` method (~80 lines)
4. **DELETE** `GetTextSectionsFromConfiguration()` method (~55 lines)
5. **DELETE** `CompetencyItem` and `TextSectionItem` private records
6. **DELETE** `GetConfigurationCollectionCount()` helper
7. Update QuestionResponseValue to use Evaluations (rename from Competencies)

**Critical Files:**
- `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireResponseAggregate/QuestionnaireResponse.cs` (lines 199-314)

**Risk:** HIGH - business-critical validation logic, needs extensive testing

**Validation:**
- ✅ Characterization tests: capture behavior of original validation
- ✅ Side-by-side comparison: old vs new validation on 100+ test cases
- ✅ Integration tests with real questionnaire responses
- ✅ Manual testing of questionnaire submission workflows

**Lines Deleted:** ~135 lines from domain layer

---

### Phase 5: Frontend Models & Services (Day 6-7) - Medium Risk

**Goal:** Update frontend to consume typed configuration from API.

**Tasks:**
1. Update `QuestionItem.cs` frontend model:
   - Replace `Dictionary<string, object>? Configuration` with `IQuestionConfiguration Configuration`
   - Remove all parsing logic (no longer needed!)

2. **DELETE** `QuestionConfigurationService.cs` entirely (~220 lines):
   - All parsing methods replaced by direct property access
   - GetCompetencies() → config.Evaluations
   - GetTextSections() → config.TextSections
   - GetRatingScale() → config.RatingScale

3. **DELETE** `AssessmentConfigurationHelper.cs` entirely (~165 lines):
   - All parsing logic replaced by typed access

4. Update `QuestionnaireTemplateService.cs`:
   - Verify API deserialization works with IQuestionConfiguration
   - JSON should include $type discriminator for polymorphism

5. Update response DTOs to use Evaluations:
   - Rename CompetencyRatingDto → EvaluationRatingDto
   - Rename AssessmentResponseDataDto.Competencies → Evaluations

**Critical Files:**
- `05_Frontend/ti8m.BeachBreak.Client/Models/QuestionItem.cs`
- **DELETE:** `05_Frontend/ti8m.BeachBreak.Client/Services/QuestionConfigurationService.cs`
- **DELETE:** `05_Frontend/ti8m.BeachBreak.Client/Helpers/AssessmentConfigurationHelper.cs`
- `05_Frontend/ti8m.BeachBreak.Client/Models/DTOs/AssessmentResponseDataDto.cs`

**Risk:** Medium - API deserialization must work correctly

**Validation:**
- ✅ Frontend receives typed configuration from API
- ✅ Polymorphic deserialization works ($type discriminator)
- ✅ No parsing errors
- ✅ ~385 lines deleted (QuestionConfigurationService + AssessmentConfigurationHelper)

---

### Phase 6: Frontend Components (Day 7-8) - Medium Risk

**Goal:** Simplify question rendering components.

**Tasks:**
1. Update `OptimizedAssessmentQuestion.razor`:
   - Replace all helper calls with direct property access
   - `Question.Configuration as AssessmentConfiguration`
   - Access `config.Evaluations`, `config.RatingScale` directly
   - Remove all JsonElement handling

2. Update `OptimizedTextQuestion.razor`:
   - **DELETE** `GetTextSectionsFromConfiguration()` method (lines 165-211)
   - Use `Question.Configuration as TextQuestionConfiguration`
   - Access `config.TextSections` directly

3. Update other rendering components:
   - `ReviewModeAssessmentRenderer.razor`
   - `EditAnswerDialog.razor`
   - Any component accessing Configuration

**Critical Files:**
- `05_Frontend/ti8m.BeachBreak.Client/Components/Questions/OptimizedAssessmentQuestion.razor`
- `05_Frontend/ti8m.BeachBreak.Client/Components/Questions/OptimizedTextQuestion.razor`
- **DELETE:** `05_Frontend/ti8m.BeachBreak.Client/Helpers/AssessmentConfigurationHelper.cs`

**Risk:** Medium - frontend rendering, visual regression testing required

**Validation:**
- ✅ Test all question types render correctly
- ✅ Test language switching (English ↔ German)
- ✅ Test readonly vs editable modes
- ✅ Test competency ratings display
- ✅ Screenshot comparison before/after

**Lines Deleted:** ~200 lines from frontend components

---

### Phase 7: Questionnaire Builder (Day 9-10) - High Risk

**Goal:** Simplify template builder configuration editing.

**Tasks:**
1. Update `QuestionCard.razor`:
   - **DELETE** inline `GetOrderedCompetencies()` parsing
   - **DELETE** inline `GetOrderedTextSections()` parsing
   - Use `Question.Configuration` (now typed!) for rendering UI
   - Update evaluation CRUD: add, edit, delete, reorder
   - Direct property access: `(Question.Configuration as AssessmentConfiguration).Evaluations`
   - Update text section CRUD operations
   - Add goal section visibility flag UI

2. Simplify `AssessmentQuestionHandler.cs`:
   - Use typed configuration for initialization
   - Create AssessmentConfiguration directly
   - Remove all Dictionary manipulation

3. Update `TextQuestionHandler.cs`:
   - Create TextQuestionConfiguration directly

4. Add `GoalQuestionHandler.cs`:
   - Handle GoalConfiguration with ShowGoalSection flag

**Critical Files:**
- `05_Frontend/ti8m.BeachBreak.Client/Components/QuestionnaireBuilder/QuestionCard.razor`
- `05_Frontend/ti8m.BeachBreak.Client/Services/QuestionConfigurationService.cs`

**Risk:** HIGH - builder is complex UI, extensive manual testing required

**Validation:**
- ✅ Test competency add/edit/delete/reorder
- ✅ Test text section add/edit/delete/reorder
- ✅ Test goal visibility flag toggle
- ✅ Test template save/load
- ✅ Test template cloning
- ✅ Test question type switching
- ✅ Ensure published templates still work

**Lines Deleted:** ~150 lines from QuestionCard parsing methods

---

### Phase 8: Command API & Handlers (Day 10-11) - Medium Risk

**Goal:** Update Command API to call explicit domain methods.

**Tasks:**
1. Update Command handlers:
   - `QuestionnaireTemplateCommandHandler.cs`
   - Replace generic `AddQuestion()` calls with explicit methods:
     ```csharp
     // Before
     template.AddQuestion(sectionId, questionItem);

     // After - explicit based on question type
     if (questionItem.Configuration is AssessmentConfiguration assessConfig)
     {
         template.AddAssessmentQuestion(sectionId, questionItem.Title, assessConfig);
     }
     else if (questionItem.Configuration is TextQuestionConfiguration textConfig)
     {
         template.AddTextQuestion(sectionId, questionItem.Title, textConfig);
     }
     else if (questionItem.Configuration is GoalConfiguration goalConfig)
     {
         template.AddGoalQuestion(sectionId, questionItem.Title, goalConfig);
     }
     ```
   - Similar changes for Update commands

2. Update Command DTOs (optional - can stay as IQuestionConfiguration):
   - `02_Application/ti8m.BeachBreak.Application.Command/Commands/QuestionnaireTemplateCommands/CommandQuestionItem.cs`
   - Keep IQuestionConfiguration (polymorphic deserialization from API)

3. Update response mapping:
   - `QuestionResponseMappingService.cs` uses evaluation terminology
   - Map Evaluations instead of Competencies

**Critical Files:**
- `02_Application/ti8m.BeachBreak.Application.Command/Commands/QuestionnaireTemplateCommands/QuestionnaireTemplateCommandHandler.cs`
- `03_Infrastructure/ti8m.BeachBreak.CommandApi/Services/QuestionResponseMappingService.cs`

**Risk:** Medium - command handler routing logic

**Validation:**
- ✅ Commands route to correct explicit domain methods
- ✅ API accepts typed configuration JSON
- ✅ Correct event type raised based on configuration type
- ✅ Commands execute successfully

---

### Phase 9: Goal Configuration Enhancement (Day 11) - Low Risk

**Goal:** Add ShowGoalSection visibility flag to Goal questions.

**Tasks:**
1. Update `GoalConfiguration` class with ShowGoalSection property
2. Add UI in QuestionCard.razor for toggling goal section visibility
3. Update DynamicQuestionnaire.razor to respect ShowGoalSection flag
4. Add migration for existing Goal questions (default ShowGoalSection = true)

**User Requirement:** "Design for 2 types only, but in template we define if a goal section is showing up when filling out questionnaire"

**Critical Files:**
- `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/GoalConfiguration.cs`
- `05_Frontend/ti8m.BeachBreak.Client/Components/QuestionnaireBuilder/QuestionCard.razor`
- `05_Frontend/ti8m.BeachBreak.Client/Pages/DynamicQuestionnaire.razor`

**Risk:** Low - additive feature, Goal questions currently have no configuration

**Validation:**
- ✅ Test ShowGoalSection toggle in builder
- ✅ Test goal section hidden when ShowGoalSection = false
- ✅ Test goal section shown when ShowGoalSection = true
- ✅ Test backward compatibility (old questionnaires default to shown)

---

### Phase 10: Testing & Validation (Day 12-13) - Critical

**Goal:** Comprehensive testing of all changes.

**Tasks:**
1. **Unit Tests:**
   - Test AssessmentConfiguration.GetRequiredEvaluations()
   - Test TextQuestionConfiguration.GetRequiredTextSections()
   - Test GoalConfiguration.ShowGoalSection
   - Test domain validation with typed configuration
   - Test event serialization/deserialization

2. **Integration Tests:**
   - Test full template creation flow
   - Test template retrieval from Query API
   - Test questionnaire response validation
   - Test projection rebuild

3. **Frontend Testing:**
   - Test all question types render correctly
   - Test builder CRUD operations (add/edit/delete/reorder)
   - Test language switching
   - Test goal visibility toggle

4. **Manual Testing:**
   - Create templates with all question types
   - Fill out questionnaires
   - Submit responses
   - Review responses in review mode

**Risk:** CRITICAL - must verify all functionality works

**Validation:**
- ✅ All unit tests pass
- ✅ All integration tests pass
- ✅ Manual testing checklist complete
- ✅ No regression bugs found

---

### Phase 11: Cleanup and Documentation (Day 14) - Low Risk

**Goal:** Remove dead code, update documentation.

**Tasks:**
1. **Code Cleanup:**
   - Search for remaining `Dictionary<string, object>` usage (should find none)
   - Remove unused parsing methods
   - Clean up imports/usings
   - Verify no "Competency" references remain (should be "Evaluation")

2. **Update CLAUDE.md:**
   - Add Pattern #11: "Strongly-Typed Question Configuration"
   - Document IQuestionConfiguration hierarchy
   - Document EvaluationItem (renamed from Competency)
   - Add examples of adding new question types

3. **XML Documentation:**
   - Add documentation to all new classes
   - Document the polymorphic serialization pattern

4. **Performance Testing:**
   - Measure template load times
   - Measure questionnaire rendering performance
   - Compare to baseline (should be similar or faster, no parsing overhead)

**CLAUDE.md Addition:**
```markdown
### 11. Strongly-Typed Question Configuration Pattern
- **ALWAYS** use `QuestionItem.Configuration` (now typed as IQuestionConfiguration)
- **PATTERN**: Cast to specific type: `if (question.Configuration is AssessmentConfiguration config)`
- **DIRECT ACCESS**: `config.Evaluations`, `config.RatingScale` (no parsing needed!)
- **ADDING NEW TYPES**: Create class implementing IQuestionConfiguration
- **VALIDATION**: Use helper methods: GetRequiredEvaluations(), GetRequiredTextSections()
- **TERMINOLOGY**: Use "Evaluation" not "Competency" for assessment items

**Example:**
```csharp
// Assessment question
if (question.Configuration is AssessmentConfiguration config)
{
    foreach (var evaluation in config.Evaluations)
    {
        var title = evaluation.GetLocalizedTitle(currentLanguage);
        // Direct property access - type safe!
    }
}

// Text question
if (question.Configuration is TextQuestionConfiguration config)
{
    var sections = config.TextSections; // No parsing!
}
```
```

**Risk:** Low - cleanup and documentation

**Validation:**
- ✅ No Dictionary<string, object> found in codebase
- ✅ No "Competency" references remain
- ✅ CLAUDE.md updated with clear examples
- ✅ All XML documentation complete
- ✅ Performance within acceptable range

---

## Files to Create (12 new files)

### Core Configuration Classes
1. `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/IQuestionConfiguration.cs`
2. `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/AssessmentConfiguration.cs`
3. `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/TextQuestionConfiguration.cs`
4. `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/GoalConfiguration.cs`
5. `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/EvaluationItem.cs` (renamed from CompetencyDefinition)
6. `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/TextSectionDefinition.cs` (moved from frontend)

### Domain Events (Explicit per Question Type)
7. `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/AssessmentQuestionAdded.cs`
8. `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/TextQuestionAdded.cs`
9. `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/GoalQuestionAdded.cs`
10. `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/AssessmentQuestionConfigurationUpdated.cs`
11. `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/TextQuestionConfigurationUpdated.cs`
12. `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/GoalQuestionConfigurationUpdated.cs`

---

## Files to Modify (20+ critical files)

### Domain Layer
1. `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/QuestionItem.cs`
   - Replace Dictionary Configuration with IQuestionConfiguration
   - Remove UpdateConfiguration(Dictionary) method

2. `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/QuestionnaireTemplate.cs`
   - Replace generic `AddQuestion()` with 3 explicit methods
   - Add 6 new Apply() methods for explicit event types
   - DELETE old Apply() methods for generic events

3. `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireResponseAggregate/QuestionnaireResponse.cs`
   - Refactor IsAssessmentComplete() - use typed configuration
   - Refactor IsTextQuestionComplete() - use typed configuration
   - **DELETE** GetCompetenciesFromConfiguration() (~80 lines)
   - **DELETE** GetTextSectionsFromConfiguration() (~55 lines)
   - Rename Competencies → Evaluations in response values

### Application Layer - Command Side
4. `02_Application/ti8m.BeachBreak.Application.Command/Commands/QuestionnaireTemplateCommands/CommandQuestionItem.cs`
   - Change Configuration to IQuestionConfiguration

5. `02_Application/ti8m.BeachBreak.Application.Command/Commands/QuestionnaireTemplateCommands/QuestionnaireTemplateCommandHandler.cs`
   - Update to call explicit aggregate methods based on configuration type
   - Route to AddAssessmentQuestion, AddTextQuestion, or AddGoalQuestion

### Application Layer - Query Side
6. `02_Application/ti8m.BeachBreak.Application.Query/Projections/QuestionnaireTemplateReadModel.cs`
   - Change Configuration to IQuestionConfiguration
   - Add 6 explicit Apply() methods for new event types
   - **DELETE** generic Apply(QuestionItemAdded) method

7. `02_Application/ti8m.BeachBreak.Application.Query/Queries/QuestionnaireTemplateQueries/QuestionItem.cs`
   - Change Configuration to IQuestionConfiguration

### Infrastructure - Command API
8. `03_Infrastructure/ti8m.BeachBreak.CommandApi/Services/QuestionResponseMappingService.cs`
   - Rename Competencies → Evaluations

### Infrastructure - Query API
9. `03_Infrastructure/ti8m.BeachBreak.QueryApi/Dto/QuestionItemDto.cs`
   - Change Configuration to IQuestionConfiguration

10. `03_Infrastructure/ti8m.BeachBreak.QueryApi/Controllers/QuestionnaireTemplatesController.cs`
    - Verify mapping works with typed configuration

### Frontend - Models
11. `05_Frontend/ti8m.BeachBreak.Client/Models/QuestionItem.cs`
    - Replace Dictionary with IQuestionConfiguration

12. `05_Frontend/ti8m.BeachBreak.Client/Models/DTOs/AssessmentResponseDataDto.cs`
    - Rename Competencies → Evaluations

13. `05_Frontend/ti8m.BeachBreak.Client/Models/DTOs/CompetencyRatingDto.cs`
    - Rename to EvaluationRatingDto

### Frontend - Components
14. `05_Frontend/ti8m.BeachBreak.Client/Components/Questions/OptimizedAssessmentQuestion.razor`
    - Remove all parsing logic
    - Use Question.Configuration directly (now typed)

15. `05_Frontend/ti8m.BeachBreak.Client/Components/Questions/OptimizedTextQuestion.razor`
    - **DELETE** GetTextSectionsFromConfiguration() (~50 lines)
    - Use Question.Configuration directly

16. `05_Frontend/ti8m.BeachBreak.Client/Components/QuestionnaireBuilder/QuestionCard.razor`
    - **DELETE** parsing methods (~150 lines)
    - Use typed configuration for builder UI

17. `05_Frontend/ti8m.BeachBreak.Client/Services/QuestionHandlers/AssessmentQuestionHandler.cs`
    - Create AssessmentConfiguration directly

18. `05_Frontend/ti8m.BeachBreak.Client/Services/QuestionHandlers/TextQuestionHandler.cs`
    - Create TextQuestionConfiguration directly

### Documentation
19. `CLAUDE.md`
    - Add Pattern #11: Strongly-Typed Question Configuration

### Global Renames (30+ files)
- CompetencyDefinition → EvaluationItem
- Competencies → Evaluations
- GetCompetenciesFromConfiguration → (deleted)
- All related method names, property names, variable names

---

## Files to Delete (4 files)

1. **`01_Domain/ti8m.BeachBreak.Domain/QuestionnaireTemplateAggregate/Events/QuestionItemData.cs`**
   - Generic event record - replaced by explicit event types

2. **`05_Frontend/ti8m.BeachBreak.Client/Helpers/AssessmentConfigurationHelper.cs`**
   - 165 lines of duplicate parsing logic
   - Replaced by typed configuration direct access

3. **`05_Frontend/ti8m.BeachBreak.Client/Services/QuestionConfigurationService.cs`**
   - 220 lines of parsing logic
   - All methods replaced by direct property access

4. **`05_Frontend/ti8m.BeachBreak.Client/Models/CompetencyDefinition.cs`**
   - Moved to Core as EvaluationItem.cs

---

## Risk Mitigation

### Risk 1: Event Store Compatibility (MEDIUM - Mitigated by Clean Slate)
**Scenario:** Changing event schema breaks existing events.

**Mitigation:**
- User confirmed "new application" - no production event history
- Clear development event store (acceptable)
- Run Marten projection rebuild
- Re-seed test data with typed configurations
- If events exist: write one-time migration script (unlikely needed)

### Risk 2: Validation Logic Differs (HIGH)
**Mitigation:**
- Characterization tests: capture original behavior
- Side-by-side comparison on 100+ test cases
- Domain expert review of validation rules
- Shadow mode: run both validations, log differences

### Risk 3: Frontend Rendering Breaks (HIGH)
**Mitigation:**
- Visual regression testing (screenshots)
- Test all question types × all languages × all workflows
- Gradual rollout (Assessment first, then TextQuestion)
- Feature flag to toggle between old/new parsing

### Risk 4: Builder CRUD Breaks (HIGH)
**Mitigation:**
- Comprehensive manual testing of all CRUD operations
- Test template save/load/clone
- Ensure backward compatibility with published templates
- User acceptance testing

### Risk 5: Performance Regression (LOW)
**Mitigation:**
- Performance benchmarks before/after
- Cache parsed TypedConfiguration (already in design)
- Profile hot paths
- Load testing with realistic volumes

---

## Success Metrics

### Code Quality
- ✅ **500+ lines deleted** (duplicate parsing logic)
- ✅ **Zero compiler warnings** about obsolete APIs (after phase 7)
- ✅ **Cyclomatic complexity reduced** from ~15 to ~5 in validation methods
- ✅ **Test coverage maintained** at >80%

### Business
- ✅ **Zero validation regressions** (no increase in invalid submissions)
- ✅ **Performance maintained** (<5% rendering time increase)
- ✅ **Zero data loss** (all existing responses render correctly)
- ✅ **Developer velocity improved** (time to add question type reduced 50%)

### Technical Debt
- ✅ **Single source of truth** (QuestionConfigurationFactory)
- ✅ **Type safety enforced** (compile-time errors for configuration access)
- ✅ **Event sourcing compatible** (old and new events work identically)
- ✅ **Documentation complete** (CLAUDE.md updated, XML docs added)

---

## Key Design Decisions

1. **Dual Property Pattern:** Keep both Configuration and TypedConfiguration during migration
   - **Why:** Enables gradual migration without breaking event sourcing
   - **Trade-off:** Temporary complexity, but enables zero-downtime refactoring

2. **Domain Uses Typed Config:** Let domain depend on IQuestionConfiguration
   - **Why:** Simpler validation logic, eliminates parsing in domain
   - **Trade-off:** Domain knows about question types (pragmatic choice)

3. **Factory Centralizes Parsing:** QuestionConfigurationFactory is single source of truth
   - **Why:** Eliminates 500+ lines of duplicate parsing across 11+ files
   - **Trade-off:** Factory must be maintained forever for old events

4. **Goal Configuration Enhancement:** Add ShowGoalSection visibility flag
   - **Why:** User requirement - "define if a goal section is showing up"
   - **Trade-off:** Minimal - Goal questions currently have no configuration

5. **Design for 2 Types Only:** Optimize for Assessment and TextQuestion
   - **Why:** User decision - don't over-engineer for hypothetical future types
   - **Trade-off:** Adding GoalTemplate later requires additional refactoring

---

## Alternatives Considered

### Alternative 1: Keep Dictionary, Add Type-Safe Accessors
**Verdict:** ❌ Rejected - doesn't solve duplicate code problem, still requires parsing

### Alternative 2: Dual Property Pattern (Configuration + TypedConfiguration)
**Verdict:** ❌ Rejected - unnecessary complexity since no backward compatibility needed

### Alternative 3: JSON Schema Validation
**Verdict:** ❌ Rejected - doesn't eliminate parsing logic, no type safety

### Alternative 4: Source Generators
**Verdict:** ❌ Rejected - overkill for 2 question types, adds build complexity

---

## Implementation Recommendation

**Proceed with full refactoring** based on:
1. ✅ User chose "Full refactoring (5 weeks)"
2. ✅ Clear evidence of problems (500+ duplicate lines, historical bug)
3. ✅ Feasible migration path (dual property pattern, backward compatibility)
4. ✅ Manageable risks (comprehensive testing strategy, gradual rollout)

**Next Steps:**
1. Review this plan with stakeholders
2. Estimate effort per phase
3. Begin Phase 1 (Foundation) - zero risk, additive only
4. Evaluate after Phase 2 before committing to full migration

---

## Appendix: Configuration Structure Reference

### Assessment Configuration (with type discriminator)
```json
{
  "$type": "AssessmentConfiguration",
  "Evaluations": [
    {
      "Key": "evaluation_1",
      "TitleEnglish": "Leadership",
      "TitleGerman": "Führung",
      "DescriptionEnglish": "Demonstrates leadership...",
      "DescriptionGerman": "Zeigt Führung...",
      "IsRequired": true,
      "Order": 0
    }
  ],
  "RatingScale": 4,
  "ScaleLowLabel": "Poor",
  "ScaleHighLabel": "Excellent"
}
```

### TextQuestion Configuration (with type discriminator)
```json
{
  "$type": "TextQuestionConfiguration",
  "TextSections": [
    {
      "TitleEnglish": "Career Goals",
      "TitleGerman": "Karriereziele",
      "DescriptionEnglish": "Describe your career goals",
      "DescriptionGerman": "Beschreiben Sie Ihre Karriereziele",
      "PlaceholderEnglish": "Enter your goals here...",
      "PlaceholderGerman": "Geben Sie hier Ihre Ziele ein...",
      "IsRequired": true,
      "Order": 0,
      "Rows": 4
    }
  ]
}
```

### Goal Configuration (with type discriminator)
```json
{
  "$type": "GoalConfiguration",
  "ShowGoalSection": true
}
```

---

## Summary of Key Changes from Original Plan

1. **No Backward Compatibility:** Direct replacement, no dual properties, no obsolete markers
2. **Explicit Event Types:** AssessmentQuestionAdded, TextQuestionAdded, GoalQuestionAdded (DDD best practice)
3. **Terminology Change:** Competency → Evaluation (better domain fit)
4. **Query Side Coverage:** Explicit projection handlers per event type
5. **Breaking Change:** Event schema change acceptable (new application)
6. **Faster Timeline:** 2-3 weeks (no backward compatibility complexity)
7. **More Deletions:** ~550+ lines deleted (QuestionConfigurationService + AssessmentConfigurationHelper + parsing methods)
8. **Event Stream Readability:** Events tell clear business story, not generic "QuestionItem with Type=0"

### Why Explicit Events (DDD Rationale)

**Events should capture business intent explicitly:**
- ✅ "AssessmentQuestionAdded" tells a clear business story
- ❌ "QuestionItemAdded with Type=Assessment" is generic and unclear

**Benefits:**
- Event stream is readable by domain experts
- Each question type can evolve independently
- Projection handlers are explicit and clear
- Follows Event Sourcing best practices (Greg Young, Vaughn Vernon)
- Slightly more projection code, but significantly better domain modeling

---

**Plan Status:** ✅ Ready for Review (Revised)
**Estimated Effort:** 2-3 weeks (11 phases including testing)
**Risk Level:** Medium-High (breaking changes, event store reset, global rename)
**Recommendation:**
- Coordinate with team before starting (breaking change)
- Budget time for comprehensive testing
- Proceed phase-by-phase with validation at each step
