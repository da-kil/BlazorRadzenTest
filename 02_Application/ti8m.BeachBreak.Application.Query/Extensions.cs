using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQueryHandlers();
        services.AddReadModelRepositories();
        services.AddTransient<IQueryDispatcher, QueryDispatcher>();

        return services;
    }

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services)
    {
        services.Scan(s =>
            s.FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }

    private static IServiceCollection AddReadModelRepositories(this IServiceCollection services)
    {
        services.Scan(s =>
            s.FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo<IRepository>(), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
