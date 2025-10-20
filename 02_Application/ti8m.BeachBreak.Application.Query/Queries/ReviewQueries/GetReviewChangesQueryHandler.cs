using Marten;
using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Queries.ReviewQueries;

/// <summary>
/// Query handler to retrieve all review changes for a specific questionnaire assignment.
/// Queries the ReviewChangeLog projection to return all edits made by the manager during the review meeting.
/// Note: Employee names are resolved in the controller/presentation layer, not here.
/// </summary>
public class GetReviewChangesQueryHandler : IQueryHandler<GetReviewChangesQuery, List<ReviewChangeLogReadModel>>
{
    private readonly IQuerySession session;

    public GetReviewChangesQueryHandler(IQuerySession session)
    {
        this.session = session;
    }

    public async Task<List<ReviewChangeLogReadModel>> HandleAsync(GetReviewChangesQuery query, CancellationToken cancellationToken = default)
    {
        var changes = await session.Query<ReviewChangeLogReadModel>()
            .Where(c => c.AssignmentId == query.AssignmentId)
            .OrderBy(c => c.ChangedAt)
            .ToListAsync(cancellationToken);

        return changes.ToList();
    }
}
