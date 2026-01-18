using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using ti8m.BeachBreak.Application.Query;
using ti8m.BeachBreak.Core.Infrastructure.Authorization;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Infrastructure.Marten;
using ti8m.BeachBreak.QueryApi.Authorization;
using ti8m.BeachBreak.QueryApi.MinimalApi;
using ti8m.BeachBreak.QueryApi.Serialization;

namespace ti8m.BeachBreak.QueryApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();
        builder.AddDefaultContexts();

        builder.Services.AddCors(
            options => options.AddDefaultPolicy(
                policy => policy.WithOrigins([builder.Configuration["BackendUrl"] ?? "https://localhost:5001",
                                    builder.Configuration["FrontendUrl"] ?? "https://localhost:5002"])
                    .AllowAnyMethod()
                    .AllowAnyHeader()));

        builder.Services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        // Add Microsoft Entra ID Authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        // Configure authorization with claim-based policies (shared configuration)
        builder.Services.AddAuthorization(options =>
        {
            options.ConfigureAuthorizationPolicies();
        });

        // Register custom authorization middleware result handler
        builder.Services.AddScoped<IAuthorizationMiddlewareResultHandler, QueryApi.Authorization.RoleBasedAuthorizationMiddlewareResultHandler>();

        // Add distributed cache (using in-memory for now, can be replaced with Redis)
        builder.Services.AddDistributedMemoryCache();

        // Register authorization cache service
        builder.Services.AddScoped<IAuthorizationCacheService, AuthorizationCacheService>();

        // Register employee role service for cache-through role retrieval
        builder.Services.AddScoped<Application.Query.Services.IEmployeeRoleService, Services.EmployeeRoleService>();

        builder.Services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Azure Entra Editor", Version = "v1" });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
            });
        });

        builder.AddNpgsqlDataSource(connectionName: "beachbreakdb");

        builder.AddMartenInfrastructure();

        builder.Services.AddScoped<UserContext>();
        builder.Services.AddApplication(builder.Configuration);

        // Register manager authorization service
        builder.Services.AddScoped<IManagerAuthorizationService, ManagerAuthorizationService>();

        // Configure JSON serialization to explicitly use PascalCase (C# naming conventions)
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = null; // null means PascalCase
            options.SerializerOptions.PropertyNameCaseInsensitive = false; // Strict PascalCase enforcement

            // Register AOT-compatible JSON serialization context
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, QueryApiJsonSerializerContext.Default);
        });

        builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null; // null means PascalCase
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = false; // Strict PascalCase enforcement

            // Register AOT-compatible JSON serialization context
            options.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, QueryApiJsonSerializerContext.Default);
        });

        var app = builder.Build();

        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors();

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseDefaultContextMiddlewares();

        // Phase 1: Low Complexity Minimal APIs (8 endpoints)
        app.MapCategoryEndpoints();
        app.MapAnalyticsEndpoints();
        app.MapOrganizationEndpoints();
        app.MapReplayEndpoints();

        // Phase 2: Medium Complexity Minimal APIs (19 endpoints)
        app.MapAuthEndpoints();
        app.MapHREndpoints();
        app.MapTranslationEndpoints();
        app.MapEmployeeFeedbackEndpoints();
        app.MapQuestionnaireTemplateEndpoints();

        // Phase 3: High Complexity Minimal APIs (32 endpoints)
        app.MapManagersEndpoints();
        app.MapAssignmentsEndpoints();
        app.MapEmployeesEndpoints();
        app.MapResponsesEndpoints();

        app.Run();
    }
}
