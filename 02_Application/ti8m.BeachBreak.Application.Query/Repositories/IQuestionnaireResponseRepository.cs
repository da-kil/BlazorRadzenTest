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

    /// <summary>
    /// Gets questionnaire responses for multiple assignment IDs.
    /// </summary>
    Task<List<QuestionnaireResponseReadModel>> GetByAssignmentIdsAsync(IEnumerable<Guid> assignmentIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all questionnaire responses ordered by last modified date descending.
    /// </summary>
    Task<List<QuestionnaireResponseReadModel>> GetAllResponsesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a questionnaire response by its ID.
    /// </summary>
    Task<QuestionnaireResponseReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
