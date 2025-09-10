using Microsoft.AspNetCore.Builder;

namespace ti8m.BeachBreak.Core.Infrastructure.Contexts.Middleware;

internal static class UserContextMiddlewareExtensions
{
    internal static IApplicationBuilder UseUserContextMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserContextMiddleware>();
    }
}
