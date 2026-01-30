# Organization Questionnaires Page - Implementation Details

## Overview
The Organization Questionnaires page is an HR-specific view that displays comprehensive questionnaire management and tracking for the entire organization. It uses a modular, component-based architecture with a reusable configuration pattern.

## Page Architecture

### Main Page Component
File: 05_Frontend/ti8m.BeachBreak.Client/Pages/OrganizationQuestionnaires.razor

- Route: /organization/questionnaires
- Authorization: Requires HR policy
- Inheritance: BaseQuestionnaireListPage (template method pattern)
- Key Responsibilities:
  - Load organization data (organizations, assignments, employees, templates)
  - Create HR-specific page configuration
  - Configure action button handlers (Export, Analytics)
  - Render generic list page with HR configuration

Data Loading (LoadRoleSpecificDataAsync):
- Loads 4 parallel data streams:
  1. All HR assignments
  2. All employees
  3. All organizations
  4. All questionnaire templates

---

## Filter Components

### QuestionnaireFilterBar Component
File: 05_Frontend/ti8m.BeachBreak.Client/Components/Shared/QuestionnaireFilterBar.razor

Main Filters:
- Search: Full-text search (employees, emails, questionnaires)
- Organization/Department: Multi-select dropdown
  Placeholder: "Select Organization..."
  Data: From configuration's Department filter options
  
- Role: Multi-select dropdown (visible on some pages)

Advanced Filters Toggle:
- Icon: tune (shows "More Filters" button when advanced filters exist)
- Toggles visibility of AdvancedFiltersPanel

### AdvancedFiltersPanel Component
File: 05_Frontend/ti8m.BeachBreak.Client/Components/Shared/AdvancedFiltersPanel.razor

Advanced Filters:
- Category Filter: Multi-select from available categories
- Template Filter: Multi-select questionnaire templates
- Status Filter: Multi-select workflow statuses
  Options: Assigned, In Progress, Submitted, In Review, Finalized, Overdue
- Date Range Filter: From/To date pickers

---

## Configuration System

### HR Configuration Details
Created by: QuestionnairePageConfigurationFactory.CreateHRConfiguration()

Page Metadata:
- Title: Organization Questionnaires
- Description: Comprehensive overview of all questionnaire activities across the organization
- Icon: corporate_fare
- Route: /organization/questionnaires
- Page Type: QuestionnairePageType.HR

Tabs (4 tabs):
1. Department Overview
2. Employee Status
3. Questionnaire Performance
4. Analytics & Insights

Filters (6 filters):
1. Search - Always visible
2. Organization/Department - Multi-select
   Options: "{OrgNumber} - {OrgName}" format
   Filtered to exclude deleted/ignored organizations
3. Category - Multi-select (visible if categories exist)
4. Questionnaire - Multi-select templates (visible if templates exist)
5. Status - Multi-select workflow statuses
6. Due Date Range - Always visible

Actions (3 actions):
1. Refresh - Light style
2. Export Report - Info style (generates CSV)
3. Analytics Dashboard - Primary style

Stats Cards (6 cards):
1. Employees (total count)
2. Questionnaires (total templates)
3. Total Assignments
4. Pending (non-finalized)
5. Completed (finalized)
6. Overdue

---

## Filter Type Enumeration

QuestionnaireFilterType enum values:
- Search = 0
- Category = 1
- Template = 2
- Status = 3
- Department = 4 (Organization filter for HR)
- DateRange = 5
- Role = 6

---

## Base Class Architecture

BaseQuestionnaireListPage uses Template Method Pattern:

Abstract Methods (must be implemented):
1. LoadRoleSpecificDataAsync() - Load role-specific data
2. CreateConfiguration() - Create page configuration
3. GetInitializationContext() - Return context name

Virtual Methods (can be overridden):
1. ConfigureActions() - Set up action button handlers
2. HasAdditionalStateChanged() - Check state changes
3. RefreshData() - Handle refresh requests

---

## Organization Filter Data Flow

How Organization Filter Works:
1. Data Loading - OrganizationService.GetAllOrganizationsAsync(excludeDeleted, excludeIgnored)
2. Configuration - Factory creates filter options as "{org.Number} - {org.Name}"
3. Visibility - Department filter always visible in HR configuration
4. Selection - Multi-select allows selecting one or more organizations
5. Filtering - Selected organizations filter displayed data

---

## Export Functionality

Method: ExportOrganizationReport() in OrganizationQuestionnaires.razor

Generates CSV Report with:
- Header: Report metadata (generated date, totals)
- Summary: Employee count, assignment count, completion rates
- Detail: Employee data by row (name, email, department, role, stats)

File Format: Organization_Questionnaires_Report_{yyyy-MM-dd_HH-mm-ss}.csv

---

## File Locations Summary

Main Files:
- OrganizationQuestionnaires.razor (Page)
- BaseQuestionnaireListPage.cs (Base class)
- GenericQuestionnaireListPage.razor (Shared UI component)
- QuestionnaireFilterBar.razor (Filter UI)
- AdvancedFiltersPanel.razor (Advanced filters)
- QuestionnairePageConfigurationFactory.cs (Configuration)

Model Files:
- QuestionnairePageConfiguration.cs
- QuestionnairePageFilter.cs
- QuestionnairePageAction.cs
- QuestionnairePageTab.cs
- QuestionnaireFilterType.cs (enum)
- QuestionnairePageType.cs (enum)
- QuestionnaireTabType.cs (enum)

All files located in: 05_Frontend/ti8m.BeachBreak.Client/

