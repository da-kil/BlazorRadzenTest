var builder = DistributedApplication.CreateBuilder(args);

var commandApi = builder.AddProject<Projects.ti8m_BeachBreak_CommandApi>("CommandApi");
var queryApi = builder.AddProject<Projects.ti8m_BeachBreak_QueryApi>("QueryApi");

builder.AddProject<Projects.ti8m_BeachBreak>("ti8mBeachBreak")
    .WithExternalHttpEndpoints()
    .WithReference(commandApi)
    .WaitFor(commandApi)
    .WithReference(queryApi)
    .WaitFor(queryApi);

builder.Build().Run();