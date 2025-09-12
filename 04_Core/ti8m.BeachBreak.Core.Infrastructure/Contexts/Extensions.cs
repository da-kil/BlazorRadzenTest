using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ti8m.BeachBreak.Core.Infrastructure.Contexts.Middleware;
using ti8m.BeachBreak.Core.Infrastructure.Database;

namespace ti8m.BeachBreak.Core.Infrastructure.Contexts;

public static class Extensions
{
    public static IHostApplicationBuilder AddDefaultContexts(this IHostApplicationBuilder builder)
    {
        builder.AddContextsAndMiddlewares();

        return builder;
    }

    public static IHostApplicationBuilder MigrateDatabase(this IHostApplicationBuilder builder)
    {
        builder.AddDatabaseInitialization();

        return builder;
    }

    private static IHostApplicationBuilder AddContextsAndMiddlewares(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<UserContext>();
        builder.Services.AddTransient<UserContextMiddleware>();

        return builder;
    }

    public static IApplicationBuilder UseDefaultContextMiddlewares(this IApplicationBuilder builder)
    {
        return builder
            .UseUserContextMiddleware();
    }
}
