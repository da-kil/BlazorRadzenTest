using Marten;
using ti8m.BeachBreak.Core.Infrastructure.Services;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Projections.Models;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Application.Query.Mappers;
using ti8m.BeachBreak.Domain;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

internal class EmployeeDashboardRepository(IDocumentStore store, ILanguageContext languageContext) : IEmployeeDashboardRepository
{
    public async Task<EmployeeDashboardReadModel?> GetDashboardByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        await using var session = await store.LightweightSerializableSessionAsync();

        // Query all assignments for this employee
        var assignments = await session.Query<QuestionnaireAssignmentReadModel>()
            .Where(a => a.EmployeeId == employeeId)
            .ToListAsync(cancellationToken);

        if (assignments.Count == 0)
        {
            return null; // No assignments yet, return null
        }

        // Get employee info from first assignment
        var firstAssignment = assignments.First();

        // Get unique template IDs for name lookup
        var templateIds = assignments.Select(a => a.TemplateId).Distinct().ToList();
        var templates = await session.Query<QuestionnaireTemplateReadModel>()
            .Where(t => templateIds.Contains(t.Id))
            .ToListAsync(cancellationToken);
        // Get user's preferred language
        var currentLanguageCode = await languageContext.GetCurrentLanguageCodeAsync();
        var currentLanguage = LanguageMapper.FromLanguageCode(currentLanguageCode);
        var domainLanguage = LanguageMapper.MapToDomain(currentLanguage);
        var templateDict = templates.ToDictionary(t => t.Id, t => GetLocalizedTemplateName(t, domainLanguage));

        // Aggregate metrics
        var pendingCount = assignments.Count(a => a.WorkflowState == WorkflowState.Assigned);
        var inProgressCount = assignments.Count(a =>
            a.WorkflowState == WorkflowState.EmployeeInProgress ||
            a.WorkflowState == WorkflowState.ManagerInProgress ||
            a.WorkflowState == WorkflowState.BothInProgress);
        var completedCount = assignments.Count(a => a.WorkflowState == WorkflowState.Finalized);

        // Find urgent assignments (due within 3 days or overdue)
        var now = DateTime.UtcNow;
        var urgentAssignments = assignments
            .Where(a => a.DueDate.HasValue &&
                        (a.DueDate.Value - now).TotalDays <= 3 &&
                        a.WorkflowState != WorkflowState.Finalized)
            .Select(a => new UrgentAssignmentItem
            {
                AssignmentId = a.Id,
                QuestionnaireTemplateName = templateDict.GetValueOrDefault(a.TemplateId, "Unknown"),
                DueDate = a.DueDate!.Value,
                WorkflowState = a.WorkflowState,
                IsOverdue = a.DueDate!.Value < now
            })
            .OrderBy(a => a.DueDate)
            .ToList();

        return new EmployeeDashboardReadModel
        {
            Id = employeeId,
            EmployeeFullName = firstAssignment.EmployeeName,
            EmployeeEmail = firstAssignment.EmployeeEmail,
            PendingCount = pendingCount,
            InProgressCount = inProgressCount,
            CompletedCount = completedCount,
            UrgentAssignments = urgentAssignments,
            LastUpdated = DateTime.UtcNow
        };
    }

    private static string GetLocalizedTemplateName(QuestionnaireTemplateReadModel template, Language language)
    {
        return language == Language.German ? template.NameGerman : template.NameEnglish;
    }
}
