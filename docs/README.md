# ti8m BeachBreak Documentation

This directory contains comprehensive documentation for the ti8m BeachBreak questionnaire management system.

## Documentation Organization

### Core Architecture (Start Here)
- **[CLAUDE.md](../CLAUDE.md)** - Universal patterns and development rules that must be followed
- **[Project Overview](../CLAUDE.md#project-overview)** - System architecture, technology stack, and key patterns

### Domain Documentation
- **[Questionnaire Workflows](domain/questionnaire-workflows.md)** - Complete workflow states, process types, and state transitions
- **[Review Workflow](domain/review-workflow.md)** - Manager-led review process documentation
- **[Initialized State Implementation](domain/initialized-state-implementation.md)** - Detailed implementation of initialization phase

### Implementation Guides
- **[Translation System](implementation/translation-system.md)** - Complete translation implementation guide
- **[Question Configuration](implementation/question-configuration.md)** - Strongly-typed question configuration patterns
- **[Configuration Serialization](implementation/configuration-serialization.md)** - JSON serialization with $type discriminator
- **[API Controller Patterns](implementation/api-controller-patterns.md)** - Controller error handling and enrichment services
- **[Organization Questionnaires](implementation/organization-questionnaires.md)** - Organization-level questionnaire management
- **[Command Dispatcher](implementation/command-dispatcher.md)** - Command dispatcher implementation
- **[Query Dispatcher](implementation/query-dispatcher.md)** - Query dispatcher implementation
- **[Query Dispatcher Source Generator](implementation/query-dispatcher-source-generator.md)** - Source generator implementation
- **[AOT Implementation](implementation/aot-implementation.md)** - Ahead-of-time compilation implementation

### Frontend Architecture
- **[Component Architecture](frontend/component-architecture.md)** - Frontend component patterns and OptimizedQuestionRenderer system
- **[Typography System](frontend/typography-system.md)** - Font weight variables and styling guidelines
- **[Question Rendering Quick Reference](frontend/Question-Rendering-Quick-Reference.md)** - Quick guide for question component development

### Planning and Sessions
- **[Critical Fixes](planning/critical-fixes.md)** - Summary of critical fixes and their resolution
- **[Refactoring Sessions](planning/refactoring-sessions.md)** - Documentation of major refactoring efforts
- **[Progress Calculation](planning/progress-calculation.md)** - Progress calculation implementation plan
- **[Command Handler Split](planning/command-handler-split.md)** - Command handler refactoring plan
- **[CSS Cleanup Completion](planning/css-cleanup-completion.md)** - CSS cleanup project summary
- **[Refactoring Context](planning/refactoring-context.md)** - Context for major refactoring decisions
- **[Query Dispatcher Implementation](planning/query-dispatcher-implementation.md)** - Query dispatcher implementation summary

### Architecture Decisions
- **[ADR-001: Frontend Component Architecture](architecture/ADR-001-Frontend-Component-Architecture.md)** - Architectural decision record for frontend component design
- **[Questionnaire Section Visibility Rules](architecture/Questionnaire-Section-Visibility-Rules.md)** - Business rules for section visibility and response filtering

### Feature Documentation
- **[Dashboards](Dashboards.md)** - Dashboard features and specifications
- **[Design System](DesignSystem.md)** - UI design system guidelines
- **[Testing](TESTING.md)** - Testing strategies and guidelines

## Quick Navigation

### For New Developers
Start with these documents to understand the system:
1. **[CLAUDE.md](../CLAUDE.md)** - Read this first for core patterns and rules
2. **[Questionnaire Workflows](domain/questionnaire-workflows.md)** - Understand the business process
3. **[Component Architecture](frontend/component-architecture.md)** - Frontend development patterns

### For Feature Development
- **Business Rules**: Check `domain/` directory for workflow and business logic
- **Implementation Details**: Check `implementation/` directory for specific feature guides
- **Frontend Work**: Check `frontend/` directory for component patterns

### For Bug Fixes
1. **[Component Architecture](frontend/component-architecture.md)** - Common component issues and troubleshooting
2. **[Translation System](implementation/translation-system.md)** - Translation-related issues
3. **[Critical Fixes](planning/critical-fixes.md)** - Previously resolved critical issues

### For Code Review
- **[CLAUDE.md](../CLAUDE.md)** - Review against core development patterns
- **[Component Architecture](frontend/component-architecture.md)** - Frontend code review checklist
- **[Translation System](implementation/translation-system.md)** - Translation code review checklist

## Documentation Standards

### Writing Guidelines
1. **Start with overview** - Always begin with purpose and scope
2. **Use clear headings** - Hierarchical structure with consistent naming
3. **Include code examples** - Show correct and incorrect patterns
4. **Reference implementation** - Link to actual files and line numbers
5. **Historical context** - Explain why patterns exist and what problems they solve

### Mermaid Diagrams
- **Workflow diagrams**: Use `stateDiagram-v2` for state machines
- **Component diagrams**: Use `graph TD` for dependency flows
- **Sequence diagrams**: Use `sequenceDiagram` for process flows

### Cross-References
- **Internal links**: Use relative paths (e.g., `[Component Architecture](frontend/component-architecture.md)`)
- **Code references**: Include file paths and line numbers when referencing implementation
- **Pattern references**: Link between related patterns and their documentation

## Project Structure Reference

```
docs/
├── README.md                               # This file - navigation and overview
├── domain/                                 # Domain-specific documentation
│   ├── questionnaire-workflows.md         # Complete workflow and process type guide
│   ├── review-workflow.md                 # Manager-led review process
│   └── initialized-state-implementation.md # Initialization phase implementation
├── implementation/                         # Feature implementation guides
│   ├── translation-system.md              # Complete translation system guide
│   ├── organization-questionnaires.md     # Organization questionnaire management
│   ├── command-dispatcher.md              # Command dispatcher implementation
│   ├── query-dispatcher.md                # Query dispatcher implementation
│   ├── query-dispatcher-source-generator.md # Source generator implementation
│   └── aot-implementation.md              # AOT compilation implementation
├── frontend/                               # Frontend-specific documentation
│   ├── component-architecture.md          # Component patterns and architecture
│   └── Question-Rendering-Quick-Reference.md # Quick component development guide
├── planning/                               # Planning and session documentation
│   ├── critical-fixes.md                  # Critical fixes and resolution
│   ├── refactoring-sessions.md            # Major refactoring documentation
│   ├── progress-calculation.md            # Progress calculation planning
│   ├── command-handler-split.md           # Command handler refactoring
│   ├── css-cleanup-completion.md          # CSS cleanup project summary
│   ├── refactoring-context.md             # Refactoring decision context
│   └── query-dispatcher-implementation.md # Query dispatcher summary
├── architecture/                          # Architecture decisions and patterns
│   ├── ADR-001-Frontend-Component-Architecture.md
│   └── Questionnaire-Section-Visibility-Rules.md
├── Dashboards.md                          # Dashboard specifications
├── DesignSystem.md                        # UI design system guidelines
└── TESTING.md                             # Testing strategies and guidelines
```

## Changelog

### 2026-01-30: Documentation Reorganization
- **Streamlined CLAUDE.md**: Reduced from 1,094 to 513 lines, focused on universal patterns
- **Consolidated Workflows**: Created comprehensive workflow documentation in `domain/questionnaire-workflows.md`
- **Extracted Feature Details**: Moved implementation details to organized structure
- **Improved Navigation**: Updated README with clear structure and quick navigation
- **Preserved All Content**: No information lost, all details moved to appropriate locations

### Previous Updates
- **2026-01-15**: Process Types feature documentation
- **2026-01-13**: Auto-Initialization feature documentation
- **2026-01-06**: Initialized Workflow State documentation
- **2025-12-04**: Translation system documentation
- **2025-11-21**: Component architecture documentation

## Contributing to Documentation

### Adding New Documentation
1. **Choose appropriate directory**:
   - `domain/` - Business rules, workflow states, domain concepts
   - `implementation/` - Feature implementation guides and technical details
   - `frontend/` - Component patterns, UI architecture, development guidelines
   - `planning/` - Planning documents, session notes, decision context
   - `architecture/` - ADRs and architectural patterns

2. **Follow naming conventions**:
   - Use kebab-case for file names: `questionnaire-workflows.md`
   - Use descriptive, specific names: `translation-system.md` not `translations.md`
   - Group related docs in subdirectories

3. **Update navigation**:
   - Add link to this README.md
   - Add cross-references in related documents
   - Update CLAUDE.md if it affects core patterns

### Documentation Quality Standards
- **Start with Table of Contents** for documents > 100 lines
- **Include Quick Reference** sections for complex topics
- **Provide code examples** showing correct and incorrect patterns
- **Add troubleshooting sections** for complex features
- **Include historical context** explaining why patterns exist
- **Cross-reference related documentation** to prevent duplication

## Glossary

### Domain Terms
- **CompletionRole**: Which party (Employee, Manager, or Both) is assigned to complete a questionnaire section
- **ResponseRole**: Which party's responses within a "Both" section (Employee or Manager responses)
- **WorkflowState**: Current state in the questionnaire lifecycle (e.g., InProgress, Submitted, InReview, Finalized)
- **ApplicationRole**: User's organizational role (Employee, TeamLead, HR, HRLead, Admin)
- **ProcessType**: Type of questionnaire process (PerformanceReview, Survey) with different business rules

### Technical Terms
- **OptimizedQuestionRenderer**: Master component for all question rendering
- **Configuration Serialization**: Polymorphic JSON with $type discriminator
- **Event Sourcing**: Domain events as source of truth
- **CQRS**: Command Query Responsibility Segregation
- **UserContext**: Service for authenticated user identification

### Workflow Terms
- **Phase 1 Read-Only**: Temporary read-only state after submission, before review meeting
- **Phase 2 Read-Only**: Permanent read-only state after finalization
- **Auto-Initialization**: Template feature that skips manual initialization phase
- **Custom Sections**: Instance-specific questions added during initialization
- **Predecessor Linking**: Connecting questionnaires for goal tracking

---

*Last Updated: 2026-01-30*
*Documentation Structure Version: 2.0*