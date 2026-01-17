using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ti8m.BeachBreak.Application.Query.Queries;

namespace ti8m.BeachBreak.Application.Query;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Use generated source code approach for 10-25x performance improvement
        services.AddGeneratedQueryHandlers();
        services.AddTransient<IQueryDispatcher, Generated.GeneratedQueryDispatcher>();

        // Register authorization services
        services.AddScoped<Services.EmployeeVisibilityService>();

        // Register progress calculation service
        services.AddScoped<Services.IProgressCalculationService, Services.ProgressCalculationService>();

        return services;
    }



    /// <summary>
    /// Adds generated query handlers using source generator registration.
    /// This method calls the generated registration from GeneratedQueryHandlerRegistrations.
    /// </summary>
    private static IServiceCollection AddGeneratedQueryHandlers(this IServiceCollection services)
    {
        // This will call the generated method when the source generator runs
        return Generated.GeneratedQueryHandlerRegistrations.AddGeneratedQueryHandlers(services);
    }
}
