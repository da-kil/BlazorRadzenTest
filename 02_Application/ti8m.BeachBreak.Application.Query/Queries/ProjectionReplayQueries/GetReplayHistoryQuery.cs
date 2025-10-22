using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Queries.ProjectionReplayQueries;

public class GetReplayHistoryQuery : IQuery<Result<IEnumerable<ProjectionReplayReadModel>>>
{
    public int Limit { get; init; } = 50;

    public GetReplayHistoryQuery(int limit = 50)
    {
        Limit = limit;
    }
}
