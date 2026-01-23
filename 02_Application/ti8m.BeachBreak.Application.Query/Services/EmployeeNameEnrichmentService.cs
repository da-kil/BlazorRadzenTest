using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Implementation of employee name enrichment service with distributed caching.
/// Derives display names from immutable user ID facts stored in events.
/// Uses distributed cache to ensure consistency across multiple service instances.
/// </summary>
public class EmployeeNameEnrichmentService : IEmployeeNameEnrichmentService
{
    private readonly IEmployeeRepository employeeRepository;
    private readonly IDistributedCache cache;
    private readonly ILogger<EmployeeNameEnrichmentService> logger;
    private const string CacheKeyPrefix = "employee-name:";

    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    public EmployeeNameEnrichmentService(
        IEmployeeRepository employeeRepository,
        IDistributedCache cache,
        ILogger<EmployeeNameEnrichmentService> logger)
    {
        this.employeeRepository = employeeRepository;
        this.cache = cache;
        this.logger = logger;
    }

    public async Task<string> GetEmployeeNameAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{employeeId}";

        try
        {
            // Try cache first
            var cachedBytes = await cache.GetAsync(cacheKey, cancellationToken);
            if (cachedBytes != null)
            {
                var cachedName = Encoding.UTF8.GetString(cachedBytes);
                logger.LogDebug("Employee name retrieved from cache for ID: {EmployeeId}", employeeId);
                return cachedName;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error retrieving employee name from cache for ID: {EmployeeId}", employeeId);
        }

        // Fetch from database
        var employee = await employeeRepository.GetEmployeeByIdAsync(employeeId, cancellationToken);

        string name;
        if (employee != null && !employee.IsDeleted)
        {
            name = $"{employee.FirstName} {employee.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Unknown";
            }
        }
        else
        {
            name = "Unknown";
        }

        try
        {
            // Cache the result
            var bytes = Encoding.UTF8.GetBytes(name);
            await cache.SetAsync(cacheKey, bytes, CacheOptions, cancellationToken);
            logger.LogDebug("Cached employee name for ID: {EmployeeId}", employeeId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error caching employee name for ID: {EmployeeId}", employeeId);
        }

        return name;
    }

    public async Task<Dictionary<Guid, string>> GetEmployeeNamesAsync(IEnumerable<Guid> employeeIds, CancellationToken cancellationToken = default)
    {
        var distinctIds = employeeIds.Distinct().ToList();
        var result = new Dictionary<Guid, string>();
        var idsToFetch = new List<Guid>();

        // Check cache for each ID
        foreach (var id in distinctIds)
        {
            var cacheKey = $"{CacheKeyPrefix}{id}";
            try
            {
                var cachedBytes = await cache.GetAsync(cacheKey, cancellationToken);
                if (cachedBytes != null)
                {
                    var cachedName = Encoding.UTF8.GetString(cachedBytes);
                    result[id] = cachedName;
                }
                else
                {
                    idsToFetch.Add(id);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error retrieving employee name from cache for ID: {EmployeeId}, will fetch from database", id);
                idsToFetch.Add(id);
            }
        }

        // Fetch missing IDs from database
        if (idsToFetch.Any())
        {
            var employees = await employeeRepository.GetEmployeesAsync(
                includeDeleted: false,
                cancellationToken: cancellationToken);

            var employeeDict = employees
                .Where(e => idsToFetch.Contains(e.Id))
                .ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());

            foreach (var id in idsToFetch)
            {
                string name;
                if (employeeDict.TryGetValue(id, out var employeeName) && !string.IsNullOrWhiteSpace(employeeName))
                {
                    name = employeeName;
                }
                else
                {
                    name = "Unknown";
                }

                // Cache the result
                try
                {
                    var cacheKey = $"{CacheKeyPrefix}{id}";
                    var bytes = Encoding.UTF8.GetBytes(name);
                    await cache.SetAsync(cacheKey, bytes, CacheOptions, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error caching employee name for ID: {EmployeeId}", id);
                }

                result[id] = name;
            }

            logger.LogDebug("Batch fetched and cached {Count} employee names", idsToFetch.Count);
        }

        return result;
    }
}
