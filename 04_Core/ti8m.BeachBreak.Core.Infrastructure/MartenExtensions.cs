using Marten;
using Marten.Events.Projections;
using Marten.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ti8m.BeachBreak.Domain.CategoryAggregate;

namespace ti8m.BeachBreak.Core.Infrastructure;

public static class MartenExtensions
{
    public static IServiceCollection AddMarten(this IServiceCollection services, string connectionString)
    {
        services.AddMarten(options =>
        {
            // Use the same PostgreSQL connection
            options.Connection(connectionString);

            // Configure event store for Category aggregate
            options.Events.DatabaseSchemaName = "marten_events";

            // Configure aggregate mappings for event sourcing
            options.Events.AddEventType<ti8m.BeachBreak.Domain.CategoryAggregate.Events.CategoryAdded>();

            // Add projections for read models if needed
            // options.Projections.Add<CategoryProjection>(ProjectionLifecycle.Inline);
        });

        return services;
    }
}