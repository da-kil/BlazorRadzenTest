using Asp.Versioning;
using PdfSharp.Fonts;
using ti8m.BeachBreak.QueryApi.Services.Pdf;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using ti8m.BeachBreak.Application.Query;
using ti8m.BeachBreak.Core.Infrastructure.Authorization;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Infrastructure.Marten;
using ti8m.BeachBreak.QueryApi.Authorization;
using ti8m.BeachBreak.QueryApi.Middleware;

namespace ti8m.BeachBreak.QueryApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Register embedded font resolver for PDFsharp (must be set before any PDF generation)
        GlobalFontSettings.FontResolver = new EmbeddedFontResolver();

        builder.AddServiceDefaults();
        builder.AddDefaultContexts();

        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = (context) =>
            {
                context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                context.ProblemDetails.Extensions.TryAdd("correlationId", context.HttpContext.Request.Headers["X-Correlation-Id"]);
            };
        });

        builder.Services.AddCors(
            options => options.AddDefaultPolicy(
                policy => policy.WithOrigins([builder.Configuration["BackendUrl"] ?? "https://localhost:5001",
                                    builder.Configuration["FrontendUrl"] ?? "https://localhost:5002"])
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()));

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

        // Register authorization cache configuration
        builder.Services.Configure<AuthorizationCacheSettings>(
            builder.Configuration.GetSection(AuthorizationCacheSettings.SectionName));

        // Register authorization cache service
        builder.Services.AddScoped<IAuthorizationCacheService, AuthorizationCacheService>();

        // Register employee role service for cache-through role retrieval
        builder.Services.AddScoped<Application.Query.Services.IEmployeeRoleService, Services.EmployeeRoleService>();

        // Register review change enrichment service for batch employee name fetching
        builder.Services.AddScoped<Application.Query.Services.IReviewChangeEnrichmentService, Application.Query.Services.ReviewChangeEnrichmentService>();

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
            option.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        builder.AddMartenInfrastructure();

        builder.Services.AddScoped<UserContext>();
        builder.Services.AddApplication(builder.Configuration);

        // Register manager authorization service
        builder.Services.AddScoped<IManagerAuthorizationService, ManagerAuthorizationService>();

        // Register PDF export services
        builder.Services.AddScoped<IQuestionnairePdfService, QuestionnairePdfService>();
        builder.Services.AddScoped<IBulkPdfExportService, BulkPdfExportService>();
        builder.Services.AddScoped<IPdfExportApplicationService, PdfExportApplicationService>();

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

        // Configure middleware and endpoints
        app.UseServiceDefaults();
        app.MapDefaultEndpoints();

        // Global exception handling middleware (must be early in pipeline)
        app.UseGlobalExceptionHandling();

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

        app.Run();
    }
}
