# Dashboard System Documentation

## Overview

The BeachBreak application implements a three-tiered dashboard system that provides role-based metrics and insights for questionnaire management. Each dashboard is tailored to specific user roles and provides relevant data aggregated from the event-sourced system.

## Architecture

All dashboards use **on-demand aggregation** rather than event-sourced projections:
- Data is queried from existing read models (EmployeeReadModel, QuestionnaireAssignmentReadModel, OrganizationReadModel)
- Aggregation happens in memory at query time via repository pattern
- Always shows current, accurate data without projection lag
- Simpler to maintain than separate projection handlers

---

## 1. Employee Dashboard

### Purpose
Provides individual employees with a personal overview of their questionnaire assignments and completion status.

### Access Control
- **Route**: `/dashboard`
- **Roles**: All authenticated employees (Employee, TeamLead, HR, HRLead, Admin)
- **Authorization Policy**: "Employee"
- **Scope**: Personal data only

### Key Features

#### Personal Metrics
- **Pending Assignments**: Count of questionnaires waiting to be started
- **In Progress**: Count of active questionnaires being worked on
- **Completed**: Count of finalized questionnaires

#### Urgent Assignments Section
Shows assignments that are:
- Due within 3 days, OR
- Already overdue
- Ordered by due date (earliest first)
- Limited to 10 most urgent items

#### Data Displayed per Urgent Assignment
- Questionnaire template name
- Manager name
- Organization name
- Due date
- Workflow state (e.g., "EmployeeInProgress", "ManagerInProgress")
- Overdue indicator (badge)
- Days until due / days overdue

### Implementation Details

**Backend:**
- Repository: `IEmployeeDashboardRepository` / `EmployeeDashboardRepository`
- Query: `EmployeeDashboardQuery` with EmployeeId parameter
- Handler: `EmployeeDashboardQueryHandler`
- API Endpoint: `GET /q/api/v1/employees/me/dashboard`
- Read Model: `EmployeeDashboardReadModel`

**Frontend:**
- Component: `Dashboard.razor`
- Service: `IEmployeeApiService.GetMyDashboardAsync()`
- DTO: `EmployeeDashboardDto`

**Data Sources:**
- Employee's own assignments from `QuestionnaireAssignmentReadModel`
- Employee details from `EmployeeReadModel`
- Manager information lookup
- Template names from `QuestionnaireTemplateReadModel`

### Use Cases
1. Employee checks their workload at a glance
2. Employee identifies urgent items requiring immediate attention
3. Employee sees which questionnaires are waiting for manager review
4. Employee tracks overall completion progress

---

## 2. Manager Dashboard

### Purpose
Provides team leads with an overview of their direct reports' questionnaire progress and team-wide metrics.

### Access Control
- **Route**: `/manager-dashboard`
- **Roles**: TeamLead, HR, HRLead, Admin
- **Authorization Policy**: "TeamLead"
- **Scope**: Manager's direct reports only

### Key Features

#### Team-Wide Metrics
- **Team Pending Count**: Total pending assignments across all team members
- **Team In Progress Count**: Total active assignments being worked on
- **Team Completed Count**: Total finalized assignments
- **Team Member Count**: Number of direct reports

#### Individual Team Member Cards
For each direct report:
- Employee name and email
- Organization
- Individual pending count
- Individual in-progress count
- Individual completed count
- Visual progress indicators

#### Team Urgent Assignments
Shows team-wide urgent assignments:
- Due within 3 days or overdue
- Across all direct reports
- Ordered by due date
- Shows which team member each assignment belongs to

### Implementation Details

**Backend:**
- Repository: `IManagerDashboardRepository` / `ManagerDashboardRepository`
- Query: `ManagerDashboardQuery` with ManagerId parameter
- Handler: `ManagerDashboardQueryHandler`
- API Endpoint: `GET /q/api/v1/managers/me/dashboard`
- Read Model: `ManagerDashboardReadModel`

**Frontend:**
- Component: `ManagerDashboard.razor`
- Service: `IManagerQuestionnaireService.GetMyDashboardAsync()`
- DTO: `ManagerDashboardDto`

**Data Sources:**
- Direct reports queried via `ManagerId` from `EmployeeReadModel`
- All assignments for team members from `QuestionnaireAssignmentReadModel`
- Manager information from current user context
- Aggregation of individual employee metrics

**Aggregation Logic:**
1. Query all employees where `ManagerId` matches current manager
2. Get all assignments for these employees
3. Aggregate counts by workflow state
4. Identify urgent items across the team
5. Calculate per-employee metrics

### Use Cases
1. Manager monitors team workload distribution
2. Manager identifies team members who may need support
3. Manager tracks team completion rates
4. Manager prioritizes urgent items across the team
5. Manager ensures timely review of submitted questionnaires

---

## 3. HR Dashboard

### Purpose
Provides HR personnel with comprehensive organization-wide analytics, metrics, and insights for strategic workforce management.

### Access Control
- **Route**: `/hr-dashboard`
- **Roles**: HR, HRLead, Admin
- **Authorization Policy**: "HR"
- **Scope**: Entire organization (all employees, managers, organizations)

### Key Features

#### Organization-Wide Metrics
- **Total Employees**: Count across all organizations
- **Total Managers**: Count of all team leads
- **Total Assignments**: All questionnaires assigned
- **Overall Completion Rate**: Percentage of completed assignments
- **Average Completion Time**: Average days from assignment to completion

#### Assignment Status Breakdown
- **Pending Assignments**: Across entire organization
- **In Progress Assignments**: Active questionnaires
- **Completed Assignments**: Finalized questionnaires
- **Overdue Assignments**: Past due date and not completed

#### Recent Activity (Last 7 Days)
- Assignments created in the past week
- Assignments completed in the past week
- Trend indicators for activity levels

#### Organization Breakdown
For each organizational unit:
- Organization name and employee count
- Total assignments for that organization
- Pending, In Progress, Completed counts
- Overdue count
- Organization-specific completion rate
- Visual metrics and progress indicators

#### Manager Overview
For each manager in the organization:
- Manager name and email
- Team size (direct reports)
- Total assignments managed
- Completed assignments
- Overdue assignments
- Manager's team completion rate
- Overdue badge if applicable

#### Organization-Wide Urgent Assignments
- All urgent assignments across the entire company
- Due within 3 days or overdue
- Shows employee, manager, organization context
- Ordered by due date
- Top 20 most urgent items

### Implementation Details

**Backend:**
- Repository: `IHRDashboardRepository` / `HRDashboardRepository`
- Query: `HRDashboardQuery` (no parameters - system-wide)
- Handler: `HRDashboardQueryHandler`
- API Endpoint: `GET /q/api/v1/hr/dashboard`
- Read Model: `HRDashboardReadModel`

**Frontend:**
- Component: `HRDashboard.razor`
- Service: `IHRApiService.GetHRDashboardAsync()`
- DTO: `HRDashboardDto`

**Data Sources:**
- All employees from `EmployeeReadModel` (excluding deleted)
- All assignments from `QuestionnaireAssignmentReadModel`
- All organizations from `OrganizationReadModel` (excluding deleted/ignored)
- Template names from `QuestionnaireTemplateReadModel`
- Complex aggregations across all entities

**Aggregation Logic:**
1. Query all non-deleted employees
2. Identify managers by `ApplicationRole.TeamLead`
3. Query all assignments across the organization
4. Calculate organization-wide metrics
5. Group and aggregate by organization
6. Calculate per-manager team metrics
7. Compute completion rates and averages
8. Identify top 20 urgent items across all assignments

**Nested DTOs:**
- `OrganizationMetricsDto`: Per-organization breakdown
- `ManagerOverviewDto`: Per-manager team metrics
- `UrgentAssignmentDto`: Urgent assignment details (shared across all dashboards)

### Use Cases
1. HR monitors organizational health and compliance
2. HR identifies organizations with low completion rates
3. HR tracks manager effectiveness across the company
4. HR plans resource allocation based on workload
5. HR generates reports for leadership
6. HR identifies systemic bottlenecks
7. HR ensures timely completion of company-wide initiatives
8. HR monitors overdue assignments requiring intervention

---

## Shared Components

### UrgentAssignmentDto
Used by all three dashboards to display urgent items:

```csharp
public class UrgentAssignmentDto
{
    public Guid AssignmentId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string ManagerName { get; set; }
    public string QuestionnaireTemplateName { get; set; }
    public DateTime DueDate { get; set; }
    public string WorkflowState { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysUntilDue { get; set; }
    public string OrganizationName { get; set; }
}
```

### Urgency Logic
Consistent across all dashboards:
- **Urgent Threshold**: 3 days from current date
- **Overdue**: DueDate < Current Date
- **Calculation**: `(DueDate - DateTime.UtcNow).TotalDays`
- **Exclusion**: Finalized assignments are never urgent

### Workflow States Tracked
All dashboards track these states from `WorkflowState` enum:
- **Assigned** (Pending): Not yet started
- **EmployeeInProgress**: Employee working on it
- **ManagerInProgress**: Manager reviewing
- **BothInProgress**: Both have edits in progress
- **Finalized**: Completed and locked

---

## Navigation

### Role-Based Dashboard Routing
The navigation menu automatically routes users to the appropriate dashboard based on their role:

```razor
<AuthorizeView Policy="HR">
    <Authorized>
        <NavLink href="hr-dashboard">Dashboard</NavLink>
    </Authorized>
    <NotAuthorized>
        <AuthorizeView Policy="TeamLead">
            <Authorized>
                <NavLink href="manager-dashboard">Dashboard</NavLink>
            </Authorized>
            <NotAuthorized>
                <NavLink href="dashboard">Dashboard</NavLink>
            </NotAuthorized>
        </AuthorizeView>
    </NotAuthorized>
</AuthorizeView>
```

**Routing Priority:**
1. HR/HRLead/Admin → HR Dashboard
2. TeamLead → Manager Dashboard
3. Employee → Employee Dashboard

---

## Performance Considerations

### On-Demand Aggregation
- **Pros:**
  - Always current and accurate
  - No stale data from projection lag
  - Simpler architecture with fewer moving parts
  - No need for projection rebuild after schema changes

- **Cons:**
  - Higher query time for HR dashboard (queries all data)
  - No caching (regenerates on every request)

### Optimization Strategies
Currently implemented:
1. Efficient Marten LINQ queries with proper filtering
2. In-memory aggregation using LINQ
3. Limited result sets for urgent assignments (10-20 items)
4. Single database round-trip per query

Future optimizations if needed:
1. Response caching with short TTL (30-60 seconds)
2. Materialized views for HR dashboard
3. Redis caching for frequently accessed dashboards
4. Background job to pre-calculate HR metrics

---

## API Endpoints Summary

| Dashboard | Endpoint | Method | Authentication | Authorization |
|-----------|----------|--------|----------------|---------------|
| Employee | `/q/api/v1/employees/me/dashboard` | GET | Required | Employee+ |
| Manager | `/q/api/v1/managers/me/dashboard` | GET | Required | TeamLead+ |
| HR | `/q/api/v1/hr/dashboard` | GET | Required | HR+ |

**Authorization Hierarchy:**
- Employee: All authenticated users
- TeamLead: TeamLead, HR, HRLead, Admin
- HR: HR, HRLead, Admin

---

## Logging

All dashboards use structured logging with source-generated LoggerMessage:

**Employee Dashboard:**
- Event 4044: Query starting
- Event 4045: Query succeeded
- Event 4046: Dashboard not found (warning)
- Event 4047: Query failed (error)

**Manager Dashboard:**
- Event 4048: Query starting
- Event 4049: Query succeeded
- Event 4050: Dashboard not found (warning)
- Event 4051: Query failed (error)

**HR Dashboard:**
- Event 4052: Query starting
- Event 4053: Query succeeded
- Event 4054: Dashboard not found (warning)
- Event 4055: Query failed (error)

---

## Future Enhancements

### Potential Features
1. **Filters and Drill-Down**
   - Date range selection
   - Organization filtering (HR Dashboard)
   - Status filtering
   - Search by employee/template name

2. **Visualizations**
   - Completion trend charts
   - Organization comparison graphs
   - Manager performance heatmaps
   - Time-to-completion histograms

3. **Alerts and Notifications**
   - Email alerts for overdue items
   - Dashboard notifications for urgent assignments
   - Weekly summary reports

4. **Export Capabilities**
   - CSV/Excel export for HR metrics
   - PDF reports generation
   - Scheduled report delivery

5. **Comparative Analytics**
   - Period-over-period comparisons
   - Benchmark against organizational targets
   - Manager performance rankings

6. **Real-Time Updates**
   - SignalR integration for live dashboard updates
   - Real-time completion notifications
   - Live activity feed

---

## Troubleshooting

### Common Issues

**Dashboard shows no data:**
- Verify user authentication and role claims
- Check that assignments exist in the system
- Verify EmployeeId/ManagerId mappings are correct

**Urgent assignments not appearing:**
- Check DueDate is set on assignments
- Verify current date/time on server
- Ensure assignments are not in Finalized state

**Manager dashboard shows no team members:**
- Verify ManagerId is set correctly on Employee records
- Check employee IsDeleted flag
- Verify manager has TeamLead role

**HR dashboard slow to load:**
- Consider implementing caching
- Check database query performance
- Monitor Marten query execution time

---

## Related Documentation

- [Event Sourcing Architecture](./EventSourcing.md)
- [CQRS Implementation](./CQRS.md)
- [Authorization Policies](./Authorization.md)
- [Questionnaire Workflow](./QuestionnaireWorkflow.md)
