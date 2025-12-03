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

        // Add memory cache for UITranslationService
        builder.Services.AddMemoryCache();

        // Register authorization cache service
        builder.Services.AddScoped<IAuthorizationCacheService, AuthorizationCacheService>();

        // Register employee role service for cache-through role retrieval
        builder.Services.AddScoped<Application.Query.Services.IEmployeeRoleService, Services.EmployeeRoleService>();

        builder.Services.AddControllers();

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
        });

        builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null; // null means PascalCase
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = false; // Strict PascalCase enforcement
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

        app.UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseDefaultContextMiddlewares()
                .UseEndpoints(endpoints =>
                {
                    endpoints
                        .MapControllers();
                });

        // Seed initial translations after full application initialization
        await SeedTranslationsAsync(app);

        app.Run();
    }

    /// <summary>
    /// Seeds initial translations with proper error handling and retry logic
    /// </summary>
    private static async Task SeedTranslationsAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var translationService = scope.ServiceProvider.GetRequiredService<ti8m.BeachBreak.Application.Query.Services.IUITranslationService>();

        const int maxRetries = 3;
        const int delayMs = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Seeding translations attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
                var seedCount = await translationService.SeedInitialTranslationsAsync();

                if (seedCount > 0)
                {
                    logger.LogInformation("Successfully seeded {Count} translations", seedCount);
                }
                else
                {
                    logger.LogInformation("Translations already exist, skipping seed");
                }

                return; // Success, exit retry loop
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed translations on attempt {Attempt}/{MaxRetries}", attempt, maxRetries);

                if (attempt < maxRetries)
                {
                    logger.LogInformation("Waiting {DelayMs}ms before retry", delayMs);
                    await Task.Delay(delayMs);
                }
                else
                {
                    logger.LogError("Failed to seed translations after {MaxRetries} attempts. Translation system may not work properly.", maxRetries);
                    // Don't throw - let the application start anyway
                }
            }
        }
    }
}
