using ti8m.BeachBreak.Client.Services;

namespace ti8m.BeachBreak.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuestionnaireServices(this IServiceCollection services)
    {
        // HTTP Clients
        services.AddHttpClient("QueryClient", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7001/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("CommandClient", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7002/");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        // Core Services
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<IApiClientService, ApiClientService>();
        services.AddScoped<IQuestionnaireService, QuestionnaireService>();

        // Legacy services removed - migration completed

        // Other existing services
        services.AddScoped<IQuestionnaireApiService, QuestionnaireApiService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ICategoryApiService, CategoryApiService>();

        return services;
    }

    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        // Add authentication-related services here
        // This is where you would configure OAuth, JWT, etc.

        return services;
    }

    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
        });

        return services;
    }
}