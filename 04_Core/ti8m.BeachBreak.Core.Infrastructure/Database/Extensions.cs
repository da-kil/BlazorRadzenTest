using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ti8m.BeachBreak.Core.Infrastructure.Database;

public static class Extensions
{
    public static IHostApplicationBuilder AddDatabaseInitialization(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<DatabaseInitializer>();
        return builder;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync();
    }
}