using Aspire.Hosting;

// ============================================================================
// ASPIRE SERVICE ORCHESTRATION - NOT HARDCODED URLS
// ============================================================================
// This file uses Aspire's service discovery pattern. URLs are NOT hardcoded here.
// Service names like "CommandApi" and "QueryApi" become discoverable service endpoints.
// Actual URLs (ports, protocols) are resolved dynamically by Aspire at runtime.
//
// How it works:
// 1. Services registered here by NAME (not URL)
// 2. Aspire assigns dynamic ports/URLs when services start
// 3. Other services reference by NAME using service discovery
// 4. URLs are injected into consuming services as configuration
//
// Service Discovery Pattern:
// - Service "CommandApi" becomes discoverable as "services:CommandApi:https:0"
// - Service "QueryApi" becomes discoverable as "services:QueryApi:https:0"
// - Frontend can request these URLs from configuration at runtime
//
// This is FLEXIBLE, not hardcoded - same pattern works across dev/staging/prod
// ============================================================================

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL Database Container
// This creates a discoverable database service that other services can reference
var postgresdb = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Session)
    .AddDatabase("beachbreakdb");

// COMMAND API SERVICE REGISTRATION
// Service Name: "CommandApi" - this becomes the discoverable service endpoint
// NOT hardcoded URL - Aspire assigns dynamic port at runtime
// Frontend can discover this service using "services:CommandApi:https:0" configuration key
var commandApi = builder.AddProject<Projects.ti8m_BeachBreak_CommandApi>("commandapi")
    .WaitFor(postgresdb)                    // Wait for database before starting
    .WithReference(postgresdb)              // Service discovery reference to database
    .WithEnvironment("AzureAd__Instance", "https://login.microsoftonline.com/")
    .WithEnvironment("AzureAd__Domain", builder.Configuration["AzureAd:Domain"] ?? "{DOMAIN}.onmicrosoft.com")
    .WithEnvironment("AzureAd__TenantId", builder.Configuration["AzureAd:TenantId"] ?? "{TENANT_ID}")
    .WithEnvironment("AzureAd__ClientId", builder.Configuration["AzureAd:CommandApi:ClientId"] ?? "{CLIENT_ID}")
    .WithEnvironment("AzureAd__Audience", builder.Configuration["AzureAd:CommandApi:Audience"] ?? "api://{CLIENT_ID}")
    .WithEnvironment("AzureAd__ClientSecret", builder.Configuration["AzureAd:CommandApi:ClientSecret"] ?? string.Empty)
    .WithEnvironment("AzureAd__Scope", builder.Configuration["AzureAd:Scope"] ?? string.Empty);

// QUERY API SERVICE REGISTRATION
// Service Name: "QueryApi" - this becomes the discoverable service endpoint
// NOT hardcoded URL - Aspire assigns dynamic port at runtime
// Frontend can discover this service using "services:QueryApi:https:0" configuration key
var queryApi = builder.AddProject<Projects.ti8m_BeachBreak_QueryApi>("queryapi")
    .WithReference(postgresdb)              // Service discovery reference to database
    .WithEnvironment("AzureAd__Instance", "https://login.microsoftonline.com/")
    .WithEnvironment("AzureAd__Domain", builder.Configuration["AzureAd:Domain"] ?? "{DOMAIN}.onmicrosoft.com")
    .WithEnvironment("AzureAd__TenantId", builder.Configuration["AzureAd:TenantId"] ?? "{TENANT_ID}")
    .WithEnvironment("AzureAd__ClientId", builder.Configuration["AzureAd:QueryApi:ClientId"] ?? "{CLIENT_ID}")
    .WithEnvironment("AzureAd__Audience", builder.Configuration["AzureAd:QueryApi:Audience"] ?? "api://{CLIENT_ID}")
    .WithEnvironment("AzureAd__ClientSecret", builder.Configuration["AzureAd:QueryApi:ClientSecret"] ?? string.Empty)
    .WithEnvironment("AzureAd__Scope", builder.Configuration["AzureAd:Scope"] ?? string.Empty);

// FRONTEND SERVICE REGISTRATION
// Service Name: "ti8mBeachBreak" - the main frontend application
// WithReference(commandApi) and WithReference(queryApi) enable SERVICE DISCOVERY
// This injects API URLs as configuration - frontend gets them dynamically at runtime
builder.AddProject<Projects.ti8m_BeachBreak>("ti8m-beachbreak")
    .WithExternalHttpEndpoints()            // Make frontend accessible externally
    .WithReference(commandApi)              // SERVICE DISCOVERY: Injects CommandApi URL as configuration
    .WaitFor(commandApi)                    // Wait for CommandApi to be ready
    .WithReference(queryApi)                // SERVICE DISCOVERY: Injects QueryApi URL as configuration
    .WaitFor(queryApi)                      // Wait for QueryApi to be ready
    .WithEnvironment("AzureAd__Instance", "https://login.microsoftonline.com/")
    .WithEnvironment("AzureAd__Domain", builder.Configuration["AzureAd:Domain"] ?? "{DOMAIN}.onmicrosoft.com")
    .WithEnvironment("AzureAd__TenantId", builder.Configuration["AzureAd:TenantId"] ?? "{TENANT_ID}")
    .WithEnvironment("AzureAd__ClientId", builder.Configuration["AzureAd:Frontend:ClientId"] ?? "{CLIENT_ID}")
    .WithEnvironment("AzureAd__ClientSecret", builder.Configuration["AzureAd:Frontend:ClientSecret"] ?? string.Empty)
    .WithEnvironment("AzureAd__CallbackPath", "/signin-oidc")
    .WithEnvironment("AzureAd__SignedOutCallbackPath", "/signout-callback-oidc")
    .WithEnvironment("AzureAd__Scope", builder.Configuration["AzureAd:Scope"] ?? string.Empty);

builder.Build().Run();