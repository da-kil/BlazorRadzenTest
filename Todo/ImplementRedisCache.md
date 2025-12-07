# Redis Distributed Cache Implementation Plan

## Problem Statement
Current translation cache uses `DistributedMemoryCache` which creates isolated caches per backend instance. When "clear translation cache" is clicked, it only clears the cache in the instance that receives the request, not across all instances.

## Solution Overview
Replace in-memory distributed cache with Redis to enable true distributed caching across multiple backend instances with coordinated cache invalidation.

## Implementation Plan

### Phase 1: Infrastructure Setup

#### 1. Add Redis to Aspire Orchestration
**File:** `Aspire/ti8m.BeachBreak.AppHost/Program.cs`

```csharp
// Add Redis container after PostgreSQL
var redis = builder.AddRedis("redis")
    .WithDataVolume(isReadOnly: false)
    .WithLifetime(ContainerLifetime.Persistent);

// Update API projects to reference Redis
var commandApi = builder.AddProject<Projects.ti8m_BeachBreak_CommandApi>("CommandApi")
    .WaitFor(postgresdb)
    .WaitFor(redis)  // Add Redis dependency
    .WithReference(postgresdb)
    .WithReference(redis);  // Add Redis reference

var queryApi = builder.AddProject<Projects.ti8m_BeachBreak_QueryApi>("QueryApi")
    .WaitFor(postgresdb)
    .WaitFor(redis)  // Add Redis dependency
    .WithReference(postgresdb)
    .WithReference(redis);  // Add Redis reference
```

#### 2. Add Redis NuGet Packages
**Files:**
- `03_Infrastructure/ti8m.BeachBreak.CommandApi/ti8m.BeachBreak.CommandApi.csproj`
- `03_Infrastructure/ti8m.BeachBreak.QueryApi/ti8m.BeachBreak.QueryApi.csproj`

```xml
<PackageReference Include="Aspire.StackExchange.Redis" Version="9.4.2" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="9.0.9" />
```

### Phase 2: Service Registration Updates

#### 3. Update Command API Service Registration
**File:** `03_Infrastructure/ti8m.BeachBreak.CommandApi/Program.cs`

```csharp
// Replace line 59-60:
// OLD: builder.Services.AddDistributedMemoryCache();
// NEW:
builder.AddRedisClient("redis");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
    options.InstanceName = "BeachBreak_Command";
});
```

#### 4. Update Query API Service Registration
**File:** `03_Infrastructure/ti8m.BeachBreak.QueryApi/Program.cs`

```csharp
// Replace lines 50-51:
// OLD: builder.Services.AddDistributedMemoryCache();
// NEW:
builder.AddRedisClient("redis");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
    options.InstanceName = "BeachBreak_Query";
});
```

### Phase 3: Enhanced Cache Invalidation

#### 5. Create Distributed Cache Invalidation Service Interface
**New File:** `04_Core/ti8m.BeachBreak.Core.Infrastructure/Services/IDistributedCacheInvalidationService.cs`

```csharp
namespace ti8m.BeachBreak.Core.Infrastructure.Services;

/// <summary>
/// Service for coordinating cache invalidation across multiple backend instances
/// </summary>
public interface IDistributedCacheInvalidationService
{
    /// <summary>
    /// Invalidate cache entries across all backend instances
    /// </summary>
    Task InvalidateAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate multiple cache patterns across all backend instances
    /// </summary>
    Task InvalidatePatternsAsync(IEnumerable<string> patterns, CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcast cache invalidation to all instances
    /// </summary>
    Task BroadcastInvalidationAsync(string channel, object data, CancellationToken cancellationToken = default);
}
```

#### 6. Implement Redis Pub/Sub Cache Invalidation Service
**New File:** `03_Infrastructure/ti8m.BeachBreak.Infrastructure.Marten/Services/RedisDistributedCacheInvalidationService.cs`

Key features:
- Uses Redis Pub/Sub for cross-instance coordination
- Prevents processing own invalidation messages
- Graceful error handling
- Instance identification to avoid loops

#### 7. Update UITranslationService for Distributed Invalidation
**File:** `03_Infrastructure/ti8m.BeachBreak.Infrastructure.Marten/Services/UITranslationService.cs`

**Changes:**
- Add `IDistributedCacheInvalidationService` dependency injection
- Update `InvalidateCacheAsync()` to use distributed invalidation
- Update `InvalidateTranslationCachesAsync()` for cross-instance coordination
- Maintain backwards compatibility with null service

### Phase 4: Configuration and Registration

#### 8. Register Distributed Cache Invalidation Services
**Files:**
- `03_Infrastructure/ti8m.BeachBreak.CommandApi/Program.cs`
- `03_Infrastructure/ti8m.BeachBreak.QueryApi/Program.cs`

```csharp
// Register Redis connection multiplexer
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("redis");
    return ConnectionMultiplexer.Connect(connectionString);
});

// Register distributed cache invalidation service
builder.Services.AddScoped<IDistributedCacheInvalidationService, RedisDistributedCacheInvalidationService>();
```

#### 9. Configuration Management
Add Redis configuration to `appsettings.json`:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "BeachBreak",
    "DefaultDatabase": 0
  }
}
```

## Benefits After Implementation

1. **True Distributed Caching**: Cache invalidation works across all backend instances
2. **Improved Performance**: Shared cache reduces database queries
3. **Scalability**: Support for horizontal scaling of backend services
4. **Consistency**: Synchronized cache state across all instances
5. **Maintains Existing API**: No changes to cache invalidation endpoints
6. **Frontend Integration**: Frontend cache clearing continues to work

## Testing Strategy

1. **Unit Tests**: Redis connection, cache invalidation service, UITranslationService updates
2. **Integration Tests**: Multiple API instances with Redis, cache invalidation across instances
3. **Manual Testing**:
   - Start multiple backend instances
   - Clear cache from one instance
   - Verify cache is cleared on all instances
   - Test frontend cache clearing

## Deployment Considerations

1. **Backwards Compatibility**: Service gracefully degrades if Redis unavailable
2. **Production Config**: Environment variables for Redis connection
3. **Health Checks**: Monitor Redis connectivity
4. **Performance Monitoring**: Track cache hit/miss ratios

## Critical Files Summary

1. `Aspire/ti8m.BeachBreak.AppHost/Program.cs` - Add Redis infrastructure
2. `03_Infrastructure/ti8m.BeachBreak.CommandApi/Program.cs` - Replace memory cache with Redis
3. `03_Infrastructure/ti8m.BeachBreak.QueryApi/Program.cs` - Replace memory cache with Redis
4. `03_Infrastructure/ti8m.BeachBreak.Infrastructure.Marten/Services/UITranslationService.cs` - Add distributed invalidation
5. `04_Core/ti8m.BeachBreak.Core.Infrastructure/Services/IDistributedCacheInvalidationService.cs` - New service interface
6. `03_Infrastructure/ti8m.BeachBreak.Infrastructure.Marten/Services/RedisDistributedCacheInvalidationService.cs` - New service implementation

## Implementation Sequence

1. **Phase 1**: Infrastructure setup (Redis container + packages)
2. **Phase 2**: Service registration updates (replace memory cache)
3. **Phase 3**: Distributed invalidation service creation
4. **Phase 4**: UITranslationService enhancement
5. **Testing**: Configuration and comprehensive testing

## Success Criteria

- [ ] Redis container runs in Aspire orchestration
- [ ] Both CommandApi and QueryApi use Redis for distributed caching
- [ ] Cache invalidation from any instance clears cache on all instances
- [ ] Frontend cache clearing triggers backend cache clearing
- [ ] Existing translation cache endpoints continue to work
- [ ] System gracefully handles Redis unavailability
- [ ] Performance is maintained or improved