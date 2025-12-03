using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public static class QuestionnairePageConfigurationFactory
{
    public static QuestionnairePageConfiguration CreateEmployeeConfiguration(
        List<QuestionnaireAssignment> allAssignments,
        List<QuestionnaireAssignment> upcomingAssignments,  // newQuestionnaires (Assigned)
        List<QuestionnaireAssignment> currentAssignments,    // inProgressQuestionnaires (working/review)
        List<QuestionnaireAssignment> completedAssignments,  // completedQuestionnaires (Finalized)
        List<QuestionnaireAssignment> overdueAssignments,
        List<Category> categories,
        Language currentLanguage = Language.English)
    {
        return new QuestionnairePageConfiguration
        {
            PageRoute = "/my-questionnaires",
            PageTitle = "My Questionnaires",
            PageDescription = "View and complete your assigned questionnaires",
            PageType = QuestionnairePageType.Employee,

            // Store pre-categorized lists (order matches what MyQuestionnaires.razor passes)
            CurrentAssignments = currentAssignments,
            UpcomingAssignments = upcomingAssignments,
            CompletedAssignments = completedAssignments,
            OverdueAssignments = overdueAssignments,

            Tabs = new List<QuestionnairePageTab>
            {
                new() { Id = "current", Title = "Current", Type = QuestionnaireTabType.Current },
                new() { Id = "completed", Title = "Completed", Type = QuestionnaireTabType.Completed },
                new() { Id = "overdue", Title = "Overdue", Type = QuestionnaireTabType.Overdue }
            },

            Filters = new List<QuestionnairePageFilter>
            {
                new()
                {
                    Id = "category",
                    Label = "Filter by Category",
                    Type = QuestionnaireFilterType.Category,
                    IsVisible = categories.Count > 0,
                    Options = categories.Select(c => currentLanguage == Language.German ? c.NameDe : c.NameEn).ToList(),
                    CategoryOptions = categories
                }
            },

            Actions = new List<QuestionnairePageAction>
            {
                new()
                {
                    Id = "refresh",
                    Text = "Refresh",
                    Icon = "refresh",
                    ButtonStyle = "ButtonStyle.Light"
                }
            },

            StatsConfig = new QuestionnaireStatsConfig
            {
                Columns = 3,
                StatCards = new List<StatCardConfiguration>
                {
                    new()
                    {
                        Id = "current",
                        Label = "Current",
                        Icon = "pending_actions",
                        IconClass = "text-info",
                        CssClass = "stats-current",
                        ValueCalculator = () => currentAssignments.Count
                    },
                    new()
                    {
                        Id = "completed",
                        Label = "Completed",
                        Icon = "task_alt",
                        IconClass = "text-success",
                        CssClass = "stats-completed",
                        ValueCalculator = () => completedAssignments.Count
                    },
                    new()
                    {
                        Id = "overdue",
                        Label = "Overdue",
                        Icon = "warning",
                        IconClass = "text-danger",
                        CssClass = "stats-overdue",
                        ValueCalculator = () => overdueAssignments.Count
                    }
                }
            }
        };
    }

    public static QuestionnairePageConfiguration CreateManagerConfiguration(
        List<QuestionnaireAssignment> allAssignments,
        List<EmployeeDto> teamMembers,
        List<Category> categories,
        Language currentLanguage = Language.English)
    {
        return new QuestionnairePageConfiguration
        {
            PageRoute = "/team/questionnaires",
            PageTitle = "Team Questionnaires",
            PageDescription = "Monitor and track your team's questionnaire progress",
            PageType = QuestionnairePageType.Manager,

            Tabs = new List<QuestionnairePageTab>
            {
                new() { Id = "team", Title = "Team View", Type = QuestionnaireTabType.TeamView },
                new() { Id = "questionnaires", Title = "Questionnaire View", Type = QuestionnaireTabType.QuestionnaireView },
                new() { Id = "analytics", Title = "Analytics", Type = QuestionnaireTabType.Analytics }
            },

            Filters = new List<QuestionnairePageFilter>
            {
                new()
                {
                    Id = "search",
                    Label = "Search",
                    Type = QuestionnaireFilterType.Search,
                    IsVisible = true
                },
                new()
                {
                    Id = "category",
                    Label = "Filter by Category",
                    Type = QuestionnaireFilterType.Category,
                    IsVisible = categories.Count > 0,
                    Options = categories.Select(c => currentLanguage == Language.German ? c.NameDe : c.NameEn).ToList(),
                    CategoryOptions = categories
                },
                new()
                {
                    Id = "status",
                    Label = "Filter by Status",
                    Type = QuestionnaireFilterType.Status,
                    IsVisible = true,
                    Options = new List<string>
                    {
                        "Assigned",
                        "In Progress",
                        "Submitted",
                        "In Review",
                        "Finalized",
                        "Overdue"
                    }
                },
                new()
                {
                    Id = "dateRange",
                    Label = "Filter by Due Date",
                    Type = QuestionnaireFilterType.DateRange,
                    IsVisible = true
                }
            },

            Actions = new List<QuestionnairePageAction>
            {
                new()
                {
                    Id = "refresh",
                    Text = "Refresh",
                    Icon = "refresh",
                    ButtonStyle = "ButtonStyle.Light"
                },
                new()
                {
                    Id = "export",
                    Text = "Export Report",
                    Icon = "download",
                    ButtonStyle = "ButtonStyle.Info"
                }
            },

            StatsConfig = new QuestionnaireStatsConfig
            {
                Columns = 6,
                StatCards = new List<StatCardConfiguration>
                {
                    new()
                    {
                        Id = "team-members",
                        Label = "Team Members",
                        Icon = "people",
                        IconClass = "text-primary",
                        CssClass = "stats-team-members",
                        ValueCalculator = () => teamMembers.Count
                    },
                    new()
                    {
                        Id = "total-assignments",
                        Label = "Total Assignments",
                        Icon = "assignment",
                        IconClass = "text-secondary",
                        CssClass = "stats-total",
                        ValueCalculator = () => allAssignments.Count
                    },
                    new()
                    {
                        Id = "overdue",
                        Label = "⚠️ Overdue",
                        Icon = "warning",
                        IconClass = "text-danger",
                        CssClass = "stats-overdue",
                        ValueCalculator = () => allAssignments.Count(a => a.DueDate.HasValue && a.DueDate.Value < DateTime.Now && a.WorkflowState != WorkflowState.Finalized)
                    },
                    new()
                    {
                        Id = "not-started",
                        Label = "Not Started",
                        Icon = "schedule",
                        IconClass = "text-warning",
                        CssClass = "stats-not-started",
                        ValueCalculator = () => allAssignments.Count(a => a.WorkflowState == WorkflowState.Assigned)
                    },
                    new()
                    {
                        Id = "in-progress",
                        Label = "In Progress",
                        Icon = "pending_actions",
                        IconClass = "text-info",
                        CssClass = "stats-in-progress",
                        ValueCalculator = () => allAssignments.Count(a =>
                            a.WorkflowState is WorkflowState.EmployeeInProgress
                            or WorkflowState.ManagerInProgress
                            or WorkflowState.BothInProgress
                            or WorkflowState.EmployeeSubmitted
                            or WorkflowState.ManagerSubmitted
                            or WorkflowState.BothSubmitted
                            or WorkflowState.InReview
                            or WorkflowState.EmployeeReviewConfirmed
                            or WorkflowState.ManagerReviewConfirmed)
                    },
                    new()
                    {
                        Id = "finalized",
                        Label = "✓ Finalized",
                        Icon = "task_alt",
                        IconClass = "text-success",
                        CssClass = "stats-finalized",
                        ValueCalculator = () => allAssignments.Count(a => a.WorkflowState == WorkflowState.Finalized)
                    }
                }
            }
        };
    }

    public static QuestionnairePageConfiguration CreateHRConfiguration(
        List<QuestionnaireAssignment> allAssignments,
        List<EmployeeDto> allEmployees,
        List<Organization> allOrganizations,
        List<QuestionnaireTemplate> allTemplates,
        List<Category> categories,
        Language currentLanguage = Language.English)
    {
        return new QuestionnairePageConfiguration
        {
            PageRoute = "/organization/questionnaires",
            PageTitle = "Organization Questionnaires",
            PageDescription = "Comprehensive overview of all questionnaire activities across the organization",
            PageType = QuestionnairePageType.HR,

            Tabs = new List<QuestionnairePageTab>
            {
                new() { Id = "departments", Title = "Department Overview", Type = QuestionnaireTabType.DepartmentOverview },
                new() { Id = "employees", Title = "Employee Status", Type = QuestionnaireTabType.EmployeeStatus },
                new() { Id = "questionnaires", Title = "Questionnaire Performance", Type = QuestionnaireTabType.QuestionnairePerformance },
                new() { Id = "analytics", Title = "Analytics & Insights", Type = QuestionnaireTabType.Analytics }
            },

            Filters = new List<QuestionnairePageFilter>
            {
                new()
                {
                    Id = "search",
                    Label = "Search",
                    Type = QuestionnaireFilterType.Search,
                    IsVisible = true
                },
                new()
                {
                    Id = "department",
                    Label = "Organization Filter",
                    Type = QuestionnaireFilterType.Department,
                    IsVisible = true,
                    Options = allOrganizations
                        .Where(org => !org.IsDeleted && !org.IsIgnored)
                        .OrderBy(org => org.Number)
                        .Select(org => $"{org.Number} - {org.Name}")
                        .ToList()
                },
                new()
                {
                    Id = "category",
                    Label = "Filter by Category",
                    Type = QuestionnaireFilterType.Category,
                    IsVisible = categories.Count > 0,
                    Options = categories.Select(c => currentLanguage == Language.German ? c.NameDe : c.NameEn).ToList(),
                    CategoryOptions = categories
                },
                new()
                {
                    Id = "template",
                    Label = "Filter by Questionnaire",
                    Type = QuestionnaireFilterType.Template,
                    IsVisible = allTemplates.Count > 0,
                    TemplateOptions = allTemplates.Select(t => new QuestionnaireTemplateOption
                    {
                        Id = t.Id,
                        Name = t.GetLocalizedNameWithFallback(currentLanguage)
                    }).ToList()
                },
                new()
                {
                    Id = "status",
                    Label = "Filter by Status",
                    Type = QuestionnaireFilterType.Status,
                    IsVisible = true,
                    Options = new List<string>
                    {
                        "Assigned",
                        "In Progress",
                        "Submitted",
                        "In Review",
                        "Finalized",
                        "Overdue"
                    }
                },
                new()
                {
                    Id = "dateRange",
                    Label = "Filter by Due Date",
                    Type = QuestionnaireFilterType.DateRange,
                    IsVisible = true
                }
            },

            Actions = new List<QuestionnairePageAction>
            {
                new()
                {
                    Id = "refresh",
                    Text = "Refresh",
                    Icon = "refresh",
                    ButtonStyle = "ButtonStyle.Light"
                },
                new()
                {
                    Id = "export",
                    Text = "Export Report",
                    Icon = "download",
                    ButtonStyle = "ButtonStyle.Info"
                },
                new()
                {
                    Id = "analytics",
                    Text = "Analytics Dashboard",
                    Icon = "analytics",
                    ButtonStyle = "ButtonStyle.Primary"
                }
            },

            StatsConfig = new QuestionnaireStatsConfig
            {
                Columns = 6,
                StatCards = new List<StatCardConfiguration>
                {
                    new()
                    {
                        Id = "employees",
                        Label = "Employees",
                        Icon = "people",
                        IconClass = "text-primary",
                        CssClass = "stats-employees",
                        ValueCalculator = () => allEmployees.Count
                    },
                    new()
                    {
                        Id = "questionnaires",
                        Label = "Questionnaires",
                        Icon = "quiz",
                        IconClass = "text-info",
                        CssClass = "stats-questionnaires",
                        ValueCalculator = () => allTemplates.Count
                    },
                    new()
                    {
                        Id = "assignments",
                        Label = "Total Assignments",
                        Icon = "assignment",
                        IconClass = "text-secondary",
                        CssClass = "stats-assignments",
                        ValueCalculator = () => allAssignments.Count
                    },
                    new()
                    {
                        Id = "pending",
                        Label = "Pending",
                        Icon = "pending_actions",
                        IconClass = "text-warning",
                        CssClass = "stats-pending",
                        ValueCalculator = () => allAssignments.Count(a => a.WorkflowState != WorkflowState.Finalized)
                    },
                    new()
                    {
                        Id = "completed",
                        Label = "Completed",
                        Icon = "task_alt",
                        IconClass = "text-success",
                        CssClass = "stats-completed",
                        ValueCalculator = () => allAssignments.Count(a => a.WorkflowState == WorkflowState.Finalized)
                    },
                    new()
                    {
                        Id = "overdue",
                        Label = "Overdue",
                        Icon = "warning",
                        IconClass = "text-danger",
                        CssClass = "stats-overdue",
                        ValueCalculator = () => allAssignments.Count(a => a.DueDate.HasValue && a.DueDate.Value < DateTime.Now && a.WorkflowState != WorkflowState.Finalized)
                    }
                }
            }
        };
    }
}