using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class GetTeamMembersQueryHandler : IQueryHandler<GetTeamMembersQuery, Result<IEnumerable<Employee>>>
{
    private readonly IEmployeeRepository repository;
    private readonly ILogger<GetTeamMembersQueryHandler> logger;

    public GetTeamMembersQueryHandler(
        IEmployeeRepository repository,
        ILogger<GetTeamMembersQueryHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<Employee>>> HandleAsync(GetTeamMembersQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting team members for manager {ManagerId}", query.ManagerId);

        try
        {
            var managerIdStr = query.ManagerId.ToString();
            var employeeReadModels = await repository.GetEmployeesByManagerIdAsync(managerIdStr, cancellationToken);

            // Filter out deleted employees
            var activeEmployees = employeeReadModels.Where(e => !e.IsDeleted);

            // Map to Employee query model
            var employees = activeEmployees.Select(e => new Employee
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
                Manager = e.ManagerId ?? string.Empty,
                LoginName = e.LoginName,
                EmployeeNumber = e.EmployeeId,
                OrganizationNumber = e.OrganizationNumber,
                Organization = string.Empty,
                IsDeleted = e.IsDeleted,
                ApplicationRole = e.ApplicationRole
            }).ToList();

            logger.LogInformation("Retrieved {Count} team members for manager {ManagerId}", employees.Count, query.ManagerId);
            return Result<IEnumerable<Employee>>.Success(employees);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve team members for manager {ManagerId}", query.ManagerId);
            return Result<IEnumerable<Employee>>.Fail($"Failed to retrieve team members: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}
