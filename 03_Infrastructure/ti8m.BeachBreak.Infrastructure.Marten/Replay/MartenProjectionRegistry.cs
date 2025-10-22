using Marten;
using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Services;

namespace ti8m.BeachBreak.Infrastructure.Marten.Replay;

/// <summary>
/// Registry of all Marten snapshot projections that can be rebuilt
/// </summary>
public class MartenProjectionRegistry : IProjectionRegistry
{
    private readonly IDocumentStore _store;
    private readonly Dictionary<string, ProjectionInfo> _projections;

    public MartenProjectionRegistry(IDocumentStore store)
    {
        _store = store;

        // Register all snapshot projections
        _projections = new Dictionary<string, ProjectionInfo>
        {
            ["CategoryReadModel"] = new ProjectionInfo
            {
                Name = "CategoryReadModel",
                Description = "Category snapshots with translations",
                DocumentType = typeof(CategoryReadModel),
                TableName = "mt_doc_categoryreadmodel",
                IsRebuildable = true
            },
            ["QuestionnaireTemplateReadModel"] = new ProjectionInfo
            {
                Name = "QuestionnaireTemplateReadModel",
                Description = "Questionnaire template snapshots",
                DocumentType = typeof(QuestionnaireTemplateReadModel),
                TableName = "mt_doc_questionnairetemplate readmodel",
                IsRebuildable = true
            },
            ["EmployeeReadModel"] = new ProjectionInfo
            {
                Name = "EmployeeReadModel",
                Description = "Employee snapshots",
                DocumentType = typeof(EmployeeReadModel),
                TableName = "mt_doc_employeereadmodel",
                IsRebuildable = true
            },
            ["OrganizationReadModel"] = new ProjectionInfo
            {
                Name = "OrganizationReadModel",
                Description = "Organization snapshots",
                DocumentType = typeof(OrganizationReadModel),
                TableName = "mt_doc_organizationreadmodel",
                IsRebuildable = true
            },
            ["QuestionnaireAssignmentReadModel"] = new ProjectionInfo
            {
                Name = "QuestionnaireAssignmentReadModel",
                Description = "Questionnaire assignment snapshots",
                DocumentType = typeof(QuestionnaireAssignmentReadModel),
                TableName = "mt_doc_questionnaireassignmentreadmodel",
                IsRebuildable = true
            },
            ["QuestionnaireResponseReadModel"] = new ProjectionInfo
            {
                Name = "QuestionnaireResponseReadModel",
                Description = "Questionnaire response snapshots",
                DocumentType = typeof(QuestionnaireResponseReadModel),
                TableName = "mt_doc_questionnaireresponsereadmodel",
                IsRebuildable = true
            },
            ["ProjectionReplayReadModel"] = new ProjectionInfo
            {
                Name = "ProjectionReplayReadModel",
                Description = "Projection replay history snapshots",
                DocumentType = typeof(ProjectionReplayReadModel),
                TableName = "mt_doc_projectionreplayreadmodel",
                IsRebuildable = false // Don't allow rebuilding replay history (would lose history)
            }
        };
    }

    public IReadOnlyList<ProjectionInfo> GetAllProjections()
    {
        return _projections.Values
            .Select(p => p with { CurrentSnapshotCount = GetSnapshotCount(p.DocumentType) })
            .ToList();
    }

    public ProjectionInfo? GetProjection(string name)
    {
        if (!_projections.TryGetValue(name, out var projection))
        {
            return null;
        }

        return projection with { CurrentSnapshotCount = GetSnapshotCount(projection.DocumentType) };
    }

    public bool CanRebuild(string name)
    {
        return _projections.TryGetValue(name, out var projection) && projection.IsRebuildable;
    }

    public Type? GetDocumentType(string projectionName)
    {
        return _projections.TryGetValue(projectionName, out var projection)
            ? projection.DocumentType
            : null;
    }

    public string? GetTableName(string projectionName)
    {
        return _projections.TryGetValue(projectionName, out var projection)
            ? projection.TableName
            : null;
    }

    private long GetSnapshotCount(Type documentType)
    {
        try
        {
            using var session = _store.QuerySession();

            // Use reflection to call session.Query<T>().Count()
            var queryMethod = typeof(IQuerySession)
                .GetMethods()
                .First(m => m.Name == "Query" && m.IsGenericMethod && m.GetParameters().Length == 0);

            var genericMethod = queryMethod.MakeGenericMethod(documentType);
            var queryable = genericMethod.Invoke(session, null);

            if (queryable is IQueryable<object> query)
            {
                return query.LongCount();
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }
}
