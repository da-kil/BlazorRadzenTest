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
- .NET 10
- Blazor WebAssembly with Radzen UI
- PostgreSQL with Marten for event sourcing
- .NET Aspire for local development orchestration
- Separate Command and Query APIs with centralized exception handling

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

### Infrastructure Architecture

#### Database Connection Management
**CRITICAL**: Single NpgsqlDataSource registration to prevent connection pool conflicts.

- **Pattern**: Register NpgsqlDataSource only in Extensions.cs via AddMartenInfrastructure()
- **NEVER** register NpgsqlDataSource directly in Program.cs files
- **Reason**: Multiple registrations cause connection pool conflicts and resource leaks
- **Implementation**: Both CommandApi and QueryApi use shared connection configuration

#### Exception Handling Architecture
**CRITICAL**: Centralized exception handling via GlobalExceptionMiddleware.

- **Pattern**: All exceptions handled by GlobalExceptionMiddleware in both APIs
- **NEVER** wrap controller actions in try-catch blocks
- **Benefits**: Consistent error responses, structured logging, correlation tracking
- **Response Format**: RFC 7807 ProblemDetails with correlation IDs and request metadata
- **Development Mode**: Includes exception details and stack traces for debugging

#### Configuration Management
**CRITICAL**: Environment-specific configuration for caching and performance.

- **Authorization Cache**: Configurable TTL via AuthorizationCacheSettings
- **Default TTL**: 15 minutes with environment-specific overrides
- **Pattern**: Use IOptions<T> for all configuration settings
- **Location**: appsettings.json in both CommandApi and QueryApi

## Workflow States and Process Types

For detailed information about questionnaire workflow states, process types, and state transitions, see: [docs/domain/questionnaire-workflows.md](docs/domain/questionnaire-workflows.md)

**Quick Reference**:
- **Workflow States**: 12 states from Assigned → Finalized with explicit enum values
- **Process Types**: PerformanceReview vs Survey with different business rules
- **Auto-Initialization**: Templates can skip manual initialization phase
- **State Machine**: Validates all transitions with role-based authorization

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

## Performance Architecture

### Translation System Optimization
**CRITICAL**: Category-based translation loading prevents performance bottlenecks.

- **Problem Solved**: Loading all 978 translation keys caused 150-400KB network transfers per component
- **Solution**: Category-based loading reduces payload by 75-87% (50-200 keys per component)
- **Pattern**: Components specify required translation categories via GetRequiredTranslationCategories()
- **Implementation**: CategoryOptimizedTranslatableComponent base class
- **Categories**: CoreCategories (~95 keys), CommonCategories (~205 keys), QuestionnaireCategories (~375 keys)
- **Memory Impact**: Reduced from 39KB to 8-15KB per component (60-80% reduction)

### Component Architecture Optimization
**CRITICAL**: Composition-based design over complex inheritance hierarchies.

- **Problem Solved**: 3-level inheritance hierarchy (524 lines) caused debugging complexity
- **Solution**: Composition-based services with lightweight base classes (~200 lines, 38% reduction)
- **Pattern**: Use IComponentPerformanceService and IComponentTranslationManager interfaces
- **Benefits**: Better testability, clearer separation of concerns, easier maintenance

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

### Testing Architecture

#### Domain Aggregate Testing Framework
**CRITICAL**: Use standardized Given/When/Then framework for all domain aggregate testing.

- **Framework**: AggregateTestBase<T> with fluent event assertions
- **Location**: Tests/ti8m.BeachBreak.Domain.Tests/Common/
- **Pattern**: Given(events) → When(action) → Then(assertions)
- **Features**: Event validation, exception testing, state verification, version tracking
- **Performance**: <100ms per test execution, suitable for large test suites

**Example Usage**:
```csharp
[Test]
public void ChangeName_WithValidName_RaisesNameChangedEvent()
{
    // Given - aggregate exists
    Given(new CategoryAdded(id, name, description, DateTime.UtcNow, DateTime.UtcNow, 1));

    // When - business operation
    When(() => Aggregate.ChangeName(newName));

    // Then - expected events
    ThenEventWasRaised<CategoryNameChanged>()
        .WithProperty(e => e.Name, newName)
        .WithTimestampNear(e => e.LastModifiedDate);
}
```

#### Testing Categories
- **Unit Tests**: Single aggregate behavior using AggregateTestBase<T>
- **Integration Tests**: Multi-aggregate scenarios and service dependencies
- **Projection Tests**: Read model consistency and event replay accuracy
- **Performance Tests**: Event sourcing performance and optimization validation

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
- **ENDPOINT MATCHING**: Ensure client calls correct endpoints
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

**COMMAND PATTERN**: Commands that track user actions should use `Guid` for user identifiers (InitiatedBy, CancelledBy, CreatedBy, etc.), not strings

### 7. Frontend Component Architecture - Questionnaire Rendering

**CRITICAL**: **NEVER** write inline question rendering logic. **ALWAYS** use OptimizedQuestionRenderer for all question rendering.

For detailed component architecture guidance, see: [docs/frontend/component-architecture.md](docs/frontend/component-architecture.md)

### 8. Enum Explicit Values Pattern
- **ALWAYS** set integer values explicitly for ALL enum members
- **NEVER** rely on implicit enum numbering
- **REASON**: Prevents serialization bugs when enum definitions differ across layers (CQRS/Event Sourcing architecture)
- **CRITICAL**: This codebase uses separate assemblies for Domain, Application, CommandApi, QueryApi, and Frontend - implicit enum values can lead to ordering mismatches

**Rule**: Set explicit integer values for ALL enums in this codebase, even if they seem to have a natural sequential order.

### 9. Domain Validation Pattern - Template Configuration vs Response Data
- **CRITICAL**: When validating questionnaire responses, you must validate response data AGAINST template configuration
- **NEVER** validate only the response data in isolation
- **ALWAYS** extract the expected structure from template configuration, then verify ALL required elements are present in the response
- **PRINCIPLE**: Follows DDD's Information Expert pattern - the aggregate owns the data and knows how to validate itself using the template as a specification



### 10. Authorization Pattern - Consistent Page-Level Security

**CRITICAL**: All admin pages use `<AuthorizeView Policy="...">` pattern for consistent user experience and clear access control.

**Rules**:
- **ALWAYS** use `<AuthorizeView Policy="...">` for admin pages
- **NEVER** use `@attribute [Authorize]` for admin pages (causes blank pages for unauthorized users)

For detailed authorization implementation guidance, see: [docs/implementation/authorization-patterns.md](docs/implementation/authorization-patterns.md)

## Translation System Guidelines

For detailed translation implementation guidance, see: [docs/implementation/translation-system.md](docs/implementation/translation-system.md)

**Core Rules**:
- **CRITICAL**: When adding new UI text that uses `@T("translation.key")`, you MUST add the translation to test-translations.json IMMEDIATELY
- Always validate translations before committing: `powershell -ExecutionPolicy Bypass -File TestDataGenerator\validate-translations.ps1`
- Use semantic translation keys with category prefixes (pages.*, sections.*, buttons.*, etc.)
- Use Unicode escape sequences for German umlauts in JSON: ä → `\u00E4`

## Typography System Guidelines

**CRITICAL**: **NEVER** hardcode font-weight values. **ALWAYS** use semantic CSS variables (e.g., `--font-weight-heading`, `--font-weight-body`).

For detailed typography system guidance, see: [docs/frontend/typography-system.md](docs/frontend/typography-system.md)

## 11. Strongly-Typed Question Configuration Pattern

**CRITICAL**: **NEVER** use `Dictionary<string, object>` for question configurations. **ALWAYS** use strongly-typed configuration classes.

**Pattern**: Use pattern matching with the `is` operator to access configuration properties.

For detailed question configuration implementation guidance, see: [docs/implementation/question-configuration.md](docs/implementation/question-configuration.md)

## 12. Configuration Serialization Pattern

**CRITICAL**: **NEVER** remove the `$type` discriminator from question configurations - it's defensive validation, not redundancy.

For detailed configuration serialization guidance, see: [docs/implementation/configuration-serialization.md](docs/implementation/configuration-serialization.md)

## 13. Controller Pattern

**CRITICAL**:
- **NEVER** wrap controller actions in try-catch blocks (let middleware handle exceptions)
- **ALWAYS** use `CreateResponse(result)` for API responses
- **ALWAYS** use `ExecuteWithAuthorizationAsync` for manager-restricted endpoints
- **ALWAYS** use dedicated enrichment services to avoid N+1 query problems

For detailed API controller implementation guidance, see: [docs/implementation/api-controller-patterns.md](docs/implementation/api-controller-patterns.md)

## 14. Infrastructure Configuration Pattern

**CRITICAL**: Prevent resource conflicts through centralized configuration.

**Database Connection Management**:
- **NEVER** register NpgsqlDataSource in individual Program.cs files
- **ALWAYS** use shared registration via Extensions.cs → AddMartenInfrastructure()
- **REASON**: Multiple registrations cause connection pool conflicts

**Exception Handling**:
- **NEVER** wrap controller actions in try-catch blocks
- **ALWAYS** let GlobalExceptionMiddleware handle all exceptions
- **PATTERN**: Consistent ProblemDetails responses with correlation tracking

**Cache Configuration**:
- **ALWAYS** make cache TTL values configurable via IOptions<T>
- **NEVER** hardcode timeout values in service classes
- **PATTERN**: Environment-specific configuration for dev/staging/production

## 15. Performance Optimization Pattern

**CRITICAL**: Optimize for scale through category-based loading and composition.

**Translation Loading**:
- **NEVER** load all 978 translation keys per component
- **ALWAYS** use CategoryOptimizedTranslatableComponent with category specification
- **PATTERN**: Override GetRequiredTranslationCategories() to specify needed keys
- **PERFORMANCE**: Targets 75-87% reduction in network payload

**Component Architecture**:
- **PREFER** composition over deep inheritance hierarchies
- **PATTERN**: Use service interfaces (IComponentPerformanceService) over base class methods
- **BENEFITS**: Better testability, clearer separation of concerns

## 16. Domain Testing Pattern

**CRITICAL**: Use standardized testing framework for all domain aggregate testing.

**Framework Usage**:
- **ALWAYS** inherit from AggregateTestBase<T> for domain aggregate tests
- **PATTERN**: Given(historical events) → When(business action) → Then(expected events)
- **LOCATION**: Tests/ti8m.BeachBreak.Domain.Tests/[AggregateName]/

**Event Assertions**:
- **PATTERN**: Use fluent assertions for event validation
- **EXAMPLE**: ThenEventWasRaised<EventType>().WithProperty(e => e.Property, expectedValue)
- **FEATURES**: Timestamp validation, aggregate state verification, exception testing

**Test Organization**:
- **STRUCTURE**: One test class per aggregate in matching folder structure
- **NAMING**: [Method]_[Condition]_[ExpectedBehavior] pattern
- **CATEGORIES**: Unit (single aggregate), Integration (multi-aggregate), Projection (read models)

---

## Documentation Structure

### Architectural Guidance (This File)
- **CLAUDE.md**: Universal patterns and core development rules
- Must be followed for all development work
- Focused on architectural consistency and quality

### Feature-Specific Documentation
- **[docs/domain/questionnaire-workflows.md](docs/domain/questionnaire-workflows.md)**: Workflow states, process types, state transitions
- **[docs/implementation/translation-system.md](docs/implementation/translation-system.md)**: Translation system implementation details
- **[docs/frontend/component-architecture.md](docs/frontend/component-architecture.md)**: Component patterns and OptimizedQuestionRenderer system

### Implementation Guides
- **docs/implementation/**: Detailed implementation guides for specific features
- **docs/planning/**: Planning documents and session summaries
- **docs/domain/**: Domain-specific documentation and business rules

### Testing Documentation
- **Tests/ti8m.BeachBreak.Domain.Tests/README.md**: Complete domain testing framework guide
- **Tests/ti8m.BeachBreak.Domain.Tests/Common/**: Testing framework implementation
- **Tests/ti8m.BeachBreak.Domain.Tests/[Aggregate]Tests.cs**: Example test implementations

### Quick Reference

**For new developers**: Start with CLAUDE.md for core patterns, then explore feature-specific docs
**For specific features**: Check docs/domain/ and docs/implementation/ for detailed guidance
**For component work**: See docs/frontend/component-architecture.md for comprehensive patterns
**For translations**: Use docs/implementation/translation-system.md for complete workflow
**For domain testing**: Use Tests/ti8m.BeachBreak.Domain.Tests/README.md for testing framework guide
**For performance optimization**: Follow patterns in sections 14-16 for infrastructure and component optimization

---

*Last Updated: 2026-02-08*
*Core Patterns Version: 3.0*
*Architecture Improvements: Infrastructure optimization, performance patterns, domain testing framework*