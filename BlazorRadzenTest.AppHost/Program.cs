var builder = DistributedApplication.CreateBuilder(args);

var webapi = builder.AddProject<Projects.WebApi>("webapi");

builder.AddProject<Projects.BlazorRadzenTest>("blazorradzentest")
    .WithExternalHttpEndpoints()
    .WithReference(webapi)
    .WaitFor(webapi);

builder.Build().Run();