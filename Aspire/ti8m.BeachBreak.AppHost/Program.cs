using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgresdb = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Session)
    .AddDatabase("beachbreakdb");

var commandApi = builder.AddProject<Projects.ti8m_BeachBreak_CommandApi>("CommandApi")
    .WaitFor(postgresdb)
    .WithReference(postgresdb)
    .WithEnvironment("AzureAd__Instance", "https://login.microsoftonline.com/")
    .WithEnvironment("AzureAd__Domain", builder.Configuration["AzureAd:Domain"] ?? "{DOMAIN}.onmicrosoft.com")
    .WithEnvironment("AzureAd__TenantId", builder.Configuration["AzureAd:TenantId"] ?? "{TENANT_ID}")
    .WithEnvironment("AzureAd__ClientId", builder.Configuration["AzureAd:CommandApi:ClientId"] ?? "{CLIENT_ID}")
    .WithEnvironment("AzureAd__Audience", builder.Configuration["AzureAd:CommandApi:Audience"] ?? "api://{CLIENT_ID}")
    .WithEnvironment("AzureAd__ClientSecret", builder.Configuration["AzureAd:CommandApi:ClientSecret"] ?? string.Empty)
    .WithEnvironment("AzureAd__Scope", builder.Configuration["AzureAd:Scope"] ?? string.Empty);

var queryApi = builder.AddProject<Projects.ti8m_BeachBreak_QueryApi>("QueryApi")
    .WithReference(postgresdb)
    .WithEnvironment("AzureAd__Instance", "https://login.microsoftonline.com/")
    .WithEnvironment("AzureAd__Domain", builder.Configuration["AzureAd:Domain"] ?? "{DOMAIN}.onmicrosoft.com")
    .WithEnvironment("AzureAd__TenantId", builder.Configuration["AzureAd:TenantId"] ?? "{TENANT_ID}")
    .WithEnvironment("AzureAd__ClientId", builder.Configuration["AzureAd:QueryApi:ClientId"] ?? "{CLIENT_ID}")
    .WithEnvironment("AzureAd__Audience", builder.Configuration["AzureAd:QueryApi:Audience"] ?? "api://{CLIENT_ID}")
    .WithEnvironment("AzureAd__ClientSecret", builder.Configuration["AzureAd:QueryApi:ClientSecret"] ?? string.Empty)
    .WithEnvironment("AzureAd__Scope", builder.Configuration["AzureAd:Scope"] ?? string.Empty);

builder.AddProject<Projects.ti8m_BeachBreak>("ti8mBeachBreak")
    .WithExternalHttpEndpoints()
    .WithReference(commandApi)
    .WaitFor(commandApi)
    .WithReference(queryApi)
    .WaitFor(queryApi)
    .WithEnvironment("AzureAd__Instance", "https://login.microsoftonline.com/")
    .WithEnvironment("AzureAd__Domain", builder.Configuration["AzureAd:Domain"] ?? "{DOMAIN}.onmicrosoft.com")
    .WithEnvironment("AzureAd__TenantId", builder.Configuration["AzureAd:TenantId"] ?? "{TENANT_ID}")
    .WithEnvironment("AzureAd__ClientId", builder.Configuration["AzureAd:Frontend:ClientId"] ?? "{CLIENT_ID}")
    .WithEnvironment("AzureAd__ClientSecret", builder.Configuration["AzureAd:Frontend:ClientSecret"] ?? string.Empty)
    .WithEnvironment("AzureAd__CallbackPath", "/signin-oidc")
    .WithEnvironment("AzureAd__SignedOutCallbackPath", "/signout-callback-oidc")
    .WithEnvironment("AzureAd__Scope", builder.Configuration["AzureAd:Scope"] ?? string.Empty);

builder.Build().Run();