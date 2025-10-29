# ti8m BeachBreak Documentation

This directory contains comprehensive documentation for the ti8m BeachBreak questionnaire management system.

## Architecture Documentation

### Core Architecture
- **[Questionnaire Section Visibility Rules](architecture/Questionnaire-Section-Visibility-Rules.md)** - Business rules for section visibility and response filtering throughout the questionnaire lifecycle
- **[ADR-001: Frontend Component Architecture](architecture/ADR-001-Frontend-Component-Architecture.md)** - Architectural decision record for frontend component design

### Workflow Documentation
- **[WorkflowDiagram.md](../WorkflowDiagram.md)** - Visual state diagram and workflow transitions
- **[questionnaire_workflow.md](../questionnaire_workflow.md)** - High-level questionnaire workflow overview

## Feature Documentation

### Dashboards
- **[Dashboards.md](Dashboards.md)** - Dashboard features and specifications

### Frontend
- See `frontend/` directory for frontend-specific documentation

## Quick Links

### For Developers
Start here to understand the system architecture:
1. [questionnaire_workflow.md](../questionnaire_workflow.md) - Understand the business process
2. [WorkflowDiagram.md](../WorkflowDiagram.md) - See the state machine
3. [Questionnaire Section Visibility Rules](architecture/Questionnaire-Section-Visibility-Rules.md) - Understand data access control

### For Business Analysts
1. [questionnaire_workflow.md](../questionnaire_workflow.md) - Business process overview
2. [Questionnaire Section Visibility Rules](architecture/Questionnaire-Section-Visibility-Rules.md) - Who sees what, when

### For QA/Testing
1. [Questionnaire Section Visibility Rules](architecture/Questionnaire-Section-Visibility-Rules.md) - See "Testing Scenarios" section
2. [WorkflowDiagram.md](../WorkflowDiagram.md) - See "Actions by State" table

## Project Structure Reference

```
docs/
├── README.md                          # This file
├── architecture/                      # Architecture decisions and patterns
│   ├── ADR-001-Frontend-Component-Architecture.md
│   └── Questionnaire-Section-Visibility-Rules.md
├── frontend/                          # Frontend-specific docs
├── Dashboards.md                      # Dashboard specifications
../WorkflowDiagram.md                  # Workflow state diagram (root)
../questionnaire_workflow.md           # Workflow overview (root)
```

## Contributing to Documentation

When adding new documentation:
1. Place architectural decisions in `architecture/` with ADR prefix
2. Place feature-specific docs in the root `docs/` folder
3. Update this README with links to new documents
4. Use Markdown format for consistency
5. Include diagrams where helpful (mermaid format preferred)

## Glossary

- **CompletionRole**: Which party (Employee, Manager, or Both) is assigned to complete a questionnaire section
- **ResponseRole**: Which party's responses within a "Both" section (Employee or Manager responses)
- **WorkflowState**: Current state in the questionnaire lifecycle (e.g., InProgress, Submitted, InReview, Finalized)
- **ApplicationRole**: User's organizational role (Employee, TeamLead, HR, HRLead, Admin)
- **Phase 1 Read-Only**: Temporary read-only state after submission, before review meeting
- **Phase 2 Read-Only**: Permanent read-only state after finalization
