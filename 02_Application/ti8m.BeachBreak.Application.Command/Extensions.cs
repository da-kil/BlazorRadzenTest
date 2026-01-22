using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Generated;

namespace ti8m.BeachBreak.Application.Command;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Use generated source code approach for 10-25x performance improvement
        services.AddGeneratedCommandHandlers();
        services.AddTransient<ICommandDispatcher, GeneratedCommandDispatcher>();

        return services;
    }

    /// <summary>
    /// Adds generated command handlers using source generator registration.
    /// This method calls the generated registration from GeneratedCommandHandlerRegistrations.
    /// </summary>
    private static IServiceCollection AddGeneratedCommandHandlers(this IServiceCollection services)
    {
        // This will call the generated method when the source generator runs
        return GeneratedCommandHandlerRegistrations.AddGeneratedCommandHandlers(services);
    }

}
