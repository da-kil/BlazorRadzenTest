namespace ti8m.BeachBreak.Core.Infrastructure.Authorization;

public class AuthorizationCacheInvalidationService : IAuthorizationCacheInvalidationService
{
    private readonly IAuthorizationCacheService authorizationCacheService;

    public AuthorizationCacheInvalidationService(IAuthorizationCacheService authorizationCacheService)
    {
        this.authorizationCacheService = authorizationCacheService;
    }

    public Task InvalidateEmployeeRoleCacheAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return authorizationCacheService.InvalidateEmployeeRoleCacheAsync(employeeId, cancellationToken);
    }
}
