namespace ti8m.BeachBreak.Core.Infrastructure.Authorization;

public interface IAuthorizationCacheInvalidationService
{
    Task InvalidateEmployeeRoleCacheAsync(Guid employeeId, CancellationToken cancellationToken = default);
}
