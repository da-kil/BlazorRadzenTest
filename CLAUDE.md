# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is ti8m BeachBreak, a .NET 9 application implementing a CQRS/Event Sourcing architecture for questionnaire management. The application uses a layered architecture with clear separation between Domain, Application, Infrastructure, and Frontend layers.

## Architecture

### Layered Structure
- **01_Domain**: Domain models, aggregates, events, and business logic
- **02_Application**: Commands, queries, handlers (separated into Command and Query sides)
- **03_Infrastructure**: APIs, data access, external concerns
- **04_Core**: Shared building blocks and infrastructure abstractions
- **05_Frontend**: Blazor WebAssembly frontend with Radzen UI components
- **Aspire**: .NET Aspire orchestration for local development

### Key Patterns
- **Event Sourcing**: Domain events are the source of truth, stored via Marten/PostgreSQL
- **CQRS**: Separate command and query sides with independent APIs
- **Domain-Driven Design**: Aggregates, entities, value objects, and domain events
- **Clean Architecture**: Dependencies point inward toward the domain

### Technology Stack
- .NET 9
- Blazor WebAssembly with Radzen UI
- PostgreSQL with Marten for event sourcing
- .NET Aspire for local development orchestration
- Separate Command and Query APIs

### Authentication & Authorization Architecture
**CRITICAL: Roles are NOT in JWT tokens - they are looked up from database**

- **JWT Tokens**: Contain only user ID (`oid` claim) from Entra ID
- **ApplicationRole Storage**: Stored in EmployeeReadModel database table
- **Authorization Flow**:
  1. JWT token provides user ID (oid claim)
  2. Authorization middleware queries database: `SELECT ApplicationRole FROM EmployeeReadModel WHERE Id = <user_id>`
  3. Middleware adds ApplicationRole as claim to request context
  4. Frontend `AuthorizeView` policies use the added ApplicationRole claim
  5. API controllers use `[Authorize(Policy = "PolicyName")]` attributes

- **Role Enum Values**: Employee=0, TeamLead=1, HR=2, HRLead=3, Admin=4
- **Benefits**: Real-time role changes without token refresh, centralized role management
- **Debugging Auth Issues**: Check database records, NOT JWT token claims
- **Common Mistake**: Assuming roles are in JWT - they're dynamically looked up per request

## Domain Model

### Aggregates
- **Category**: Core entity with multilingual support via Translation value object
- **Employee**: Employee management (referenced in EmployeePrompt.md)
- **Questionnaire**: Template and assignment management

## Domain Events Guidelines

### Event Characteristics
- **Past tense naming**: Use past participle forms (e.g., "Order Placed", "Payment Processed", "Customer Registered")
- **Business significance**: Focus on events that matter to domain experts and stakeholders
- **Rich semantics**: Prefer events that capture business meaning over technical CRUD operations
- **Stakeholder relevance**: Consider what different actors in the domain would care about

### Event Quality Principles
**PREFER Rich Events:**
- "Order Shipped with Express Delivery"
- "Customer Loyalty Status Upgraded"
- "Product Inventory Critically Low"
- "Payment Failed Due to Insufficient Funds"
 
**AVOID Anemic/CRUD Events:**
- "Record Created"
- "Data Updated"
- "Field Changed"
- "Status Modified"

### Value Objects
- **Translation**: Multilingual text with German and English properties

### Event Sourcing Implementation
- All aggregates inherit from `AggregateRoot` which tracks domain events
- Events are applied via reflection using the Apply pattern
- Domain events implement `IDomainEvent` interface
- Version tracking for optimistic concurrency control

## Development Commands

### Build and Run
```bash
# Build entire solution
dotnet build ti8m.BeachBreak.sln

# Run with Aspire (recommended for local development)
cd Aspire/ti8m.BeachBreak.AppHost
dotnet run

# Run individual APIs
cd 03_Infrastructure/ti8m.BeachBreak.CommandApi
dotnet run

cd 03_Infrastructure/ti8m.BeachBreak.QueryApi
dotnet run

cd 05_Frontend/ti8m.BeachBreak
dotnet run
```

### Database
- PostgreSQL database via Aspire with PgAdmin
- Event store in `events` schema
- Read models in `readmodels` schema
- Auto-migration in development mode

## Development Guidelines

### Adding New Aggregates
1. Create aggregate root in `01_Domain/ti8m.BeachBreak.Domain/[AggregateName]Aggregate/`
2. Define domain events in `Events/` subfolder
3. Implement Apply methods for event sourcing
4. Add commands in `02_Application/ti8m.BeachBreak.Application.Command/Commands/[AggregateName]Commands/`
5. Add command handlers implementing `ICommandHandler<TCommand, TResult>`
6. Add queries and read models in `02_Application/ti8m.BeachBreak.Application.Query/`
7. Update repository interfaces in application layer
8. Implement repositories in `03_Infrastructure/ti8m.BeachBreak.Infrastructure.Marten/`

### Command/Query Pattern
- Commands: Modify state, return Result<T> or Result
- Queries: Read data, return DTOs/read models
- Handlers: One handler per command/query, implement respective interface
- Use CommandDispatcher/QueryDispatcher for execution

### Event Sourcing Conventions
- Events are immutable records implementing IDomainEvent
- Use past tense naming (CategoryAdded, CategoryNameChanged)
- Apply methods update aggregate state from events
- RaiseEvent() both applies event and tracks for persistence

### Frontend Integration
- Blazor Server with WebAssembly components
- Radzen UI component library
- Service discovery integration for API communication
- Separate client project for WebAssembly components

### Testing Considerations
- Event sourcing allows easy testing of business logic via events
- Use Given/When/Then pattern with domain events
- Test command handlers independently of infrastructure
- Consider read model projections in integration tests

## Critical Development Patterns (ALWAYS FOLLOW)

### 1. Controller Response Pattern
- **ALWAYS** use `CreateResponse(result)` in API controllers
- **NEVER** use direct HTTP responses like `Ok()`, `BadRequest()`, `NotFound()`, etc.
- All controllers inherit base functionality that handles response formatting consistently

### 2. Logging Pattern
- **ALWAYS** use compile-time logging with `LoggerMessageAttribute`
- **NEVER** use direct `logger.LogInformation()`, `logger.LogError()`, etc.
- Add new logging definitions to `LoggerMessageDefinitions.cs` with unique event IDs
- Follow existing event ID ranges: QuestionnaireTemplate (5001-5022), QuestionnaireAssignment (6001-6018)

### 3. File Organization Pattern
- **EVERY** class, record, interface, and enum must be in its own separate file
- **NO** multiple types per file (except for very small nested types)
- File names must match the type name exactly

### 4. DTO and Type Safety Pattern
- **NEVER** use anonymous types for API requests/responses
- **ALWAYS** create proper DTOs that match between client and server
- **FRONTEND DTOs**: Create matching DTOs in `05_Frontend/ti8m.BeachBreak.Client/Models/Dto/`
- **API DTOs**: Located in `03_Infrastructure/ti8m.BeachBreak.CommandApi/Dto/`
- **ENDPOINT MATCHING**: Ensure client calls correct endpoints:
- **TYPE SAFETY**: Replace all `object`, `var`, and anonymous types with proper strongly-typed DTOs

### 5. NEVER Use `dynamic` Keyword
- **ABSOLUTELY FORBIDDEN**: The `dynamic` keyword must NEVER be used in this codebase
- **ALWAYS** create strongly-typed classes or records instead
- **REASON**: Loss of compile-time type safety, IntelliSense, and refactoring support
- **EXAMPLE - BAD**:
  ```csharp
  TemplateOptions = items.Select(i => (dynamic)new { Id = i.Id, Name = i.Name }).ToList()
  ```
- **EXAMPLE - GOOD**:
  ```csharp
  // Create a proper model class
  public class QuestionnaireTemplateOption
  {
      public Guid Id { get; set; }
      public string Name { get; set; } = string.Empty;
  }

  // Use it
  TemplateOptions = items.Select(i => new QuestionnaireTemplateOption
  {
      Id = i.Id,
      Name = i.Name
  }).ToList()
  ```

### 6. UserContext Pattern for User Identification
- **ALWAYS** use `UserContext` to get the current user's ID in API controllers
- **NEVER** use `User.Identity?.Name` or similar ASP.NET Core identity properties directly
- **INJECT** `UserContext` via dependency injection in controller constructors
- **USER ID TYPE**: UserContext.Id is a string representation of a GUID - always parse it to Guid when needed for commands
- **SECURITY**: UserContext is populated by middleware and provides authenticated user information consistently

**EXAMPLE - BAD**:
```csharp
[HttpPost("start")]
public async Task<IActionResult> StartReplay([FromBody] StartReplayDto request)
{
    var initiatedBy = User.Identity?.Name ?? "Unknown"; // DON'T DO THIS
    var command = new StartReplayCommand(request.Name, initiatedBy);
    // ...
}
```

**EXAMPLE - GOOD**:
```csharp
public class ReplayController : BaseController
{
    private readonly UserContext _userContext;

    public ReplayController(UserContext userContext, /* other dependencies */)
    {
        _userContext = userContext;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartReplay([FromBody] StartReplayDto request)
    {
        // Parse UserContext.Id (string) to Guid
        if (!Guid.TryParse(_userContext.Id, out var initiatedBy))
        {
            return CreateResponse(Result.Fail("User identification failed", 401));
        }

        var command = new StartReplayCommand(request.Name, initiatedBy);
        // ...
    }
}
```

**COMMAND PATTERN**: Commands that track user actions should use `Guid` for user identifiers (InitiatedBy, CancelledBy, CreatedBy, etc.), not strings:
```csharp
// CORRECT - Use Guid for user identifiers
public record StartReplayCommand(string Name, Guid InitiatedBy, string Reason);

// INCORRECT - Don't use string for user identifiers
public record StartReplayCommand(string Name, string InitiatedBy, string Reason);
```

### 7. Frontend Component Architecture - Questionnaire Rendering

**CRITICAL**: This pattern must be followed for ALL question rendering to prevent code duplication and data inconsistencies.

#### Always Use Optimized Components

**NEVER** write inline question rendering logic. **ALWAYS** use the centralized Optimized components:

```csharp
// ✅ CORRECT - Use OptimizedQuestionRenderer
<OptimizedQuestionRenderer
    Question="@question"
    Response="@response"
    OnResponseChanged="@HandleResponseChanged"
    IsReadOnly="@isReadOnly"
    HideHeader="@hideHeader" />

// ❌ WRONG - Inline rendering (causes duplication)
@if (question.Type == QuestionType.Assessment)
{
    // DON'T duplicate rendering logic here!
}
```

#### Component Locations

- **OptimizedQuestionRenderer.razor**: Master dispatcher for all question types
  Location: `05_Frontend/ti8m.BeachBreak.Client/Components/Questions/`
- **OptimizedAssessmentQuestion.razor**: Assessment questions with competency ratings
- **OptimizedTextQuestion.razor**: Text questions with single or multiple sections
- **OptimizedGoalQuestion.razor**: Goal achievement questions

#### Data Format Standards (CRITICAL - Prevents Data Loss Bugs)

**Text Question Response Keys**:
- **Single section**: Use key `"value"`
- **Multiple sections**: Use keys `"section_0"`, `"section_1"`, `"section_2"`, etc.
- **NEVER** use old format `"text_0"`, `"text_1"` (deprecated, causes data loss bugs)

**Assessment Response Keys**:
- **Ratings**: Use key `"rating_{competencyKey}"` (e.g., `"rating_communication"`)
- **Comments**: Use key `"comment_{competencyKey}"` (e.g., `"comment_communication"`)

**Goal Achievement Response Keys**:
- **Description**: Use key `"Description"`
- **Percentage**: Use key `"AchievementPercentage"`
- **Justification**: Use key `"Justification"`

#### Configuration Parsing

**NEVER** duplicate configuration parsing logic. If you need to parse question configuration:

1. **For rendering**: Use the Optimized components (they handle parsing internally)
2. **For validation**: Use the same key format as the Optimized components
3. **Existing duplications**: If you see `GetCompetenciesFromConfiguration()`, `GetRatingScaleFromQuestion()`, `GetTextSectionsFromQuestion()` duplicated across files, consolidate them into a shared service

#### Component Lifecycle Rules

**ALWAYS** initialize data in `OnInitialized()` in addition to `OnParametersSet()`:

```csharp
// ✅ CORRECT - Handles first render
protected override void OnInitialized()
{
    base.OnInitialized();
    LoadData(); // Populate data on first render
}

protected override void OnParametersSet()
{
    if (HasParameterChanged(nameof(Question), Question))
    {
        LoadData(); // Repopulate when parameters change
    }
}

// ❌ WRONG - Only in OnParametersSet (may not fire on first render)
protected override void OnParametersSet()
{
    LoadData(); // This might not execute on first render!
}
```

#### Validation Pattern

When validating question responses, **ALWAYS** match the data keys used by the Optimized components:

```csharp
// ✅ CORRECT - Matches OptimizedTextQuestion format
private bool IsTextQuestionCompleted(QuestionItem question, QuestionResponse response)
{
    if (textSections.Count == 1)
    {
        // Single section uses "value" key
        return response.ComplexValue?.TryGetValue("value", out var val) == true &&
               !string.IsNullOrWhiteSpace(val?.ToString());
    }

    // Multiple sections use "section_0", "section_1", etc.
    for (int i = 0; i < textSections.Count; i++)
    {
        var key = $"section_{i}";
        if (!response.ComplexValue?.TryGetValue(key, out var val) == true ||
            string.IsNullOrWhiteSpace(val?.ToString()))
        {
            return false;
        }
    }
    return true;
}

// ❌ WRONG - Uses old "text_" format (causes validation bugs)
private bool IsTextQuestionCompleted(QuestionItem question, QuestionResponse response)
{
    var key = $"text_{sectionOrder}"; // DON'T USE THIS FORMAT!
    return response.ComplexValue?.TryGetValue(key, out var val) == true;
}
```

#### Triggering Validation Updates

When responses change, **ALWAYS** update validation state:

```csharp
// ✅ CORRECT - Updates validation and progress
private async Task HandleQuestionResponseChanged(QuestionResponse updatedResponse)
{
    UpdateProgress(); // Recalculate validation state
    await InvokeAsync(StateHasChanged);
}

// ❌ WRONG - Only re-renders, doesn't recalculate validation
private async Task HandleQuestionResponseChanged(QuestionResponse updatedResponse)
{
    await InvokeAsync(StateHasChanged); // Submit button won't update!
}
```

#### Code Review Checklist for Question Rendering

Before submitting any code that touches question rendering, verify:

- [ ] Uses OptimizedQuestionRenderer (not inline rendering)
- [ ] Uses correct data key format (`"section_"` not `"text_"`)
- [ ] No duplicate configuration parsing (GetCompetencies, GetRatingScale, etc.)
- [ ] Validation matches the data keys used by Optimized components
- [ ] Component lifecycle properly initializes data in OnInitialized()
- [ ] Progress/validation updates when responses change (calls UpdateProgress())

#### Known Problem Areas (As of 2025-10-22)

If working with these components, **check for and fix duplications**:

- **QuestionnaireReviewMode.razor**: Has ~350 lines duplicate rendering logic, needs refactoring to use OptimizedQuestionRenderer
- **EditAnswerDialog.razor**: Has ~50 lines duplicate text question rendering, uses old "text_" key format
- **PreviewTab.razor**: Has ~100 lines duplicate configuration parsing, acceptable for preview purposes
- **DynamicQuestionnaire.razor**: Was refactored to use OptimizedQuestionRenderer, may still have some duplicate validation helpers

#### Historical Context: Why These Rules Exist

These rules were established after discovering critical bugs caused by code duplication:

1. **Submit Button Bug**: Button stayed enabled when required fields were cleared (validation not updating)
2. **Data Key Mismatch Bug**: Review mode couldn't read saved answers due to "text_" vs "section_" key mismatch (data loss)
3. **Edit Dialog Bug**: Edit dialog overwrote data with wrong keys "text_" instead of "section_" (data corruption)
4. **Code Duplication**: ~500+ lines of duplicate code across 5+ components made bugs hard to fix

**These were not theoretical concerns - they were real production bugs discovered through architectural review.**

#### When to Break These Rules

**NEVER**. If you think you need to break these rules:
1. Ask the user first
2. Document the architectural decision in an ADR
3. Provide justification for why existing components can't be used
4. Add comprehensive tests to prevent regressions

The refactoring effort to create the Optimized components was significant. Don't waste it by reintroducing duplication.

### 8. Enum Explicit Values Pattern
- **ALWAYS** set integer values explicitly for ALL enum members
- **NEVER** rely on implicit enum numbering
- **REASON**: Prevents serialization bugs when enum definitions differ across layers (CQRS/Event Sourcing architecture)
- **CRITICAL**: This codebase uses separate assemblies for Domain, Application, CommandApi, QueryApi, and Frontend - implicit enum values can lead to ordering mismatches

**EXAMPLE - BAD**:
```csharp
public enum QuestionType
{
    Assessment,      // Implicitly 0
    Goal,            // Implicitly 1 - DANGEROUS if order differs in another assembly
    TextQuestion     // Implicitly 2
}
```

**EXAMPLE - GOOD**:
```csharp
public enum QuestionType
{
    Assessment = 0,      // Explicitly 0 - consistent across all assemblies
    TextQuestion = 1,    // Explicitly 1
    Goal = 2             // Explicitly 2
}
```

**Historical Bug**: QuestionType enum had different implicit ordering in CommandApi (Assessment, Goal, TextQuestion) vs other layers (Assessment, TextQuestion, Goal), causing Goal questions (saved as Type=1) to be deserialized as TextQuestion (Type=1) in other layers, rendering them incorrectly.

**Rule**: Set explicit integer values for ALL enums in this codebase, even if they seem to have a natural sequential order.

### 9. Domain Validation Pattern - Template Configuration vs Response Data
- **CRITICAL**: When validating questionnaire responses, you must validate response data AGAINST template configuration
- **NEVER** validate only the response data in isolation
- **ALWAYS** extract the expected structure from template configuration, then verify ALL required elements are present in the response
- **PRINCIPLE**: Follows DDD's Information Expert pattern - the aggregate owns the data and knows how to validate itself using the template as a specification

**The Common Bug Pattern**:
```csharp
// ❌ WRONG - Validates response in isolation
private bool IsAssessmentComplete(QuestionItem question, AssessmentResponse response)
{
    // This checks if ANY competency is rated, but ignores the template configuration
    return response.Competencies.Any(c => c.Value.Rating > 0);
}

// Problem: If template defines 4 competencies but user only rates 1, this returns true!
```

**The Correct Pattern**:
```csharp
// ✅ CORRECT - Validates response against template configuration
private bool IsAssessmentComplete(QuestionItem question, AssessmentResponse response)
{
    // Step 1: Extract expected structure from template configuration
    var configCompetencyKeys = GetCompetencyKeysFromConfiguration(question);

    if (configCompetencyKeys.Count == 0)
    {
        return true; // No competencies defined, consider complete
    }

    // Step 2: Verify ALL required elements from config exist in response
    return configCompetencyKeys.All(competencyKey =>
        response.Competencies.TryGetValue(competencyKey, out var competencyResponse) &&
        competencyResponse.Rating > 0);
}
```

**Configuration Parsing Pattern**:

When parsing template configuration (stored as `Dictionary<string, object>` containing JSON):

```csharp
private List<string> GetCompetencyKeysFromConfiguration(QuestionItem question)
{
    if (question.Configuration.TryGetValue("Competencies", out var obj))
    {
        // Configuration values are stored as System.Text.Json.JsonElement
        if (obj is System.Text.Json.JsonElement jsonElement &&
            jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            var keys = new List<string>();
            foreach (var item in jsonElement.EnumerateArray())
            {
                // Extract the "Key" property from each competency object
                if (item.TryGetProperty("Key", out var keyProperty) &&
                    keyProperty.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var key = keyProperty.GetString();
                    if (!string.IsNullOrEmpty(key))
                    {
                        keys.Add(key);
                    }
                }
            }
            return keys;
        }
    }
    return new List<string>();
}
```

**Where This Pattern Applies**:
- `QuestionnaireResponse.GetCompletedSections()` - Domain aggregate validation (01_Domain)
- Frontend validation in question components - Must match domain validation logic
- Command handlers that validate before submission - Use domain aggregate methods

**Key Locations**:
- `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireResponseAggregate/QuestionnaireResponse.cs` - Canonical validation logic
- See pattern #7 (Frontend Component Architecture) for matching validation in the UI layer

**Historical Bug** (Fixed 2025-11-10): Assessment validation only checked if ANY competency was rated (`Any(c => c.Rating > 0)`), rather than validating that ALL competencies defined in the template configuration were rated. This allowed incomplete assessments to be marked as complete.