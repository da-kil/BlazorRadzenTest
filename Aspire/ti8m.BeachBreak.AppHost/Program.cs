using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgresdb = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin()
    .AddDatabase("beachbreakdb");

var commandApi = builder.AddProject<Projects.ti8m_BeachBreak_CommandApi>("CommandApi")
    .WithReference(postgresdb);

var queryApi = builder.AddProject<Projects.ti8m_BeachBreak_QueryApi>("QueryApi")
    .WithReference(postgresdb);

builder.AddProject<Projects.ti8m_BeachBreak>("ti8mBeachBreak")
    .WithExternalHttpEndpoints()
    .WithReference(commandApi)
    .WaitFor(commandApi)
    .WithReference(queryApi)
    .WaitFor(queryApi);

builder.Build().Run();