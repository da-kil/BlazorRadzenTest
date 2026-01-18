using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using ti8m.BeachBreak.CommandApi.Authorization;
using ti8m.BeachBreak.CommandApi.MinimalApi;
using ti8m.BeachBreak.Core.Infrastructure.Authorization;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Infrastructure.Marten;

namespace ti8m.BeachBreak.CommandApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
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
            builder.Services.AddScoped<IAuthorizationMiddlewareResultHandler, CommandApi.Authorization.RoleBasedAuthorizationMiddlewareResultHandler>();

            // Add distributed cache (using in-memory for now, can be replaced with Redis)
            builder.Services.AddDistributedMemoryCache();

            // Register authorization cache service
            builder.Services.AddScoped<IAuthorizationCacheService, AuthorizationCacheService>();

            // Register authorization cache invalidation service
            builder.Services.AddScoped<IAuthorizationCacheInvalidationService, AuthorizationCacheInvalidationService>();

            // Configure CORS for frontend connection
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("https://localhost:5001", "http://localhost:5001", "http://localhost:5000", "https://localhost:7000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // Add Swagger for API documentation
            builder.Services.AddEndpointsApiExplorer();
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
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                });
            });

            builder.Host.UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            });

            builder.AddNpgsqlDataSource(connectionName: "beachbreakdb");

            builder.AddMartenInfrastructure();

            builder.Services.AddScoped<UserContext>();

            // Register mapping services
            builder.Services.AddScoped<ti8m.BeachBreak.CommandApi.Services.QuestionResponseMappingService>();
            builder.Services.AddScoped<ti8m.BeachBreak.CommandApi.Services.SectionMappingService>();

            // Register EmployeeHierarchyService (Command-side only)
            // This service depends on IEmployeeAggregateRepository and is used for authorization checks
            builder.Services.AddScoped<Domain.EmployeeAggregate.Services.IEmployeeHierarchyService,
                Infrastructure.Marten.Services.EmployeeHierarchyService>();

            // Register Command-side LanguageContext (can write language preferences)
            builder.Services.AddCommandSideLanguageContext();

            // Add Command application services
            Application.Command.Extensions.AddApplication(builder.Services, builder.Configuration);

            // Add Query application services for authorization handler
            Application.Query.Extensions.AddApplication(builder.Services, builder.Configuration);

            // Register manager authorization service for command operations
            builder.Services.AddScoped<IManagerAuthorizationService, ManagerAuthorizationService>();

            // Register employee role service for cache-through role retrieval
            builder.Services.AddScoped<Application.Command.Services.IEmployeeRoleService, Services.EmployeeRoleService>();

            // Configure JSON serialization to use PascalCase (C# naming conventions)
            // Also configure AOT-compatible JSON context for minimal APIs
            builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = null; // null means PascalCase
                options.SerializerOptions.AllowTrailingCommas = true; // More lenient parsing
                options.SerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;

                // Register AOT-compatible JSON serialization context
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, ti8m.BeachBreak.CommandApi.Serialization.CommandApiJsonSerializerContext.Default);
            });

            builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // null means PascalCase
                options.JsonSerializerOptions.AllowTrailingCommas = true; // More lenient parsing
                options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;

                // Register AOT-compatible JSON serialization context
                options.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, ti8m.BeachBreak.CommandApi.Serialization.CommandApiJsonSerializerContext.Default);
            });

            var app = builder.Build();

            app.MapDefaultEndpoints();

            // Map AOT-compatible minimal API endpoints
            app.MapHealthEndpoints();
            app.MapAuthorizationCacheEndpoints();
            app.MapReplayEndpoints();
            app.MapTranslationEndpoints();
            app.MapCategoryEndpoints();
            app.MapOrganizationEndpoints();
            app.MapQuestionnaireTemplateEndpoints();
            app.MapEmployeeFeedbackEndpoints();
            app.MapEmployeeEndpoints();
            app.MapResponseEndpoints();
            app.MapAssignmentEndpoints(); // Note: Partial implementation - 47 endpoints total

            // Configure the HTTP request pipeline.
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
                    .UseDefaultContextMiddlewares();

            app.Run();
        }
    }
}
