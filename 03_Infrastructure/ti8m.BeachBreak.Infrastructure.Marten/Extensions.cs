using JasperFx;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Hosting;
using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Infrastructure.Marten;

public static class Extensions
{
    public static void AddMartenInfrastructure(this IHostApplicationBuilder builder, bool includeWolverine = false)
    {
        // Add NpgsqlDataSource from Aspire.Npgsql 
        builder.AddNpgsqlDataSource(connectionName: "beachbreakdb");

        var expr = builder.Services.AddMarten(options =>
        {
            // Specify that we want to use STJ as our serializer
            options.UseSystemTextJsonForSerialization();

            // If we're running in development mode, let Marten just take care
            // of all necessary schema building and patching behind the scenes
            if (builder.Environment.IsDevelopment())
            {
                options.AutoCreateSchemaObjects = AutoCreate.All;
            }

            options.Events.DatabaseSchemaName = "events";
            options.Events.MetadataConfig.EnableAll();

            options.DatabaseSchemaName = "readmodels";
            options.DisableNpgsqlLogging = !builder.Environment.IsDevelopment();

            options.Projections.Snapshot<CategoryReadModel>(SnapshotLifecycle.Inline);

        }).UseLightweightSessions().UseNpgsqlDataSource();

        if (builder.Environment.IsDevelopment())
        {
            expr.ApplyAllDatabaseChangesOnStartup();
        }
    }
}