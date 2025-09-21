using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ti8m.BeachBreak.Client.Services.Enhanced;

/// <summary>
/// Extension methods for registering enhanced API services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers enhanced API services with default configuration
    /// </summary>
    public static IServiceCollection AddEnhancedApiServices(this IServiceCollection services)
    {
        return services.AddEnhancedApiServices(new ApiServiceOptions());
    }

    /// <summary>
    /// Registers enhanced API services with custom configuration
    /// </summary>
    public static IServiceCollection AddEnhancedApiServices(this IServiceCollection services, ApiServiceOptions options)
    {
        // Register the configuration
        services.AddSingleton(options);

        // Register enhanced services
        services.AddScoped<EnhancedApiService>(provider =>
        {
            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var logger = provider.GetRequiredService<ILogger<EnhancedApiService>>();
            return new ConcreteEnhancedApiService(factory, logger, options);
        });

        return services;
    }

    /// <summary>
    /// Configures enhanced API services with a configuration action
    /// </summary>
    public static IServiceCollection AddEnhancedApiServices(this IServiceCollection services, Action<ApiServiceOptions> configureOptions)
    {
        var options = new ApiServiceOptions();
        configureOptions(options);
        return services.AddEnhancedApiServices(options);
    }

    /// <summary>
    /// Concrete implementation for DI registration
    /// </summary>
    private class ConcreteEnhancedApiService : EnhancedApiService
    {
        public ConcreteEnhancedApiService(IHttpClientFactory factory, ILogger<EnhancedApiService> logger, ApiServiceOptions options)
            : base(factory, logger, options)
        {
        }
    }
}