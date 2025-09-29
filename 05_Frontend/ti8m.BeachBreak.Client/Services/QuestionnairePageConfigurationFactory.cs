using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public static class QuestionnairePageConfigurationFactory
{
    public static QuestionnairePageConfiguration CreateEmployeeConfiguration(
        List<QuestionnaireAssignment> allAssignments,
        List<QuestionnaireAssignment> currentAssignments,
        List<QuestionnaireAssignment> upcomingAssignments,
        List<QuestionnaireAssignment> completedAssignments,
        List<QuestionnaireAssignment> overdueAssignments,
        List<Category> categories)
    {
        return new QuestionnairePageConfiguration
        {
            PageRoute = "/my-questionnaires",
            PageTitle = "My Questionnaires",
            PageDescription = "View and complete your assigned questionnaires",
            PageIcon = "assignment",
            PageType = QuestionnairePageType.Employee,

            Tabs = new List<QuestionnairePageTab>
            {
                new() { Id = "current", Title = "Current", Type = QuestionnaireTabType.Current },
                new() { Id = "upcoming", Title = "Upcoming", Type = QuestionnaireTabType.Upcoming },
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
                    Options = categories.Select(c => c.NameEn).ToList(),
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
                Columns = 4,
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
                        Id = "upcoming",
                        Label = "Upcoming",
                        Icon = "schedule",
                        IconClass = "text-primary",
                        CssClass = "stats-upcoming",
                        ValueCalculator = () => upcomingAssignments.Count
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
        List<Category> categories)
    {
        return new QuestionnairePageConfiguration
        {
            PageRoute = "/team/questionnaires",
            PageTitle = "Team Questionnaires",
            PageDescription = "Monitor and track your team's questionnaire progress",
            PageIcon = "groups",
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
                    Id = "status",
                    Label = "Status Filter",
                    Type = QuestionnaireFilterType.Status,
                    IsVisible = true,
                    Options = new List<string> { "All", "Pending", "In Progress", "Completed", "Overdue" }
                },
                new()
                {
                    Id = "category",
                    Label = "Category Filter",
                    Type = QuestionnaireFilterType.Category,
                    IsVisible = categories.Count > 0,
                    Options = categories.Select(c => c.NameEn).ToList(),
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
                Columns = 4,
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
                        Id = "pending",
                        Label = "Pending",
                        Icon = "pending_actions",
                        IconClass = "text-warning",
                        CssClass = "stats-pending",
                        ValueCalculator = () => allAssignments.Count(a => a.Status == AssignmentStatus.Assigned || a.Status == AssignmentStatus.InProgress)
                    },
                    new()
                    {
                        Id = "completed",
                        Label = "Completed",
                        Icon = "task_alt",
                        IconClass = "text-success",
                        CssClass = "stats-completed",
                        ValueCalculator = () => allAssignments.Count(a => a.Status == AssignmentStatus.Completed)
                    },
                    new()
                    {
                        Id = "overdue",
                        Label = "Overdue",
                        Icon = "warning",
                        IconClass = "text-danger",
                        CssClass = "stats-overdue",
                        ValueCalculator = () => allAssignments.Count(a => a.DueDate.HasValue && a.DueDate.Value < DateTime.Now && a.Status != AssignmentStatus.Completed)
                    }
                }
            }
        };
    }

    public static QuestionnairePageConfiguration CreateHRConfiguration(
        List<QuestionnaireAssignment> allAssignments,
        List<EmployeeDto> allEmployees,
        List<Organization> allOrganizations,
        List<QuestionnaireTemplate> allTemplates)
    {
        return new QuestionnairePageConfiguration
        {
            PageRoute = "/organization/questionnaires",
            PageTitle = "Organization Questionnaires",
            PageDescription = "Comprehensive overview of all questionnaire activities across the organization",
            PageIcon = "corporate_fare",
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
                    Id = "status",
                    Label = "Status Filter",
                    Type = QuestionnaireFilterType.Status,
                    IsVisible = true,
                    Options = new List<string> { "All", "Assigned", "In Progress", "Completed", "Overdue" }
                },
                new()
                {
                    Id = "daterange",
                    Label = "Date Range",
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
                        ValueCalculator = () => allAssignments.Count(a => a.Status == AssignmentStatus.Assigned || a.Status == AssignmentStatus.InProgress)
                    },
                    new()
                    {
                        Id = "completed",
                        Label = "Completed",
                        Icon = "task_alt",
                        IconClass = "text-success",
                        CssClass = "stats-completed",
                        ValueCalculator = () => allAssignments.Count(a => a.Status == AssignmentStatus.Completed)
                    },
                    new()
                    {
                        Id = "overdue",
                        Label = "Overdue",
                        Icon = "warning",
                        IconClass = "text-danger",
                        CssClass = "stats-overdue",
                        ValueCalculator = () => allAssignments.Count(a => a.DueDate.HasValue && a.DueDate.Value < DateTime.Now && a.Status != AssignmentStatus.Completed)
                    }
                }
            }
        };
    }
}