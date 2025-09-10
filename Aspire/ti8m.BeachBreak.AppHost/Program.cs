var builder = DistributedApplication.CreateBuilder(args);

var commandApi = builder.AddProject<Projects.ti8m_BeachBreak_CommandApi>("CommandApi");

builder.AddProject<Projects.ti8m_BeachBreak>("ti8mBeachBreak")
    .WithExternalHttpEndpoints()
    .WithReference(commandApi)
    .WaitFor(commandApi);

builder.Build().Run();