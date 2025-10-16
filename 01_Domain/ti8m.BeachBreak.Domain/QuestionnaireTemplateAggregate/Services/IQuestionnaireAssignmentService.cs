namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Services;

public interface IQuestionnaireAssignmentService
{
    Task<bool> HasActiveAssignmentsAsync(Guid templateId, CancellationToken cancellationToken = default);
    Task<int> GetActiveAssignmentCountAsync(Guid templateId, CancellationToken cancellationToken = default);
}