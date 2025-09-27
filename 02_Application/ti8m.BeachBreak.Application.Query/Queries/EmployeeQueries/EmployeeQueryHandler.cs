using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeQueryHandler :
    IQueryHandler<EmployeeListQuery, Result<IEnumerable<Employee>>>,
    IQueryHandler<EmployeeQuery, Result<Employee?>>
{
    private readonly IEmployeeRepository repository;
    private readonly ILogger<EmployeeQueryHandler> logger;

    public EmployeeQueryHandler(IEmployeeRepository repository, ILogger<EmployeeQueryHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<Employee>>> HandleAsync(EmployeeListQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogEmployeeListQueryStarting(query.IncludeDeleted, query.OrganizationNumber, query.Role, query.ManagerId);

        try
        {
            var employeeReadModels = await repository.GetEmployeesAsync(
                query.IncludeDeleted,
                query.OrganizationNumber,
                query.Role,
                query.ManagerId,
                cancellationToken);

            var employees = employeeReadModels.Select(e => new Employee
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