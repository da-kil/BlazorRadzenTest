var builder = DistributedApplication.CreateBuilder(args);

var webapi = builder.AddProject<Projects.WebApi>("webapi");

builder.AddProject<Projects.ti8m_BeachBreak>("ti8m-beachbreak")
    .WithExternalHttpEndpoints()
    .WithReference(webapi)
    .WaitFor(webapi);

builder.Build().Run();