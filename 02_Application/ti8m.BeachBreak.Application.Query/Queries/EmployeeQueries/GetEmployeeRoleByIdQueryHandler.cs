using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Mappers;
using ti8m.BeachBreak.Application.Query.Models;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

/// <summary>
/// Lightweight handler for authorization middleware - only queries employee role by ID
/// </summary>
public class GetEmployeeRoleByIdQueryHandler : IQueryHandler<GetEmployeeRoleByIdQuery, EmployeeRoleResult?>
{
    private readonly IEmployeeRepository repository;
    private readonly ILogger<GetEmployeeRoleByIdQueryHandler> logger;

    public GetEmployeeRoleByIdQueryHandler(
        IEmployeeRepository repository,
        ILogger<GetEmployeeRoleByIdQueryHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<EmployeeRoleResult?> HandleAsync(GetEmployeeRoleByIdQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Getting employee role for user ID: {UserId}", query.UserId);

        try
        {
            var employeeReadModel = await repository.GetEmployeeByIdAsync(query.UserId, cancellationToken);

            if (employeeReadModel == null || employeeReadModel.IsDeleted)
            {
                logger.LogWarning("Employee not found or deleted for user ID: {UserId}", query.UserId);
                return null;
            }

            return new EmployeeRoleResult(employeeReadModel.Id, employeeReadModel.ApplicationRole);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve employee role for user ID: {UserId}", query.UserId);
            return null;
        }
    }
}
