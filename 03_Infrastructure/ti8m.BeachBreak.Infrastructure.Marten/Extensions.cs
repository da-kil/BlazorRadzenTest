using JasperFx;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Core.Infrastructure.Configuration;
using ti8m.BeachBreak.Core.Infrastructure.Services;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Services;
using ti8m.BeachBreak.Infrastructure.Marten.JsonSerialization;
using ti8m.BeachBreak.Infrastructure.Marten.Projections;
using ti8m.BeachBreak.Infrastructure.Marten.Services;

namespace ti8m.BeachBreak.Infrastructure.Marten;

public static class Extensions
{
    public static void AddMartenInfrastructure(this IHostApplicationBuilder builder, bool includeWolverine = false)
    {
        // Add NpgsqlDataSource from Aspire.Npgsql
        builder.AddNpgsqlDataSource(connectionName: "beachbreakdb");

        // Configure global System.Text.Json options for PascalCase
        var jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = null // null means PascalCase
        };

        var expr = builder.Services.AddMarten(options =>
        {
            // Specify that we want to use STJ as our serializer with custom options
            options.UseSystemTextJsonForSerialization(configure: opts =>
            {
                opts.PropertyNamingPolicy = null; // PascalCase for new data

                // Configure polymorphic serialization for QuestionResponseValue discriminated union
                opts.Converters.Add(new QuestionResponseValueJsonConverter());
            });

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
            options.Projections.Snapshot<QuestionnaireTemplateReadModel>(SnapshotLifecycle.Inline);
            options.Projections.Snapshot<EmployeeReadModel>(SnapshotLifecycle.Inline);
            options.Projections.Snapshot<OrganizationReadModel>(SnapshotLifecycle.Inline);
            options.Projections.Snapshot<QuestionnaireAssignmentReadModel>(SnapshotLifecycle.Inline);
            options.Projections.Snapshot<QuestionnaireResponseReadModel>(SnapshotLifecycle.Inline);
            options.Projections.Snapshot<ProjectionReplayReadModel>(SnapshotLifecycle.Inline);

            // Event-based projections for review change tracking
            options.Projections.Add<ReviewChangeLogProjection>(ProjectionLifecycle.Inline);

        }).UseLightweightSessions().UseNpgsqlDataSource();

        if (builder.Environment.IsDevelopment())
        {
            expr.ApplyAllDatabaseChangesOnStartup();
        }

        // Register domain services
        builder.Services.AddScoped<IQuestionnaireAssignmentService, QuestionnaireAssignmentService>();

        // Note: IEmployeeHierarchyService is registered only in CommandApi
        // It depends on IEmployeeAggregateRepository which is only available on the Command side

        // Register notification service (includes email notifications)
        builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();

        // Register replay configuration
        builder.Services.Configure<ProjectionReplaySettings>(
            builder.Configuration.GetSection(ProjectionReplaySettings.SectionName));

        // Register replay services
        builder.Services.AddSingleton<Application.Query.Services.IProjectionRegistry, Replay.MartenProjectionRegistry>();
        builder.Services.AddScoped<Replay.IProjectionRebuilder, Replay.MartenProjectionRebuilder>();
        builder.Services.AddScoped<Application.Command.Services.IProjectionReplayService, Services.ProjectionReplayService>();

        // Register aggregate repositories
        builder.Services.AddScoped<Application.Command.Repositories.IProjectionReplayAggregateRepository, Repositories.ProjectionReplayAggregateRepository>();
    }
}