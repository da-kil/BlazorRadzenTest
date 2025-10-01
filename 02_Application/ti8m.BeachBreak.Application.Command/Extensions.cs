using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommandHandlers();
        services.AddRepositories();
        services.AddTransient<ICommandDispatcher, CommandDispatcher>();

        // Register ClaimsTransformation to enrich user claims with Employee data
        services.AddScoped<Microsoft.AspNetCore.Authentication.IClaimsTransformation,
            Services.EmployeeClaimsTransformation>();

        return services;
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        services.Scan(s =>
            s.FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.Scan(s =>
            s.FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo<IRepository>(), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
