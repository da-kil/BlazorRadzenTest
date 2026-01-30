# API Controller Implementation Patterns

## Overview

This guide covers standardized patterns for QueryApi and CommandApi controller implementation, focusing on error handling, authorization, and data enrichment.

---

## Error Handling Pattern

**CRITICAL**: QueryApi controllers follow a standardized error handling pattern to ensure clean, maintainable code.

### Core Rule

**NEVER** wrap controller actions in try-catch blocks unless there's a specific reason to handle the exception locally. The middleware and `CreateResponse` method already handle errors appropriately.

### Pattern Implementation

**❌ WRONG**: Unnecessary try-catch blocks
```csharp
public async Task<IActionResult> GetSomething(Guid id)
{
    try
    {
        var result = await queryDispatcher.QueryAsync(new SomeQuery(id));
        return CreateResponse(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error getting something");
        return StatusCode(500, "An error occurred");
    }
}
```

**✅ CORRECT**: Let middleware handle exceptions
```csharp
public async Task<IActionResult> GetSomething(Guid id)
{
    var result = await queryDispatcher.QueryAsync(new SomeQuery(id));
    return CreateResponse(result);
}
```

### Rationale

- `CreateResponse` already handles Result failures appropriately
- Middleware handles unhandled exceptions with proper logging
- Try-catch adds unnecessary code and can hide issues
- Controllers should be thin HTTP orchestration layer

---

## Authorization Pattern with ExecuteWithAuthorizationAsync

### Core Pattern

**ALWAYS** use the `ExecuteWithAuthorizationAsync` helper method for manager-restricted endpoints:

```csharp
[HttpGet("{id:guid}")]
[Authorize(Policy = "TeamLead")]
public async Task<IActionResult> GetSomething(Guid id)
{
    return await ExecuteWithAuthorizationAsync(
        authorizationService,
        employeeRoleService,
        logger,
        async (managerId, hasElevatedRole) =>
        {
            // Execute query
            var result = await queryDispatcher.QueryAsync(new SomeQuery(id));

            if (!result.Succeeded)
                return Result<SomeDto>.Fail(result.Message ?? "Failed", result.StatusCode);

            // Map to DTO
            var dto = MapToDto(result.Payload);
            return Result<SomeDto>.Success(dto);
        },
        resourceId: id,
        requiresResourceAccess: true);
}
```

### Benefits

- Consistent authorization logic across controllers
- Automatic handling of elevated roles (HR/Admin)
- Centralized logging with caller name tracking
- Eliminates ~20-30 lines of duplicate authorization code per method

---

## Data Enrichment Services Pattern

### Core Rule

**ALWAYS** use dedicated enrichment services for batch data fetching to avoid N+1 query problems.

### Problem: N+1 Query Anti-Pattern

```csharp
// ❌ BAD: Fetches employee name for each change individually
var employeeIds = changes.Select(c => c.ChangedByEmployeeId).Distinct();
var employeeNames = new Dictionary<Guid, string>();

foreach (var employeeId in employeeIds)  // N+1 problem!
{
    var employee = await queryDispatcher.QueryAsync(new EmployeeQuery(employeeId));
    employeeNames[employeeId] = $"{employee.FirstName} {employee.LastName}";
}
```

### Solution: Batch Enrichment Service

```csharp
// ✅ GOOD: Fetches all employee names in a single query
var employeeIds = changes.Select(c => c.ChangedByEmployeeId).Distinct();
var employeeNames = await enrichmentService.GetEmployeeNamesAsync(
    employeeIds,
    HttpContext.RequestAborted);
```

### Creating Enrichment Services

**Pattern**:
1. Create interface in `02_Application/ti8m.BeachBreak.Application.Query/Services/`
2. Implement with efficient batch queries
3. Register in DI container (QueryApi/Program.cs)
4. Inject into controllers that need data enrichment

### Example: ReviewChangeEnrichmentService

```csharp
// Interface
public interface IReviewChangeEnrichmentService
{
    Task<Dictionary<Guid, string>> GetEmployeeNamesAsync(
        IEnumerable<Guid> employeeIds,
        CancellationToken cancellationToken = default);
}

// Implementation
public class ReviewChangeEnrichmentService : IReviewChangeEnrichmentService
{
    private readonly IEmployeeRepository employeeRepository;

    public async Task<Dictionary<Guid, string>> GetEmployeeNamesAsync(
        IEnumerable<Guid> employeeIds,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = employeeIds.Distinct().ToList();
        if (!distinctIds.Any())
            return new Dictionary<Guid, string>();

        // Single batch query for all employees
        var allEmployees = await employeeRepository.GetEmployeesAsync(
            includeDeleted: false,
            cancellationToken: cancellationToken);

        return allEmployees
            .Where(e => distinctIds.Contains(e.Id))
            .ToDictionary(
                e => e.Id,
                e => $"{e.FirstName} {e.LastName}");
    }
}

// Registration
builder.Services.AddScoped<IReviewChangeEnrichmentService, ReviewChangeEnrichmentService>();
```

---

## Controller Responsibilities

### Controllers Should ONLY

Controllers in QueryApi should **ONLY**:
1. Validate HTTP input (ModelState)
2. Dispatch queries to handlers
3. Map results to DTOs
4. Return HTTP responses via `CreateResponse`

### Controllers Should NEVER

Controllers should **NEVER**:
1. Contain business logic
2. Perform data transformations beyond simple DTO mapping
3. Execute loops or complex operations
4. Have methods longer than ~40 lines
5. Have try-catch blocks (except for specific local handling needs)

---

## Historical Context

### 2026-01-23: Controller Simplification

**Phase 6 of controller refactoring**:
- Removed ~100 lines of try-catch blocks from AssignmentsController
- Introduced `ExecuteWithAuthorizationAsync` pattern for consistent authorization
- Created `ReviewChangeEnrichmentService` to eliminate N+1 query problem
- Established pattern for thin controllers with centralized error handling

### Controller Evolution

**Before Simplification**:
- Controllers contained extensive try-catch blocks
- Duplicate authorization logic in every method
- N+1 query problems with employee name lookups
- Inconsistent error handling across endpoints

**After Simplification**:
- Middleware handles exceptions consistently
- Centralized authorization with ExecuteWithAuthorizationAsync
- Batch enrichment services eliminate N+1 problems
- Thin controllers focused on HTTP orchestration

---

## References

### Implementation Files

- **Enrichment Service Example**: `ReviewChangeEnrichmentService.cs` (Application.Query/Services)
- **Authorization Helper**: `BaseController.ExecuteWithAuthorizationAsync` (QueryApi/Controllers)
- **Controller Simplification Plan**: `docs/planning/controller-simplification.md`

### Related Documentation

- **CLAUDE.md**: Core controller response pattern
- **Query Dispatcher**: `docs/implementation/query-dispatcher.md`
- **Command Dispatcher**: `docs/implementation/command-dispatcher.md`

---

*Last Updated: 2026-01-30*
*Document Version: 1.0*