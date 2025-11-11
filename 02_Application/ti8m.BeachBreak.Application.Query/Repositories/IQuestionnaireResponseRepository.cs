using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Repositories;

/// <summary>
/// Repository for querying QuestionnaireResponse read models.
/// Provides access to response data including goal responses stored in SectionResponses.
/// </summary>
public interface IQuestionnaireResponseRepository : IRepository
{
    /// <summary>
    /// Gets a questionnaire response by assignment ID.
    /// </summary>
    Task<QuestionnaireResponseReadModel?> GetByAssignmentIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);
}
