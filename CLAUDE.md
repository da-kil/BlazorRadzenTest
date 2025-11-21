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


#### Historical Context

This typography system was consolidated (2025-11-21) to eliminate 88+ hardcoded font-weight declarations scattered across 19+ CSS files. The new system provides:
- Single source of truth for font decisions
- Semantic variable names for clear intent
- Component-specific variables for maintainability
- 77% reduction in font-weight declarations