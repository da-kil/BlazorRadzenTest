using Microsoft.Extensions.DependencyInjection;

namespace ti8m.BeachBreak.Application.Query.Queries;

public sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var queryHandlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType().UnderlyingSystemType, typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(queryHandlerType);

        return await (Task<TResponse>)queryHandlerType
            .GetMethod(nameof(IQueryHandler<IQuery<TResponse>, TResponse>.HandleAsync))
            !.Invoke(handler, [query, cancellationToken])!;
    }
}
