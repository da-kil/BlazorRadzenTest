using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public static class QuestionnairePageConfigurationFactory
{
    public static async Task<QuestionnairePageConfiguration> CreateEmployeeConfigurationAsync(
        List<QuestionnaireAssignment> allAssignments,
        List<QuestionnaireAssignment> upcomingAssignments,  // newQuestionnaires (Assigned)
        List<QuestionnaireAssignment> currentAssignments,    // inProgressQuestionnaires (working/review)
        List<QuestionnaireAssignment> completedAssignments,  // completedQuestionnaires (Finalized)
        List<QuestionnaireAssignment> overdueAssignments,
        List<Category> categories,
        IUITranslationService translationService,
        Language currentLanguage = Language.English)
    {
        return new QuestionnairePageConfiguration
        {
            PageRoute = "/my-questionnaires",
            PageTitle = await translationService.GetTextAsync("pages.my-questionnaires", currentLanguage),
            PageDescription = await translationService.GetTextAsync("pages.my-questionnaires-description", currentLanguage),
            PageType = QuestionnairePageType.Employee,

            // Store pre-categorized lists (order matches what MyQuestionnaires.razor passes)
            CurrentAssignments = currentAssignments,
            UpcomingAssignments = upcomingAssignments,
            CompletedAssignments = completedAssignments,
            OverdueAssignments = overdueAssignments,

            Tabs = new List<QuestionnairePageTab>
            {
                new() { Id = "current", Title = await translationService.GetTextAsync("tabs.current", currentLanguage), Type = QuestionnaireTabType.Current },
                new() { Id = "completed", Title = await translationService.GetTextAsync("tabs.completed", currentLanguage), Type = QuestionnaireTabType.Completed },
                new() { Id = "overdue", Title = await translationService.GetTextAsync("tabs.overdue", currentLanguage), Type = QuestionnaireTabType.Overdue }
            },

            Filters = new List<QuestionnairePageFilter>
            {
                new()
                {
                    Id = "category",
                    Label = await translationService.GetTextAsync("filters.filter-by-category", currentLanguage),
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
                    Text = await translationService.GetTextAsync("buttons.refresh", currentLanguage),
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
                        Label = await translationService.GetTextAsync("status.current", currentLanguage),
                        Icon = "pending_actions",
                        IconClass = "text-info",
                        CssClass = "stats-current",
                        ValueCalculator = () => currentAssignments.Count
                    },
                    new()
                    {
                        Id = "completed",
                        Label = await translationService.GetTextAsync("status.completed", currentLanguage),
                        Icon = "task_alt",
                        IconClass = "text-success",
                        CssClass = "stats-completed",
                        ValueCalculator = () => completedAssignments.Count
                    },
                    new()
                    {
                        Id = "overdue",
                        Label = await translationService.GetTextAsync("status.overdue", currentLanguage),
                        Icon = "warning",
                        IconClass = "text-danger",
                        CssClass = "stats-overdue",
                        ValueCalculator = () => overdueAssignments.Count
                    }
                }
            }
        };
    }

    public static async Task<QuestionnairePageConfiguration> CreateManagerConfigurationAsync(
        List<QuestionnaireAssignment> allAssignments,
        List<EmployeeDto> teamMembers,
        List<Category> categories,
        IUITranslationService translationService,
        Language currentLanguage = Language.English)
    {
        return new QuestionnairePageConfiguration
        {
            PageRoute = "/team/questionnaires",
            PageTitle = await translationService.GetTextAsync("pages.team-questionnaires", currentLanguage),
            PageDescription = await translationService.GetTextAsync("pages.team-questionnaires-description", currentLanguage),
            PageType = QuestionnairePageType.Manager,

            Tabs = new List<QuestionnairePageTab>
            {
                new() { Id = "team", Title = await translationService.GetTextAsync("tabs.team-view", currentLanguage), Type = QuestionnaireTabType.TeamView },
                new() { Id = "questionnaires", Title = await translationService.GetTextAsync("tabs.questionnaire-view", currentLanguage), Type = QuestionnaireTabType.QuestionnaireView },
                new() { Id = "analytics", Title = await translationService.GetTextAsync("tabs.analytics", currentLanguage), Type = QuestionnaireTabType.Analytics }
            },

            Filters = new List<QuestionnairePageFilter>
            {
                new()
                {
                    Id = "search",
                    Label = await translationService.GetTextAsync("filters.search", currentLanguage),
                    Type = QuestionnaireFilterType.Search,
                    IsVisible = true
                },
                new()
                {
                    Id = "category",
                    Label = await translationService.GetTextAsync("filters.filter-by-category", currentLanguage),
                    Type = QuestionnaireFilterType.Category,
                    IsVisible = categories.Count > 0,
                    Options = categories.Select(c => currentLanguage == Language.German ? c.NameDe : c.NameEn).ToList(),
                    CategoryOptions = categories
                },
                new()
                {
                    Id = "status",
                    Label = await translationService.GetTextAsync("filters.filter-by-status", currentLanguage),
                    Type = QuestionnaireFilterType.Status,
                    IsVisible = true,
                    Options = new List<string>
                    {
                        await translationService.GetTextAsync("workflow-states.assigned", currentLanguage),
                        await translationService.GetTextAsync("workflow-states.in-progress", currentLanguage),
                        await translationService.GetTextAsync("workflow-states.submitted", currentLanguage),
                        await translationService.GetTextAsync("workflow-states.in-review", currentLanguage),
                        await translationService.GetTextAsync("workflow-states.finalized", currentLanguage),
                        await translationService.GetTextAsync("workflow-states.overdue", currentLanguage)
                    }
                },
                new()
                {
                    Id = "dateRange",
                    Label = await translationService.GetTextAsync("filters.filter-by-due-date", currentLanguage),
                    Type = QuestionnaireFilterType.DateRange,
                    IsVisible = true
                }
            },

            Actions = new List<QuestionnairePageAction>
            {
                new()
                {
                    Id = "refresh",
                    Text = await translationService.GetTextAsync("buttons.refresh", currentLanguage),
                    Icon = "refresh",
                    ButtonStyle = "ButtonStyle.Light"
                },
                new()
                {
                    Id = "export",
                    Text = await translationService.GetTextAsync("buttons.export-report", currentLanguage),
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
                        Label = await translationService.GetTextAsync("labels.team-members", currentLanguage),
                        Icon = "people",
                        IconClass = "text-primary",
                        CssClass = "stats-team-members",
                        ValueCalculator = () => teamMembers.Count
                    },
                    new()
                    {
                        Id = "total-assignments",
                        Label = await translationService.GetTextAsync("labels.total-assignments", currentLanguage),
                        Icon = "assignment",
                        IconClass = "text-secondary",
                        CssClass = "stats-total",
                        ValueCalculator = () => allAssignments.Count
                    },
                    new()
                    {
                        Id = "overdue",
                        Label = await translationService.GetTextAsync("status.overdue", currentLanguage),
                        Icon = "warning",
                        IconClass = "text-danger",
                        CssClass = "stats-overdue",
                        ValueCalculator = () => allAssignments.Count(a => a.DueDate.HasValue && a.DueDate.Value < DateTime.Now && a.WorkflowState != WorkflowState.Finalized)
                    },
                    new()
                    {
                        Id = "not-started",
                        Label = await translationService.GetTextAsync("status.not-started", currentLanguage),
                        Icon = "schedule",
                        IconClass = "text-warning",
                        CssClass = "stats-not-started",
                        ValueCalculator = () => allAssignments.Count(a => a.WorkflowState == WorkflowState.Assigned)
                    },
                    new()
                    {
                        Id = "in-progress",
                        Label = await translationService.GetTextAsync("status.in-progress", currentLanguage),
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
                            or WorkflowState.ReviewFinished)
                    },
                    new()
                    {
                        Id = "finalized",
                        Label = await translationService.GetTextAsync("status.finalized", currentLanguage),
                        Icon = "task_alt",
                        IconClass = "text-success",
                        CssClass = "stats-finalized",
                        ValueCalculator = () => allAssignments.Count(a => a.WorkflowState == WorkflowState.Finalized)
                    }
                }
            }
        };
    }

    public static async Task<QuestionnairePageConfiguration> CreateHRConfigurationAsync(
        List<QuestionnaireAssignment> allAssignments,
        List<EmployeeDto> allEmployees,
        List<Organization> allOrganizations,
        List<QuestionnaireTemplate> allTemplates,
        List<Category> categories,
        IUITranslationService translationService,
        Language currentLanguage = Language.English)
    {
        return new QuestionnairePageConfiguration
        {
            PageRoute = "/organization/questionnaires",
            PageTitle = await translationService.GetTextAsync("pages.organization-questionnaires", currentLanguage),
            PageDescription = await translationService.GetTextAsync("pages.organization-questionnaires-description", currentLanguage),
            PageType = QuestionnairePageType.HR,

            Tabs = new List<QuestionnairePageTab>
            {
                new() { Id = "departments", Title = await translationService.GetTextAsync("tabs.department-overview", currentLanguage), Type = QuestionnaireTabType.DepartmentOverview },
                new() { Id = "employees", Title = await translationService.GetTextAsync("tabs.employee-status", currentLanguage), Type = QuestionnaireTabType.EmployeeStatus },
                new() { Id = "questionnaires", Title = await translationService.GetTextAsync("tabs.questionnaire-performance", currentLanguage), Type = QuestionnaireTabType.QuestionnairePerformance },
                new() { Id = "analytics", Title = await translationService.GetTextAsync("tabs.analytics-insights", currentLanguage), Type = QuestionnaireTabType.Analytics }
            },

            Filters = new List<QuestionnairePageFilter>
            {
                new()
                {
                    Id = "search",
                    Label = await translationService.GetTextAsync("filters.search", currentLanguage),
                    Type = QuestionnaireFilterType.Search,
                    IsVisible = true
                },
                new()
                {
                    Id = "department",
                    Label = await translationService.GetTextAsync("filters.organization-filter", currentLanguage),
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
                    Label = await translationService.GetTextAsync("filters.filter-by-category", currentLanguage),
                    Type = QuestionnaireFilterType.Category,
                    IsVisible = categories.Count > 0,
                    Options = categories.Select(c => currentLanguage == Language.German ? c.NameDe : c.NameEn).ToList(),
                    CategoryOptions = categories
                },
                new()
                {
                    Id = "template",
                    Label = await translationService.GetTextAsync("filters.filter-by-questionnaire", currentLanguage),
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
                    Label = await translationService.GetTextAsync("filters.filter-by-status", currentLanguage),
                    Type = QuestionnaireFilterType.Status,
                    IsVisible = true,
                    Options = new List<string>
                    {
                        await translationService.GetTextAsync("workflow-states.assigned", currentLanguage),
                        await translationService.GetTextAsync("workflow-states.in-progress", currentLanguage),
                        await translationService.GetTextAsync("workflow-states.submitted", currentLanguage),
                        await translationService.GetTextAsync("workflow-states.in-review", currentLanguage),
                        await translationService.GetTextAsync("workflow-states.finalized", currentLanguage),
                        await translationService.GetTextAsync("workflow-states.overdue", currentLanguage)
                    }
                },
                new()
                {
                    Id = "dateRange",
                    Label = await translationService.GetTextAsync("filters.filter-by-due-date", currentLanguage),
                    Type = QuestionnaireFilterType.DateRange,
                    IsVisible = true
                }
            },

            Actions = new List<QuestionnairePageAction>
            {
                new()
                {
                    Id = "refresh",
                    Text = await translationService.GetTextAsync("buttons.refresh", currentLanguage),
                    Icon = "refresh",
                    ButtonStyle = "ButtonStyle.Light"
                },
                new()
                {
                    Id = "export",
                    Text = await translationService.GetTextAsync("buttons.export-report", currentLanguage),
                    Icon = "download",
                    ButtonStyle = "ButtonStyle.Info"
                },
                new()
                {
                    Id = "analytics",
                    Text = await translationService.GetTextAsync("buttons.analytics-dashboard", currentLanguage),
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
                        Label = await translationService.GetTextAsync("labels.employees", currentLanguage),
                        Icon = "people",
                        IconClass = "text-primary",
                        CssClass = "stats-employees",
                        ValueCalculator = () => allEmployees.Count
                    },
                    new()
                    {
                        Id = "questionnaires",
                        Label = await translationService.GetTextAsync("labels.questionnaires", currentLanguage),
                        Icon = "quiz",
                        IconClass = "text-info",
                        CssClass = "stats-questionnaires",
                        ValueCalculator = () => allTemplates.Count
                    },
                    new()
                    {
                        Id = "assignments",
                        Label = await translationService.GetTextAsync("labels.total-assignments", currentLanguage),
                        Icon = "assignment",
                        IconClass = "text-secondary",
                        CssClass = "stats-assignments",
                        ValueCalculator = () => allAssignments.Count
                    },
                    new()
                    {
                        Id = "pending",
                        Label = await translationService.GetTextAsync("status.pending", currentLanguage),
                        Icon = "pending_actions",
                        IconClass = "text-warning",
                        CssClass = "stats-pending",
                        ValueCalculator = () => allAssignments.Count(a => a.WorkflowState != WorkflowState.Finalized)
                    },
                    new()
                    {
                        Id = "completed",
                        Label = await translationService.GetTextAsync("status.completed", currentLanguage),
                        Icon = "task_alt",
                        IconClass = "text-success",
                        CssClass = "stats-completed",
                        ValueCalculator = () => allAssignments.Count(a => a.WorkflowState == WorkflowState.Finalized)
                    },
                    new()
                    {
                        Id = "overdue",
                        Label = await translationService.GetTextAsync("status.overdue", currentLanguage),
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