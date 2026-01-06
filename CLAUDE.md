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

## Questionnaire Workflow States

### Initialized Workflow State (Added 2026-01-06)

**Purpose**: The `Initialized` state represents a manager-only initialization phase between assignment creation and employee access. This allows managers to prepare the assignment with optional customizations before employees begin work.

**Workflow Sequence**:
1. **Assigned** (value=0) - Assignment created, manager-only access
2. **Initialized** (value=1) - Manager completed initialization, both can access
3. **EmployeeInProgress** / **ManagerInProgress** / **BothInProgress** (values=2-4)
4. Continue through existing workflow states (5-11)

**Key Features**:
- **Manager Initialization Tasks** (all optional):
  - Link predecessor questionnaire for goal tracking
  - Add custom Assessment or TextQuestion sections (`IsInstanceSpecific = true`)
  - Add initialization notes for employee (max 5000 characters)
- **Access Control**:
  - Employees CANNOT see assignments in `Assigned` state
  - Employees CAN see and work on assignments in `Initialized+` states
  - Only managers (TeamLead/HR/HRLead/Admin) can initialize

**Custom Sections**:
- Marked with `IsInstanceSpecific = true`
- Created during manager initialization (Assigned state only)
- Appear seamlessly with template sections in UI
- **Excluded from aggregate reports** (instance-specific, not comparable across assignments)
- Cannot add Goal-type custom sections (created dynamically during workflow)

**Commands**:
- `InitializeAssignmentCommand` - Transitions Assigned → Initialized
- `AddCustomSectionsCommand` - Adds custom questions (must be in Assigned state)
- `GetCustomSectionsQuery` - Retrieves custom sections for an assignment

**Events**:
- `AssignmentInitializedEvent` - Marks completion of manager initialization
- `CustomSectionsAddedEvent` - Tracks addition of custom questions

**Frontend Routes**:
- `/assignments/{id}/initialize` - Manager-only initialization page (AuthorizeView: TeamLead policy)
- Page includes: predecessor linking, custom question dialog, initialization notes

**Translation Keys** (46 total, EN/DE):
- `workflow-states.initialized`
- `actions.employee.waiting-manager-initialization`
- `actions.manager.initialize-assignment`
- See `TestDataGenerator/test-translations.json` for complete list

**Validation Rules**:
- Can only initialize from `Assigned` state
- Cannot go backwards from `Initialized` to `Assigned`
- Custom sections can only be added in `Assigned` state (before initialization)
- Assigned → EmployeeInProgress is **invalid** (must initialize first)

**Implementation Locations**:
- Domain: `01_Domain/QuestionnaireAssignmentAggregate/WorkflowState.cs` (enum value=1)
- Commands: `02_Application/Application.Command/Commands/QuestionnaireAssignmentCommands/`
- Handlers: `02_Application/Application.Command/Commands/QuestionnaireAssignmentCommands/`
- Frontend: `05_Frontend/ti8m.BeachBreak.Client/Pages/InitializeAssignment.razor`
- Component: `05_Frontend/ti8m.BeachBreak.Client/Components/Dialogs/AddCustomQuestionDialog.razor`
- Helper: `05_Frontend/ti8m.BeachBreak.Client/Models/WorkflowStateHelper.cs`

**Testing**:
- Unit tests: `Tests/ti8m.BeachBreak.Domain.Tests/WorkflowStateMachineTests.cs`
- Manual E2E checklist: `Tests/README.md`

**Design Decisions**:
- Enum value 1 explicitly set (all workflow states have explicit values per CLAUDE.md Section 8)
- IsInstanceSpecific flag prevents custom sections from appearing in cross-instance reports
- Initialization is optional (manager can complete it immediately with no customizations)
- Maintains event sourcing pattern with explicit domain events

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

### 6. UserContext Pattern for User Identification
- **ALWAYS** use `UserContext` to get the current user's ID in API controllers
- **NEVER** use `User.Identity?.Name` or similar ASP.NET Core identity properties directly
- **INJECT** `UserContext` via dependency injection in controller constructors
- **USER ID TYPE**: UserContext.Id is a string representation of a GUID - always parse it to Guid when needed for commands
- **SECURITY**: UserContext is populated by middleware and provides authenticated user information consistently

**COMMAND PATTERN**: Commands that track user actions should use `Guid` for user identifiers (InitiatedBy, CancelledBy, CreatedBy, etc.), not strings:

### 7. Frontend Component Architecture - Questionnaire Rendering

**CRITICAL**: This pattern must be followed for ALL question rendering to prevent code duplication and data inconsistencies.

#### Always Use Optimized Components

**NEVER** write inline question rendering logic. **ALWAYS** use the centralized Optimized components:

#### Component Locations

- **OptimizedQuestionRenderer.razor**: Master dispatcher for all question types
  Location: `05_Frontend/ti8m.BeachBreak.Client/Components/Questions/`
- **OptimizedAssessmentQuestion.razor**: Assessment questions with competency ratings
- **OptimizedTextQuestion.razor**: Text questions with single or multiple sections
- **OptimizedGoalQuestion.razor**: Goal achievement questions

#### Configuration Parsing

**NEVER** duplicate configuration parsing logic. If you need to parse question configuration:

1. **For rendering**: Use the Optimized components (they handle parsing internally)
2. **For validation**: Use the same key format as the Optimized components
3. **Existing duplications**: If you see `GetCompetenciesFromConfiguration()`, `GetRatingScaleFromQuestion()`, `GetTextSectionsFromQuestion()` duplicated across files, consolidate them into a shared service

#### Component Lifecycle Rules

**ALWAYS** initialize data in `OnInitialized()` in addition to `OnParametersSet()`:

#### Validation Pattern

When validating question responses, **ALWAYS** match the data keys used by the Optimized components:

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

**Historical Bug**: QuestionType enum had different implicit ordering in CommandApi (Assessment, Goal, TextQuestion) vs other layers (Assessment, TextQuestion, Goal), causing Goal questions (saved as Type=1) to be deserialized as TextQuestion (Type=1) in other layers, rendering them incorrectly.

**Rule**: Set explicit integer values for ALL enums in this codebase, even if they seem to have a natural sequential order.

### 9. Domain Validation Pattern - Template Configuration vs Response Data
- **CRITICAL**: When validating questionnaire responses, you must validate response data AGAINST template configuration
- **NEVER** validate only the response data in isolation
- **ALWAYS** extract the expected structure from template configuration, then verify ALL required elements are present in the response
- **PRINCIPLE**: Follows DDD's Information Expert pattern - the aggregate owns the data and knows how to validate itself using the template as a specification

**Configuration Parsing Pattern**:

**Where This Pattern Applies**:
- `QuestionnaireResponse.GetCompletedSections()` - Domain aggregate validation (01_Domain)
- Frontend validation in question components - Must match domain validation logic
- Command handlers that validate before submission - Use domain aggregate methods

**Key Locations**:
- `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireResponseAggregate/QuestionnaireResponse.cs` - Canonical validation logic
- See pattern #7 (Frontend Component Architecture) for matching validation in the UI layer

**Historical Bug** (Fixed 2025-11-10): Assessment validation only checked if ANY competency was rated (`Any(c => c.Rating > 0)`), rather than validating that ALL competencies defined in the template configuration were rated. This allowed incomplete assessments to be marked as complete.

### 10. Authorization Pattern - Consistent Page-Level Security

**CRITICAL**: All admin pages use `<AuthorizeView Policy="...">` pattern for consistent user experience and clear access control.

#### Standard Authorization Pattern for Admin Pages

**ALWAYS** use the AuthorizeView component for admin pages instead of `@attribute [Authorize]`:

```razor
@page "/admin/some-page"
@using Microsoft.AspNetCore.Components.Authorization

<AuthorizeView Policy="PolicyName">
    <Authorized>
        <StandardPageLayout Title="Page Title" Description="Page description">
            <!-- Page content here -->
        </StandardPageLayout>
    </Authorized>
    <NotAuthorized>
        <AccessDeniedComponent RequiredRole="RequiredRole" PageName="Descriptive Page Name" />
    </NotAuthorized>
</AuthorizeView>
```

#### Authorization Policies Available

1. **Employee** - All authenticated users (Employee+)
2. **TeamLead** - Team leads and above (TeamLead, HR, HRLead, Admin)
3. **HR** - HR staff and above (HR, HRLead, Admin)
4. **HRLead** - HR leads and above (HRLead, Admin)
5. **Admin** - Administrator only

#### Pages Using AuthorizeView Pattern

**Admin Pages (HR Policy)**:
- HRDashboard.razor
- QuestionnaireManagement.razor
- QuestionnaireBuilder.razor
- QuestionnaireAssignments.razor
- CategoryAdmin.razor
- RoleManagement.razor
- OrganizationQuestionnaires.razor

**Manager Pages (TeamLead Policy)**:
- ManagerDashboard.razor
- TeamQuestionnaires.razor

**System Admin (Admin Policy)**:
- ProjectionReplayAdmin.razor

**Employee Pages (Generic @attribute [Authorize])**:
- Dashboard.razor
- MyQuestionnaires.razor
- DynamicQuestionnaire.razor
- Home.razor

#### Key Benefits of AuthorizeView Pattern

1. **Consistent UX**: Users get clear "Access Denied" messages instead of blank pages
2. **Professional Error Handling**: Custom error UI with icons and explanations
3. **Maintainable**: Centralized AccessDeniedComponent for consistent styling
4. **User-Friendly**: Clear messaging about required permissions

#### AccessDeniedComponent Usage

The reusable component accepts two parameters:
- `RequiredRole`: Display name of required role (e.g., "HR", "Admin", "TeamLead")
- `PageName`: Descriptive name of the page (e.g., "Questionnaire Builder", "Role Management")

#### Historical Context

This pattern was standardized (2025-11-21) to replace inconsistent authorization approaches. Previously, most admin pages used `@attribute [Authorize(Policy = "...")]` which resulted in blank pages for unauthorized users, while only RoleManagement.razor provided proper error messages.

#### When to Use Each Pattern

- **Use AuthorizeView**: For admin pages where users need clear feedback about access restrictions
- **Use @attribute [Authorize]**: For employee-accessible pages where generic authentication is sufficient

## Translation System Guidelines

### Overview

The application uses a multilingual translation system with support for English and German. Translations are stored in `TestDataGenerator/test-translations.json` and loaded at runtime.

### Adding New Translations

**CRITICAL**: When adding new UI text that uses `@T("translation.key")`, you MUST add the translation to test-translations.json IMMEDIATELY.

#### Step-by-Step Process

1. **Add translation key to your Razor component:**
   ```razor
   <RadzenText>@T("sections.my-new-section")</RadzenText>
   ```

2. **Add entry to test-translations.json:**
   ```json
   {
     "key": "sections.my-new-section",
     "german": "Mein neuer Abschnitt",
     "english": "My New Section",
     "category": "sections",
     "createdDate": "2025-12-04T12:00:00.0000000+00:00"
   }
   ```

3. **Validate translations:**
   ```bash
   powershell -ExecutionPolicy Bypass -File TestDataGenerator\validate-translations.ps1
   ```

### Translation Key Naming Conventions

- **Format**: Lowercase with hyphens: `my-translation-key`
- **Semantic prefixes**: Use category-based prefixes
  - `pages.*` - Page titles and descriptions
  - `sections.*` - Section headings
  - `tabs.*` - Tab labels
  - `buttons.*` - Button text
  - `labels.*` - Form labels and UI labels
  - `columns.*` - Data grid column headers
  - `placeholders.*` - Input placeholders
  - `filters.*` - Filter labels
  - `messages.*` - User messages
  - `dialogs.*` - Dialog titles and content
  - `tooltips.*` - Tooltip text
  - `notifications.*` - Toast/notification messages
  - `status.*` - Status labels
  - `workflow-states.*` - Workflow state labels

### German Translation Guidelines

**Domain-Specific Terminology** (maintain consistency):
- "Questionnaire" → "Fragebogen"
- "Assignment" → "Zuweisung"
- "Employee" → "Mitarbeiter"
- "Template" → "Vorlage"
- "Category" → "Kategorie"
- "Status" → "Status"
- "Manager" → "Manager"
- "Review" → "Überprüfung"

**Unicode Encoding for German Umlauts** (in JSON):
- ä → `\u00E4`
- ö → `\u00F6`
- ü → `\u00FC`
- Ä → `\u00C4`
- Ö → `\u00D6`
- Ü → `\u00DC`
- ß → `\u00DF`

**Formality**: Use professional business German (Sie-Form)

### Validation Tools

**Validate all translations:**
```bash
powershell -ExecutionPolicy Bypass -File TestDataGenerator\validate-translations.ps1
```

This script:
- Scans all .razor files for `@T("...")` calls
- Compares against test-translations.json
- Reports missing translations with detailed breakdown by category
- Returns exit code 1 if translations are missing

**Verify specific pages:**
```bash
powershell -ExecutionPolicy Bypass -File TestDataGenerator\verify-target-pages.ps1
```

### Historical Context

**2025-12-04**: Translation Recovery
- During initial multilingual migration (commit c79f22a), 187 translation keys were used in code but missing from test-translations.json
- Recovered 54 missing translations for QuestionnaireAssignments, ProjectionReplayAdmin, and CategoryAdmin pages
- Original English text extracted from git commit 4a3f807 (pre-migration)
- German translations generated using AI-assisted translation with domain terminology
- Created validate-translations.ps1 to prevent future gaps

**Lesson Learned**: Always add translations atomically with UI changes. Running the validation script before committing prevents missing translations from reaching production.

### Code Review Checklist

When reviewing PRs that add or modify UI text:

- [ ] All new `@T("...")` calls have entries in test-translations.json
- [ ] German translations are accurate and use professional business terminology
- [ ] Translation keys follow naming conventions (lowercase-with-hyphens)
- [ ] Proper category prefix used (pages.*, sections.*, etc.)
- [ ] German umlauts properly Unicode-escaped in JSON
- [ ] Validation script passes: `validate-translations.ps1`
- [ ] Tested language switching (English ↔ German)
- [ ] No hardcoded UI text visible (all text uses @T())

### Common Mistakes to Avoid

1. **❌ Using hardcoded text without translations:**
   ```razor
   <RadzenText>Select Employees</RadzenText>  <!-- BAD -->
   ```
   **✅ Always use translation keys:**
   ```razor
   <RadzenText>@T("sections.select-employees")</RadzenText>  <!-- GOOD -->
   ```

2. **❌ Adding @T() calls without adding to test-translations.json:**
   - This causes translation keys to appear in the UI instead of actual text
   - Always add both simultaneously

3. **❌ Using wrong Unicode encoding:**
   ```json
   "german": "Mitarbeiter auswählen"  <!-- BAD - will break JSON -->
   ```
   **✅ Use proper Unicode escaping:**
   ```json
   "german": "Mitarbeiter ausw\u00E4hlen"  <!-- GOOD -->
   ```

4. **❌ Inconsistent domain terminology:**
   - Using "Angestellte" instead of "Mitarbeiter" for "Employee"
   - Check existing translations for consistency

### Quick Reference: Merge Translations

If you've created new translations in a separate file:

```bash
cd TestDataGenerator
powershell -ExecutionPolicy Bypass -File merge-translations.ps1
```

This will merge, deduplicate, and sort all translations alphabetically by key.

## Typography System Guidelines

### Font Weight Architecture

**CRITICAL**: This application uses a consolidated typography system with semantic CSS variables. Never hardcode font-weight values.

#### Base Font System
- **Font Family**: Roboto only (`font-family: 'Roboto', sans-serif`)
- **Default Body Weight**: 300 (Roboto Light)
- **Default Heading Weight**: 500 (Roboto Medium)

#### Font Weight Variables (Use These)

**Base Weights**:
- `--font-weight-light: 300` - Roboto Light
- `--font-weight-regular: 400` - Roboto Regular
- `--font-weight-medium: 500` - Roboto Medium
- `--font-weight-semibold: 600` - Roboto Semibold

**Semantic Aliases (Preferred)**:
- `--font-weight-body: 300` - Body text default
- `--font-weight-heading: 500` - All headings (h1-h6)
- `--font-weight-emphasis: 500` - Emphasized text
- `--font-weight-strong: 600` - Strong emphasis

**Component-Specific Variables**:
- `--font-weight-card-title: 500` - Card headers
- `--font-weight-section-title: 500` - Section headings
- `--font-weight-form-label: 500` - Form labels
- `--font-weight-badge: 500` - Badge text
- `--font-weight-button: 500` - Button text
- `--font-weight-nav-item: 500` - Navigation items

#### Typography Rules

**✅ DO**:
```css
/* Use semantic variables */
.employee-name { font-weight: var(--font-weight-emphasis); }

/* Use component-specific variables */
.card-header { font-weight: var(--font-weight-card-title); }

/* Rely on inheritance for headings */
<h3 class="section-title">Title</h3> /* Already inherits font-weight: 500 */
```

**❌ DON'T**:
```css
/* Never hardcode numeric values */
.title { font-weight: 500; }
.label { font-weight: 600; }
.text { font-weight: bold; }

/* Don't override inherited heading weights unnecessarily */
h3 { font-weight: 500; } /* Already inherited from root */
```

#### Utility Classes Available

Use these for inline emphasis in HTML:
- `.font-light` - Light text (300)
- `.font-medium` - Medium emphasis (500)
- `.font-semibold` - Strong emphasis (600)

```html
<!-- Good: Utility classes for emphasis -->
<p class="font-medium">Important text</p>
<span class="font-light">Subtle text</span>
```

#### Maintenance Guidelines

**Single Source of Truth**: All font-weight changes happen in `shared-variables.css`

**Component Changes**: Use semantic variables, not hardcoded values
```css
/* Component CSS - Use semantic variables */
.special-title {
    font-weight: var(--font-weight-section-title); /* Good */
}
```

**Design System Evolution**: Change variable values to update entire application
```css
/* To make all sections bolder, change one variable: */
--font-weight-section-title: var(--font-weight-semibold); /* Updates everywhere */
```

## 11. Strongly-Typed Question Configuration Pattern

### Overview

**CRITICAL**: This codebase uses strongly-typed configuration classes instead of `Dictionary<string, object>` for question configurations. This provides compile-time type safety and eliminates ~700 lines of JSON parsing logic that was previously duplicated across the codebase.

### IQuestionConfiguration Hierarchy

All question configurations implement the `IQuestionConfiguration` interface:

```csharp
public interface IQuestionConfiguration
{
    QuestionType QuestionType { get; }
}
```

**Available Configuration Types:**

1. **AssessmentConfiguration** - For competency/skill assessments
2. **TextQuestionConfiguration** - For text-based questions
3. **GoalConfiguration** - For goal management questions

### Pattern: Accessing Typed Configuration

**ALWAYS** use pattern matching with the `is` operator to access configuration properties:

```csharp
// ✅ CORRECT: Pattern matching for type-safe access
if (question.Configuration is AssessmentConfiguration config)
{
    var evaluations = config.Evaluations;
    var ratingScale = config.RatingScale;
    var lowLabel = config.ScaleLowLabel;
    var highLabel = config.ScaleHighLabel;
}

// ❌ WRONG: Don't use Dictionary access
var evaluations = question.Configuration["Evaluations"]; // Compiler error!
```

### AssessmentConfiguration

Used for questions that assess competencies/skills with ratings:

```csharp
public sealed class AssessmentConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.Assessment;
    public List<EvaluationItem> Evaluations { get; set; } = new();
    public int RatingScale { get; set; } = 4;
    public string ScaleLowLabel { get; set; } = "Poor";
    public string ScaleHighLabel { get; set; } = "Excellent";
}
```

**Example Usage:**
```csharp
private void UpdateRatingScale(QuestionItem question, int newScale)
{
    if (question.Configuration is AssessmentConfiguration config)
    {
        config.RatingScale = newScale;
        // Direct property access - compile-time safe!
    }
}
```

### TextQuestionConfiguration

Used for questions with text input sections:

```csharp
public sealed class TextQuestionConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.TextQuestion;
    public List<TextSection> TextSections { get; set; } = new();
}
```

**Example Usage:**
```csharp
private List<TextSection> GetTextSections(QuestionItem question)
{
    if (question.Configuration is TextQuestionConfiguration config)
    {
        return config.TextSections; // Direct access, no parsing!
    }
    return new List<TextSection>();
}
```

### GoalConfiguration

Used for goal management questions (goals added dynamically during workflow):

```csharp
public sealed class GoalConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.Goal;
    public bool ShowGoalSection { get; set; } = true;
}
```

**Key Point:** Goals don't have template-level items. The `ShowGoalSection` flag controls visibility in the UI.

**Example Usage:**
```csharp
private bool ShouldShowGoalSection(QuestionItem question)
{
    if (question.Configuration is GoalConfiguration config)
    {
        return config.ShowGoalSection;
    }
    return true; // Default to visible
}
```

### Pattern: Initializing Configuration

When creating new questions, initialize with typed configuration:

```csharp
// ✅ CORRECT: Create typed configuration
var question = new QuestionItem
{
    Type = QuestionType.Assessment,
    Configuration = new AssessmentConfiguration
    {
        Evaluations = new List<EvaluationItem>
        {
            new EvaluationItem("evaluation_1", "Leadership", "", false, 0)
        },
        RatingScale = 4,
        ScaleLowLabel = "Poor",
        ScaleHighLabel = "Excellent"
    }
};

// ❌ WRONG: Don't create Dictionary
var question = new QuestionItem
{
    Configuration = new Dictionary<string, object>() // Don't do this!
};
```

### Pattern: Updating Configuration

Always check type before updating:

```csharp
// ✅ CORRECT: Type-safe updates
public void AddEvaluation(QuestionItem question, EvaluationItem evaluation)
{
    if (question.Configuration is AssessmentConfiguration config)
    {
        config.Evaluations.Add(evaluation);
    }
    else
    {
        // Initialize if needed
        question.Configuration = new AssessmentConfiguration
        {
            Evaluations = new List<EvaluationItem> { evaluation }
        };
    }
}
```

### Common Mistakes to Avoid

1. **❌ Don't use Dictionary methods:**
   ```csharp
   // WRONG - Will not compile
   question.Configuration.ContainsKey("Evaluations")
   question.Configuration["RatingScale"] = 5
   question.Configuration.TryGetValue("TextSections", out var value)
   ```

2. **❌ Don't parse JSON manually:**
   ```csharp
   // WRONG - No longer needed
   var jsonElement = configuration["Evaluations"] as JsonElement;
   var evaluations = JsonSerializer.Deserialize<List<EvaluationItem>>(jsonElement.GetRawText());
   ```

3. **❌ Don't create helper methods for Dictionary parsing:**
   ```csharp
   // WRONG - This pattern is obsolete
   private List<EvaluationItem> GetEvaluationsFromConfiguration(Dictionary<string, object> config)
   {
       // 50 lines of JSON parsing logic - NO LONGER NEEDED!
   }
   ```

### Services Using Typed Configuration

The following services have been updated to use typed configuration:

1. **QuestionConfigurationService** - Helper methods for configuration access
2. **AssessmentConfigurationHelper** - Static helpers for assessment questions
3. **QuestionHandlers** - Initialize and manage question configurations
   - AssessmentQuestionHandler
   - TextQuestionHandler
   - GoalQuestionHandler

### Benefits of Typed Configuration

1. **Compile-Time Safety**: Typos and type errors caught by compiler
2. **IntelliSense Support**: IDE autocomplete for all properties
3. **Simplified Code**: ~700 lines of parsing logic eliminated
4. **Maintainability**: Single source of truth for configuration structure
5. **Refactoring Support**: Rename refactoring works across entire codebase

### Historical Context

**Why This Pattern Exists:**

Prior to 2025-12, this codebase used `Dictionary<string, object>` for question configurations, which resulted in:
- ~550+ lines of duplicate JSON parsing logic across 15+ files
- No compile-time type safety (runtime errors only)
- Historical validation bug (2025-11-10) due to parsing complexity
- Difficult maintenance when adding new question types

The refactoring to typed configuration (completed 2025-12-08) eliminated these issues and provides a robust, type-safe foundation for future development.

### Adding New Question Types

If you need to add a new question type:

1. Create a new configuration class implementing `IQuestionConfiguration`
2. Add the new `QuestionType` enum value
3. Create a handler class implementing `IQuestionTypeHandler`
4. Update rendering components to handle the new type
5. Update validation logic if needed

**Example:**
```csharp
public sealed class MultipleChoiceConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.MultipleChoice;
    public List<ChoiceOption> Options { get; set; } = new();
    public bool AllowMultipleSelections { get; set; } = false;
}
```

### References

- Configuration classes: `05_Frontend/ti8m.BeachBreak.Client/Models/`
- Handler classes: `05_Frontend/ti8m.BeachBreak.Client/Services/QuestionHandlers/`
- Usage examples: See QuestionCard.razor, SectionCard.razor, DynamicQuestionnaire.razor

## 12. Configuration Serialization Pattern

### Overview

The questionnaire configuration uses polymorphic JSON serialization with a `$type` discriminator. This section explains why the JSON contains both a section-level `Type` field and a configuration-level `$type` discriminator, and why this apparent redundancy is intentional defensive design.

### JSON Structure

```json
{
  "Type": 0,  // Section-level question type (QuestionType enum)
  "Configuration": {
    "$type": 0,  // Configuration-level discriminator (same value)
    "Evaluations": [...],
    "RatingScale": 4,
    "ScaleLowLabel": "Poor",
    "ScaleHighLabel": "Excellent"
  }
}
```

### Why Both Type and $type Exist

**1. Section.Type**: Semantic field indicating what type of question this section represents
   - Used by domain logic for validation and business rules
   - Part of the `QuestionSection` domain model
   - Accessed directly in code: `section.Type`

**2. Configuration.$type**: JSON discriminator enabling polymorphic deserialization of `IQuestionConfiguration`
   - Required by the JSON deserializer to determine concrete type
   - Maps to the appropriate configuration class:
     - `$type: 0` → `AssessmentConfiguration`
     - `$type: 1` → `TextQuestionConfiguration`
     - `$type: 2` → `GoalConfiguration`
   - Handled by `QuestionConfigurationJsonConverter`

### The Pattern is Intentional Defensive Design

The apparent redundancy is **intentional and necessary**:

1. **Validation Safety**: The two values must always match, validated by `ValidateConfigurationMatchesType()`:
   ```csharp
   if (Type != Configuration.QuestionType)
   {
       throw new InvalidOperationException(
           $"Configuration type mismatch: Section Type is {Type} but Configuration is for {Configuration.QuestionType}");
   }
   ```
   This catches bugs where frontend and backend disagree on types.

2. **Separation of Concerns**: `IQuestionConfiguration` is used independently in many contexts without access to the parent section:
   - Domain events (`QuestionSectionData`)
   - Command DTOs (`CommandQuestionSection`)
   - Query DTOs (`QuestionSectionDto`)
   - Frontend models (`QuestionSection`)

   In these contexts, the Configuration object needs its own type information.

3. **Standard .NET Pattern**: Using a discriminator for polymorphic JSON follows .NET best practices:
   - Supported by System.Text.Json
   - Well-understood industry pattern
   - Tool support (Swagger, API testing)
   - Fast and unambiguous deserialization

4. **Similar to Other Safety Mechanisms**:
   - Database foreign key constraints
   - Email confirmation fields in forms
   - Checksums in data transmission

   The redundancy **prevents bugs** rather than creating them.

### QuestionConfigurationJsonConverter

**Location**:
- `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/QuestionConfigurationJsonConverter.cs`
- `05_Frontend/ti8m.BeachBreak.Client/Models/QuestionConfigurationJsonConverter.cs`

**Read Method** (Deserialization):
- Looks for `"$type"` property in JSON
- If found: uses it to determine the concrete type
- If NOT found: falls back to property inference (backward compatibility)

**Write Method** (Serialization):
- Always writes `"$type": (int)value.QuestionType` at the beginning
- Then writes type-specific properties

### Common Misconceptions

**❌ WRONG**: "The $type discriminator is redundant because we have Section.Type"

**✅ CORRECT**: The $type discriminator serves a different purpose than Section.Type:
- Section.Type is a domain field for business logic
- Configuration.$type is a JSON serialization mechanism
- Both are necessary and must match

**❌ WRONG**: "We can remove $type and just use Section.Type for deserialization"

**✅ CORRECT**: This would require:
- Custom converter logic coupling Section and Configuration
- Breaking isolated Configuration usage (events, DTOs)
- Loss of backward compatibility
- More complex deserialization code
- Going against .NET best practices

### Design Decision (2025-12-12)

This pattern was explicitly reviewed and the decision was made to **keep the current design**:

**Rationale**:
- Follows .NET best practices for polymorphic JSON
- Provides defensive validation safety
- Enables Configuration to be used independently
- Low cost (~10 bytes per section) vs high complexity of alternatives
- Backward compatible with property inference fallback

**Do not remove the $type discriminator** - it's not redundant, it's defensive validation.

### Historical Context

**Investigation Date**: 2025-12-12

During a review of the questionnaire template JSON structure, the apparent redundancy between `Type` and `$type` was questioned. A comprehensive investigation revealed:

1. The $type discriminator is necessary for polymorphic deserialization
2. The apparent redundancy is intentional defensive design
3. This pattern follows .NET best practices
4. Works correctly in CQRS/Event Sourcing architecture

The investigation confirmed the current implementation is correct and should be preserved.

### References

- JSON Converter: `QuestionConfigurationJsonConverter.cs` (Core.Domain and Client projects)
- Domain Validation: `QuestionSection.ValidateConfigurationMatchesType()`
- Pattern documentation: See Section 11 "Strongly-Typed Question Configuration Pattern"