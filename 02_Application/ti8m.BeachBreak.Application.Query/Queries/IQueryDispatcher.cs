namespace ti8m.BeachBreak.Application.Query.Queries;

public interface IQueryDispatcher
{
    Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
