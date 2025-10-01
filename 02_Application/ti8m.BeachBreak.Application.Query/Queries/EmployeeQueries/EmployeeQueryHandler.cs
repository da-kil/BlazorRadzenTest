using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Application.Query.Services;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeQueryHandler :
    IQueryHandler<EmployeeListQuery, Result<IEnumerable<Employee>>>,
    IQueryHandler<EmployeeQuery, Result<Employee?>>
{
    private readonly IEmployeeRepository repository;
    private readonly ILogger<EmployeeQueryHandler> logger;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly EmployeeVisibilityService visibilityService;

    public EmployeeQueryHandler(
        IEmployeeRepository repository,
        ILogger<EmployeeQueryHandler> logger,
        IHttpContextAccessor httpContextAccessor,
        EmployeeVisibilityService visibilityService)
    {
        this.repository = repository;
        this.logger = logger;
        this.httpContextAccessor = httpContextAccessor;
        this.visibilityService = visibilityService;
    }

    public async Task<Result<IEnumerable<Employee>>> HandleAsync(EmployeeListQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogEmployeeListQueryStarting(query.IncludeDeleted, query.OrganizationNumber, query.Role, query.ManagerId);

        try
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                logger.LogWarning("No user context available for EmployeeListQuery");
                return Result<IEnumerable<Employee>>.Fail("Unauthorized", StatusCodes.Status401Unauthorized);
            }

            // Use EmployeeVisibilityService to get only employees the user can see
            var visibleEmployeeReadModels = await visibilityService.GetVisibleEmployeesAsync(user, cancellationToken);

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
                Manager = e.ManagerId ?? string.Empty, // TODO: Look up manager name
                LoginName = e.LoginName,
                EmployeeNumber = e.EmployeeId, // Using EmployeeId as EmployeeNumber
                OrganizationNumber = e.OrganizationNumber,
                Organization = string.Empty, // TODO: Look up organization name
                IsDeleted = e.IsDeleted
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
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                logger.LogWarning("No user context available for EmployeeQuery");
                return Result<Employee?>.Fail("Unauthorized", StatusCodes.Status401Unauthorized);
            }

            // Check if user can view this specific employee
            if (!await visibilityService.CanViewEmployeeAsync(user, query.EmployeeId, cancellationToken))
            {
                logger.LogWarning("User not authorized to view employee {EmployeeId}", query.EmployeeId);
                return Result<Employee?>.Fail("Forbidden", StatusCodes.Status403Forbidden);
            }

            var employeeReadModel = await repository.GetEmployeeByIdAsync(query.EmployeeId, cancellationToken);

            if (employeeReadModel != null)
            {
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
                    Manager = employeeReadModel.ManagerId ?? string.Empty, // TODO: Look up manager name
                    LoginName = employeeReadModel.LoginName,
                    EmployeeNumber = employeeReadModel.EmployeeId, // Using EmployeeId as EmployeeNumber
                    OrganizationNumber = employeeReadModel.OrganizationNumber,
                    Organization = string.Empty, // TODO: Look up organization name
                    IsDeleted = employeeReadModel.IsDeleted
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