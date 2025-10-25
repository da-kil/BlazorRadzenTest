using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Application.Query.Repositories;

public interface IQuestionnaireAssignmentRepository : IRepository
{
    Task<IEnumerable<QuestionnaireAssignmentReadModel>> GetAllAssignmentsAsync(CancellationToken cancellationToken = default);
    Task<QuestionnaireAssignmentReadModel?> GetAssignmentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionnaireAssignmentReadModel>> GetAssignmentsByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionnaireAssignmentReadModel>> GetAssignmentsByTemplateIdAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionnaireAssignmentReadModel>> GetAssignmentsByWorkflowStateAsync(WorkflowState workflowState, CancellationToken cancellationToken = default);
    Task<IEnumerable<QuestionnaireAssignmentReadModel>> GetOverdueAssignmentsAsync(CancellationToken cancellationToken = default);

    // Goal-specific queries (loads from aggregate - can be optimized with projections later)
    Task<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment?> LoadAggregateAsync(Guid id, CancellationToken cancellationToken = default);
}