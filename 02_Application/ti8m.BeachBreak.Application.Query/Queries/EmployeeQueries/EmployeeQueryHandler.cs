using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Mappers;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeQueryHandler :
    IQueryHandler<EmployeeListQuery, Result<IEnumerable<Employee>>>,
    IQueryHandler<EmployeeQuery, Result<Employee?>>
{
    private readonly IEmployeeRepository repository;
    private readonly IOrganizationRepository organizationRepository;
    private readonly ILogger<EmployeeQueryHandler> logger;
    private readonly UserContext userContext;
    private readonly EmployeeVisibilityService visibilityService;

    public EmployeeQueryHandler(
        IEmployeeRepository repository,
        IOrganizationRepository organizationRepository,
        ILogger<EmployeeQueryHandler> logger,
        UserContext userContext,
        EmployeeVisibilityService visibilityService)
    {
        this.repository = repository;
        this.organizationRepository = organizationRepository;
        this.logger = logger;
        this.userContext = userContext;
        this.visibilityService = visibilityService;
    }

    public async Task<Result<IEnumerable<Employee>>> HandleAsync(EmployeeListQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogEmployeeListQueryStarting(query.IncludeDeleted, query.OrganizationNumber, query.Role, query.ManagerId);

        try
        {
            if (!Guid.TryParse(userContext.Id, out var userId))
            {
                logger.LogWarning("Unable to parse user ID from UserContext for EmployeeListQuery");
                return Result<IEnumerable<Employee>>.Fail("User identification failed", StatusCodes.Status401Unauthorized);
            }

            // Use EmployeeVisibilityService to get only employees the user can see
            var visibleEmployeeReadModels = await visibilityService.GetVisibleEmployeesAsync(userId, cancellationToken);

            // Apply additional filters from the query
            var filteredEmployees = visibleEmployeeReadModels.AsEnumerable();

            if (!query.IncludeDeleted)
            {
                filteredEmployees = filteredEmployees.Where(e => !e.IsDeleted);
            }

            if (query.OrganizationNumber.HasValue)
            {
                filteredEmployees = filteredEmployees.Where(e => e.OrganizationNumber == query.OrganizationNumber.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                filteredEmployees = filteredEmployees.Where(e => e.Role == query.Role);
            }

            if (query.ManagerId.HasValue)
            {
                var managerIdStr = query.ManagerId.Value.ToString();
                filteredEmployees = filteredEmployees.Where(e => e.ManagerId == managerIdStr);
            }

            // Collect all unique manager IDs for batch lookup
            var managerIds = filteredEmployees
                .Where(e => !string.IsNullOrEmpty(e.ManagerId))
                .Select(e => Guid.Parse(e.ManagerId!))
                .Distinct()
                .ToList();

            // Batch fetch manager names
            var managerLookup = new Dictionary<Guid, string>();
            if (managerIds.Count > 0)
            {
                var managers = await repository.GetEmployeesAsync(cancellationToken: cancellationToken);
                managerLookup = managers
                    .Where(m => managerIds.Contains(m.Id))
                    .ToDictionary(m => m.Id, m => $"{m.FirstName} {m.LastName}");
            }

            // Collect all unique organization numbers for batch lookup
            var organizationNumbers = filteredEmployees
                .Select(e => e.OrganizationNumber)
                .Distinct()
                .ToList();

            // Batch fetch organization names
            var organizationLookup = new Dictionary<int, string>();
            if (organizationNumbers.Any())
            {
                var organizations = await organizationRepository.GetAllOrganizationsAsync(includeDeleted: false, includeIgnored: false, cancellationToken);
                organizationLookup = organizations
                    .Where(o => !string.IsNullOrEmpty(o.Number) && int.TryParse(o.Number, out _))
                    .ToDictionary(o => int.Parse(o.Number), o => o.Name);
            }

            // Map to Employee query model
            var employees = filteredEmployees.Select(e => new Employee
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Role = e.Role,
                EMail = e.EMail,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                LastStartDate = e.LastStartDate,
                ManagerId = string.IsNullOrEmpty(e.ManagerId) ? null : Guid.Parse(e.ManagerId),
                Manager = string.IsNullOrEmpty(e.ManagerId) ? string.Empty :
                    (managerLookup.TryGetValue(Guid.Parse(e.ManagerId), out var managerName) ? managerName : string.Empty),
                LoginName = e.LoginName,
                EmployeeNumber = e.EmployeeId, // Using EmployeeId as EmployeeNumber
                OrganizationNumber = e.OrganizationNumber,
                Organization = organizationLookup.TryGetValue(e.OrganizationNumber, out var orgName) ? orgName : string.Empty,
                IsDeleted = e.IsDeleted,
                ApplicationRole = e.ApplicationRole
            }).ToList();

            logger.LogEmployeeListQuerySucceeded(employees.Count);
            return Result<IEnumerable<Employee>>.Success(employees);
        }
        catch (Exception ex)
        {
            logger.LogEmployeeListQueryFailed(ex);
            return Result<IEnumerable<Employee>>.Fail($"Failed to retrieve employees: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<Employee?>> HandleAsync(EmployeeQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogEmployeeQueryStarting(query.EmployeeId);

        try
        {
            if (!Guid.TryParse(userContext.Id, out var userId))
            {
                logger.LogWarning("Unable to parse user ID from UserContext for EmployeeQuery");
                return Result<Employee?>.Fail("User identification failed", StatusCodes.Status401Unauthorized);
            }

            // Check if user can view this specific employee
            if (!await visibilityService.CanViewEmployeeAsync(userId, query.EmployeeId, cancellationToken))
            {
                logger.LogWarning("User not authorized to view employee {EmployeeId}", query.EmployeeId);
                return Result<Employee?>.Fail("Forbidden", StatusCodes.Status403Forbidden);
            }

            var employeeReadModel = await repository.GetEmployeeByIdAsync(query.EmployeeId, cancellationToken);

            if (employeeReadModel != null)
            {
                // Resolve manager name if manager ID exists
                string managerName = string.Empty;
                if (!string.IsNullOrEmpty(employeeReadModel.ManagerId))
                {
                    var managerId = Guid.Parse(employeeReadModel.ManagerId);
                    var manager = await repository.GetEmployeeByIdAsync(managerId, cancellationToken);
                    if (manager != null)
                    {
                        managerName = $"{manager.FirstName} {manager.LastName}";
                    }
                }

                // Resolve organization name
                string organizationName = string.Empty;
                var organizations = await organizationRepository.GetAllOrganizationsAsync(includeDeleted: false, includeIgnored: false, cancellationToken);
                var organization = organizations.FirstOrDefault(o =>
                    !string.IsNullOrEmpty(o.Number) &&
                    int.TryParse(o.Number, out var orgNum) &&
                    orgNum == employeeReadModel.OrganizationNumber);

                if (organization != null)
                {
                    organizationName = organization.Name;
                }

                var employee = new Employee
                {
                    Id = employeeReadModel.Id,
                    FirstName = employeeReadModel.FirstName,
                    LastName = employeeReadModel.LastName,
                    Role = employeeReadModel.Role,
                    EMail = employeeReadModel.EMail,
                    StartDate = employeeReadModel.StartDate,
                    EndDate = employeeReadModel.EndDate,
                    LastStartDate = employeeReadModel.LastStartDate,
                    ManagerId = string.IsNullOrEmpty(employeeReadModel.ManagerId) ? null : Guid.Parse(employeeReadModel.ManagerId),
                    Manager = managerName,
                    LoginName = employeeReadModel.LoginName,
                    EmployeeNumber = employeeReadModel.EmployeeId, // Using EmployeeId as EmployeeNumber
                    OrganizationNumber = employeeReadModel.OrganizationNumber,
                    Organization = organizationName,
                    IsDeleted = employeeReadModel.IsDeleted,
                    ApplicationRole = employeeReadModel.ApplicationRole
                };

                logger.LogEmployeeQuerySucceeded(query.EmployeeId);
                return Result<Employee?>.Success(employee);
            }

            logger.LogEmployeeNotFound(query.EmployeeId);
            return Result<Employee?>.Success(null);
        }
        catch (Exception ex)
        {
            logger.LogEmployeeQueryFailed(query.EmployeeId, ex);
            return Result<Employee?>.Fail($"Failed to retrieve employee: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}