using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate;

namespace ti8m.BeachBreak.Application.Command.Repositories;

public interface IQuestionnaireResponseAggregateRepository : IAggregateRepository
{
    /// <summary>
    /// Finds a questionnaire response by assignment ID.
    /// Uses the read model to lookup the aggregate ID, then loads from event stream.
    /// </summary>
    Task<QuestionnaireResponse?> FindByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);
}
