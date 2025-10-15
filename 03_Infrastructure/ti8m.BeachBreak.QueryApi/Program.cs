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

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Employee", policy => policy.RequireRole("Employee"));
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("HR", policy => policy.RequireRole("HR"));
            options.AddPolicy("HRLead", policy => policy.RequireRole("HRLead"));
            options.AddPolicy("TeamLead", policy => policy.RequireRole("TeamLead"));
        });

        // Register custom authorization middleware result handler
        builder.Services.AddScoped<IAuthorizationMiddlewareResultHandler, QueryApi.Authorization.RoleBasedAuthorizationMiddlewareResultHandler>();

        // Add distributed cache (using in-memory for now, can be replaced with Redis)
        builder.Services.AddDistributedMemoryCache();

        // Register authorization cache service
        builder.Services.AddScoped<IAuthorizationCacheService, AuthorizationCacheService>();

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

        app.Run();
    }
}
