using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

/// <summary>
/// Marten-based repository for querying FeedbackTemplateReadModel projections.
/// </summary>
internal class FeedbackTemplateRepository(IDocumentStore store) : IFeedbackTemplateRepository
{
    public async Task<FeedbackTemplateReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        var template = await session.Query<FeedbackTemplateReadModel>()
            .Where(x => x.Id == id && !x.IsDeleted)
            .SingleOrDefaultAsync(cancellationToken);

        if (template != null)
        {
            await EnrichWithEmployeeNameAsync(session, template, cancellationToken);
        }

        return template;
    }

    public async Task<IEnumerable<FeedbackTemplateReadModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        var templates = await session.Query<FeedbackTemplateReadModel>()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);

        await EnrichWithEmployeeNamesAsync(session, templates, cancellationToken);
        return templates;
    }

    /// <summary>
    /// Enriches a single template with employee name by looking up the creator in EmployeeReadModel.
    /// </summary>
    private async Task EnrichWithEmployeeNameAsync(IDocumentSession session, FeedbackTemplateReadModel template, CancellationToken cancellationToken)
    {
        var employee = await session.Query<EmployeeReadModel>()
            .Where(e => e.Id == template.CreatedByEmployeeId && !e.IsDeleted)
            .SingleOrDefaultAsync(cancellationToken);

        template.CreatedByEmployeeName = employee != null
            ? $"{employee.FirstName} {employee.LastName}".Trim()
            : string.Empty;
    }

    /// <summary>
    /// Enriches multiple templates with employee names using efficient batch lookup.
    /// </summary>
    private async Task EnrichWithEmployeeNamesAsync(IDocumentSession session, IEnumerable<FeedbackTemplateReadModel> templates, CancellationToken cancellationToken)
    {
        var employeeIds = templates.Select(t => t.CreatedByEmployeeId).Distinct().ToList();

        var employees = await session.Query<EmployeeReadModel>()
            .Where(e => employeeIds.Contains(e.Id) && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        var employeeDict = employees.ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());

        foreach (var template in templates)
        {
            template.CreatedByEmployeeName = employeeDict.TryGetValue(template.CreatedByEmployeeId, out var name)
                ? name
                : string.Empty;
        }
    }

    public async Task<IEnumerable<FeedbackTemplateReadModel>> GetBySourceTypeAsync(FeedbackSourceType sourceType, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        int sourceTypeInt = (int)sourceType;
        var templates = await session.Query<FeedbackTemplateReadModel>()
            .Where(x => !x.IsDeleted
                && x.AllowedSourceTypes.Contains(sourceTypeInt)
                && x.Status == Domain.QuestionnaireTemplateAggregate.TemplateStatus.Published)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);

        await EnrichWithEmployeeNamesAsync(session, templates, cancellationToken);
        return templates;
    }
}
