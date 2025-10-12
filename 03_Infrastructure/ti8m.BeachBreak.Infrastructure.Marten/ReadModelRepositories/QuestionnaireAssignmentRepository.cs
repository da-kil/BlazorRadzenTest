using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

internal class QuestionnaireAssignmentRepository(IDocumentStore store) : IQuestionnaireAssignmentRepository
{
    public async Task<IEnumerable<QuestionnaireAssignmentReadModel>> GetAllAssignmentsAsync(CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<QuestionnaireAssignmentReadModel>()
            .Where(a => !a.IsWithdrawn)
            .OrderBy(a => a.AssignedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<QuestionnaireAssignmentReadModel?> GetAssignmentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<QuestionnaireAssignmentReadModel>()
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<QuestionnaireAssignmentReadModel>> GetAssignmentsByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<QuestionnaireAssignmentReadModel>()
            .Where(a => a.EmployeeId == employeeId && !a.IsWithdrawn)
            .OrderBy(a => a.AssignedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<QuestionnaireAssignmentReadModel>> GetAssignmentsByTemplateIdAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<QuestionnaireAssignmentReadModel>()
            .Where(a => a.TemplateId == templateId && !a.IsWithdrawn)
            .OrderBy(a => a.AssignedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<QuestionnaireAssignmentReadModel>> GetAssignmentsByWorkflowStateAsync(WorkflowState workflowState, CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        return await session.Query<QuestionnaireAssignmentReadModel>()
            .Where(a => !a.IsWithdrawn && a.WorkflowState == workflowState)
            .OrderBy(a => a.AssignedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<QuestionnaireAssignmentReadModel>> GetOverdueAssignmentsAsync(CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();
        var now = DateTime.UtcNow;
        return await session.Query<QuestionnaireAssignmentReadModel>()
            .Where(a => !a.IsWithdrawn && a.DueDate.HasValue && a.DueDate.Value < now && !a.CompletedDate.HasValue)
            .OrderBy(a => a.DueDate)
            .ToListAsync(cancellationToken);
    }
}