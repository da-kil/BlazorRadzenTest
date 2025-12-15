# Multi-Source Employee Feedback System Implementation Plan

## Overview
Implement a comprehensive feedback system to record external feedback for employees from multiple sources: customers, peers, and project colleagues. The system will leverage existing questionnaire infrastructure for UI consistency while maintaining domain separation and supporting flexible feedback criteria.

## Requirements Summary
- **Employee Scope**: Existing employees only
- **Recording Rights**: HR staff and Team leads/managers
- **Source Types**: Customer, Peer, Project Colleague feedback (unified system with tagging)
- **Flexible Criteria**: Configurable evaluation criteria per feedback entry (leveraging existing questionnaire/template system)
- **Project Context**: Track project information and feedback provider role/context
- **Integration**: Independent system with read-only integration during questionnaire reviews
- **Data Structure**: Configurable rating criteria (1-10 scale) + flexible comment sections
- **External Data Entry**: Interface to record feedback collected outside the application

## Implementation Strategy

### Phase 1: Domain Foundation

#### 1.1 Create EmployeeFeedback Aggregate
**Location**: `01_Domain/ti8m.BeachBreak.Domain/EmployeeFeedbackAggregate/`

**Files to create:**
- `EmployeeFeedback.cs` - Main aggregate root with business logic
- `Events/EmployeeFeedbackRecorded.cs` - Domain event for initial feedback
- `Events/EmployeeFeedbackUpdated.cs` - Domain event for modifications
- `Events/EmployeeFeedbackDeleted.cs` - Domain event for soft deletion
- `ValueObjects/FeedbackSource.cs` - Source type (Customer, Peer, Project Colleague)
- `ValueObjects/FeedbackProviderInfo.cs` - Provider name, role, project context
- `ValueObjects/ConfigurableFeedbackData.cs` - Flexible rating and comment structure
- `FeedbackSourceType.cs` - Enum for source types

**Key Business Rules:**
- Must record feedback source type, provider info, date
- Support configurable evaluation criteria per feedback entry
- Ratings 1-10 or null for "not rated"
- Track project context for Project Colleague feedback
- At least one rating OR one comment required
- Authorization via UserContext (HR/TeamLead roles)

#### 1.2 Extend Question Configuration System
**Location**: `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/`

**Files to modify:**
- `QuestionType.cs` - Add `EmployeeFeedback = 3` enum value
- Create `EmployeeFeedbackConfiguration.cs` implementing `IQuestionConfiguration`
- Create `FeedbackTemplateManager.cs` - Service to manage configurable feedback templates

**Strategy**:
- Reuse existing `EvaluationItem` and `TextSectionDefinition` patterns for UI consistency
- Support configurable criteria selection per feedback entry
- Default templates for Customer/Peer/Project Colleague feedback types
- Ability to customize criteria on-the-fly during data entry

### Phase 2: Application Layer

#### 2.1 Commands
**Location**: `02_Application/ti8m.BeachBreak.Application.Command/Commands/EmployeeFeedbackCommands/`

**Files to create:**
- `RecordEmployeeFeedbackCommand.cs` - Primary data entry command with source type and configurable criteria
- `UpdateEmployeeFeedbackCommand.cs` - Modify existing feedback
- `DeleteEmployeeFeedbackCommand.cs` - Soft delete with audit trail
- `RecordEmployeeFeedbackCommandHandler.cs` - Business logic and validation
- `UpdateEmployeeFeedbackCommandHandler.cs` - Update logic with authorization
- `DeleteEmployeeFeedbackCommandHandler.cs` - Deletion logic

#### 2.2 Queries and Read Models
**Location**: `02_Application/ti8m.BeachBreak.Application.Query/`

**Files to create:**
- `Projections/EmployeeFeedbackReadModel.cs` - Denormalized view with source type filtering
- `Queries/EmployeeFeedbackQueries/GetEmployeeFeedbackQuery.cs` - Main listing query with source type filters
- `Queries/EmployeeFeedbackQueries/GetCurrentYearFeedbackQuery.cs` - Questionnaire integration
- `Queries/EmployeeFeedbackQueries/GetFeedbackByIdQuery.cs` - Single record retrieval
- `Queries/EmployeeFeedbackQueries/GetFeedbackTemplatesQuery.cs` - Available feedback templates/criteria

#### 2.3 Repository Interface
**Location**: `02_Application/ti8m.BeachBreak.Application.Command/Repositories/`

**Files to create:**
- `IEmployeeFeedbackAggregateRepository.cs` - Repository contract

### Phase 3: Infrastructure Layer

#### 3.1 API Controllers
**Location**: `03_Infrastructure/ti8m.BeachBreak.CommandApi/Controllers/`

**Files to create:**
- `EmployeeFeedbackController.cs` - Command API with `[Authorize(Policy = "HR")]`

**Location**: `03_Infrastructure/ti8m.BeachBreak.QueryApi/Controllers/`

**Files to create:**
- `EmployeeFeedbackQueryController.cs` - Query API with `[Authorize(Policy = "TeamLead")]`

#### 3.2 Data Transfer Objects
**Location**: `03_Infrastructure/ti8m.BeachBreak.CommandApi/Dto/`

**Files to create:**
- `RecordEmployeeFeedbackDto.cs` - Command payload with source type, provider info, project context
- `UpdateEmployeeFeedbackDto.cs` - Update payload
- `FeedbackProviderInfoDto.cs` - Provider name, role, project details
- `ConfigurableFeedbackDataDto.cs` - Dynamic rating and comment structure
- `FeedbackSourceType.cs` - Enum: Customer, Peer, ProjectColleague

#### 3.3 Repository Implementation
**Location**: `03_Infrastructure/ti8m.BeachBreak.Infrastructure.Marten/`

**Files to create:**
- `EmployeeFeedbackAggregateRepository.cs` - Marten-based implementation

### Phase 4: Frontend Implementation

#### 4.1 Pages
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Pages/`

**Files to create:**
- `EmployeeFeedbackManagement.razor` - Main management interface with source type filtering
- `RecordEmployeeFeedback.razor` - Data entry form with dynamic criteria selection
- `EditEmployeeFeedback.razor` - Edit existing feedback

**Authorization Pattern**: Use `<AuthorizeView Policy="HR">` with `AccessDeniedComponent`

#### 4.2 Components
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Components/EmployeeFeedback/`

**Files to create:**
- `OptimizedEmployeeFeedbackRenderer.razor` - Main dispatcher component
- `FeedbackSourceSelector.razor` - Source type selection (Customer/Peer/Project Colleague)
- `ConfigurableFeedbackSection.razor` - Dynamic criteria selection and rating
- `FeedbackProviderInfoSection.razor` - Provider details and project context
- `EmployeeFeedbackSummary.razor` - Read-only display for questionnaire reviews
- `EmployeeFeedbackDisplayCard.razor` - Individual feedback display with source type badges

#### 4.3 Services
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Services/`

**Files to create:**
- `IEmployeeFeedbackApiService.cs` - Service interface
- `EmployeeFeedbackApiService.cs` - HTTP client implementation
- `IFeedbackTemplateService.cs` - Template management service
- `FeedbackTemplateService.cs` - Default templates and criteria management

#### 4.4 Models and DTOs
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Models/Dto/`

**Files to create:**
- Mirror all API DTOs for type-safe client-server communication
- `EmployeeFeedbackSummaryDto.cs` - Display data with source type information
- `FeedbackQueryParams.cs` - Query parameters with source type filtering
- `FeedbackTemplateDto.cs` - Available criteria templates

#### 4.5 Question Handler
**Location**: `05_Frontend/ti8m.BeachBreak.Client/Services/QuestionHandlers/`

**Files to create:**
- `EmployeeFeedbackQuestionHandler.cs` - Implements `IQuestionTypeHandler`

### Phase 5: Translation and UI Polish

#### 5.1 Translation Keys
**Location**: `TestDataGenerator/test-translations.json`

**Add translation keys:**
- `pages.employee-feedback-management`
- `sections.feedback-source`
- `labels.customer-feedback`, `labels.peer-feedback`, `labels.project-colleague-feedback`
- `labels.feedback-provider`, `labels.provider-role`, `labels.project-context`
- `sections.configurable-criteria`
- `labels.overall-satisfaction`, `labels.leadership-behavior`, `labels.technical-skills`, etc. (default criteria)
- `labels.positive-impressions`, `labels.potential-improvement`, `labels.general-comments`
- `buttons.add-criterion`, `buttons.record-feedback`, `buttons.save-feedback`
- `messages.feedback-recorded-successfully`
- `filters.filter-by-source-type`

#### 5.2 Questionnaire Integration
**Files to modify:**
- `05_Frontend/ti8m.BeachBreak.Client/Components/Shared/QuestionnaireReviewMode.razor` - Add multi-source feedback summary section
- Review data fetching services to include current year feedback with source type filtering

### Phase 6: Authorization and Security

#### 6.1 Policy Configuration
**Files to modify:**
- Verify existing HR and TeamLead policies support the access patterns
- Implement team hierarchy checking for TeamLead access

#### 6.2 User Context Integration
**Pattern**: Use `UserContext.Id` (parsed to Guid) for all command authorization

## Key Implementation Details

### Domain Event Patterns
- Use past-tense naming: `EmployeeFeedbackRecorded`, `EmployeeFeedbackUpdated`
- Include rich business context: who recorded, when, for which employee, source type, provider info
- Apply pattern with reflection for event sourcing reconstruction

### UI Component Reuse Strategy
- Leverage `EvaluationRatingItem` for 1-10 rating scales with star display
- Use `OptimizedTextQuestion` patterns for comment sections
- Follow `OptimizedQuestionRenderer` dispatcher pattern to prevent code duplication
- Support dynamic criteria selection and configuration per feedback entry

### Authorization Flow
1. JWT provides user ID via `UserContext`
2. ApplicationRole looked up from database per request
3. HR policy allows access to all employee feedback (all source types)
4. TeamLead policy allows access to team hierarchy only (all source types)
5. Server-side validation on all endpoints

### Multi-Source Support Strategy
- Unified aggregate handles Customer, Peer, and Project Colleague feedback
- Source type tagging for filtering and categorization
- Configurable criteria per feedback entry (not fixed to 7 criteria)
- Project context tracking for Project Colleague feedback
- Provider role/context information for all source types

### Questionnaire Integration Points
- Add `EmployeeFeedbackSummary` component in review mode showing all source types
- Query current year feedback during review data loading with source type breakdown
- Display read-only feedback cards below questionnaire responses with source type badges
- Filter/group by source type in review interface
- No workflow integration - feedback remains independent

## Critical Files for Initial Implementation

1. `01_Domain/ti8m.BeachBreak.Domain/EmployeeFeedbackAggregate/EmployeeFeedback.cs`
2. `02_Application/ti8m.BeachBreak.Application.Command/Commands/EmployeeFeedbackCommands/RecordEmployeeFeedbackCommand.cs`
3. `05_Frontend/ti8m.BeachBreak.Client/Pages/EmployeeFeedbackManagement.razor`
4. `04_Core/ti8m.BeachBreak.Core.Domain/QuestionConfiguration/EmployeeFeedbackConfiguration.cs`
5. `05_Frontend/ti8m.BeachBreak.Client/Components/EmployeeFeedback/OptimizedEmployeeFeedbackRenderer.razor`
6. `05_Frontend/ti8m.BeachBreak.Client/Components/EmployeeFeedback/FeedbackSourceSelector.razor`
7. `05_Frontend/ti8m.BeachBreak.Client/Services/FeedbackTemplateService.cs`

## Success Criteria

- HR/TeamLead users can record external feedback from multiple sources (Customer, Peer, Project Colleague) through intuitive UI
- Support configurable evaluation criteria per feedback entry, not restricted to fixed 7 criteria
- Feedback displays consistently using existing questionnaire UI components
- Source type filtering and categorization in management interface
- Project context tracking for Project Colleague feedback
- Provider role/context information captured for all feedback types
- Current year feedback visible during questionnaire reviews (read-only) with source type breakdown
- Complete audit trail via event sourcing
- Role-based authorization prevents unauthorized access
- Multilingual support (English/German)
- Integration with existing notification and validation systems

## Risk Mitigation

- **Domain Separation**: EmployeeFeedback as independent aggregate prevents coupling with questionnaire workflows
- **UI Consistency**: Reusing existing components prevents design divergence
- **Flexible Configuration**: Configurable criteria system prevents rigid limitations while leveraging questionnaire infrastructure
- **Authorization**: Server-side validation with existing role infrastructure
- **Data Integrity**: Event sourcing provides complete audit trail and recovery options
- **Performance**: CQRS allows independent read/write scaling
- **Multi-Source Complexity**: Unified aggregate with source type tagging simplifies domain model while supporting diverse feedback sources