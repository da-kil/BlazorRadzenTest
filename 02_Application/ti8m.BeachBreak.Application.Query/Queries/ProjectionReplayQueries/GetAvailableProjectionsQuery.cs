using ti8m.BeachBreak.Application.Query.Models;

namespace ti8m.BeachBreak.Application.Query.Queries.ProjectionReplayQueries;

public class GetAvailableProjectionsQuery : IQuery<Result<IEnumerable<ProjectionInfo>>>
{
    public bool RebuildableOnly { get; init; } = true;

    public GetAvailableProjectionsQuery(bool rebuildableOnly = true)
    {
        RebuildableOnly = rebuildableOnly;
    }
}
