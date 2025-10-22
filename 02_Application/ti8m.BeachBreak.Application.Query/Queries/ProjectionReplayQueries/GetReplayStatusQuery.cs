namespace ti8m.BeachBreak.Application.Query.Queries.ProjectionReplayQueries;

public class GetReplayStatusQuery : IQuery<Result<Projections.ProjectionReplayReadModel>>
{
    public Guid ReplayId { get; init; }

    public GetReplayStatusQuery(Guid replayId)
    {
        ReplayId = replayId;
    }
}
