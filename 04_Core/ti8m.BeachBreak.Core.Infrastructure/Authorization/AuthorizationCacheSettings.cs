namespace ti8m.BeachBreak.Core.Infrastructure.Authorization;

/// <summary>
/// Configuration settings for the authorization cache service.
/// </summary>
public class AuthorizationCacheSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "AuthorizationCache";

    /// <summary>
    /// Duration in minutes for which employee roles are cached.
    /// Default: 15 minutes
    ///
    /// Considerations for different environments:
    /// - Development: Lower values (5-10 minutes) for faster testing of role changes
    /// - Production: Higher values (15-30 minutes) for better performance
    /// - Load testing: Very low values (1-2 minutes) to test cache invalidation scenarios
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Gets the cache duration as a TimeSpan for use with DistributedCacheEntryOptions.
    /// </summary>
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(CacheDurationMinutes);
}