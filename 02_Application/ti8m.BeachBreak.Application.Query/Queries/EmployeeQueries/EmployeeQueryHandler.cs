using Npgsql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;

public class EmployeeQueryHandler :
    IQueryHandler<EmployeeListQuery, Result<IEnumerable<Employee>>>,
    IQueryHandler<EmployeeQuery, Result<Employee?>>
{
    private readonly NpgsqlDataSource dataSource;
    private readonly ILogger<EmployeeQueryHandler> logger;

    public EmployeeQueryHandler(NpgsqlDataSource dataSource, ILogger<EmployeeQueryHandler> logger)
    {
        this.dataSource = dataSource;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<Employee>>> HandleAsync(EmployeeListQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting employee list query with filters - IncludeDeleted: {IncludeDeleted}, OrganizationNumber: {OrganizationNumber}, Role: {Role}, ManagerId: {ManagerId}",
            query.IncludeDeleted, query.OrganizationNumber, query.Role, query.ManagerId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            logger.LogDebug("Database connection established for employee list query");
            await using var cmd = connection.CreateCommand();

            var sql = new StringBuilder();
            sql.AppendLine("SELECT id, first_name, last_name, role, email, start_date, end_date,");
            sql.AppendLine("       last_start_date, manager_id, manager, login_name, employee_number,");
            sql.AppendLine("       organization_number, organization, is_deleted");
            sql.AppendLine("FROM employees");
            sql.AppendLine("WHERE 1=1");

            var parameters = new List<NpgsqlParameter>();

            if (!query.IncludeDeleted)
            {
                sql.AppendLine("AND is_deleted = false");
            }

            if (query.OrganizationNumber.HasValue)
            {
                sql.AppendLine("AND organization_number = @organization_number");
                parameters.Add(new NpgsqlParameter("@organization_number", query.OrganizationNumber.Value));
            }

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                sql.AppendLine("AND role = @role");
                parameters.Add(new NpgsqlParameter("@role", query.Role));
            }

            if (query.ManagerId.HasValue)
            {
                sql.AppendLine("AND manager_id = @manager_id");
                parameters.Add(new NpgsqlParameter("@manager_id", query.ManagerId.Value));
            }

            sql.AppendLine("ORDER BY last_name, first_name");

            cmd.CommandText = sql.ToString();
            if (parameters.Any())
            {
                cmd.Parameters.AddRange(parameters.ToArray());
            }

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            var employees = new List<Employee>();

            while (await reader.ReadAsync(cancellationToken))
            {
                employees.Add(new Employee
                {
                    Id = reader.GetGuid(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Role = reader.GetString(3),
                    EMail = reader.GetString(4),
                    StartDate = DateOnly.FromDateTime(reader.GetDateTime(5)),
                    EndDate = reader.IsDBNull(6) ? null : DateOnly.FromDateTime(reader.GetDateTime(6)),
                    LastStartDate = reader.IsDBNull(7) ? null : DateOnly.FromDateTime(reader.GetDateTime(7)),
                    ManagerId = reader.IsDBNull(8) ? null : reader.GetGuid(8),
                    Manager = reader.GetString(9),
                    LoginName = reader.GetString(10),
                    EmployeeNumber = reader.GetString(11),
                    OrganizationNumber = reader.GetInt32(12),
                    Organization = reader.GetString(13),
                    IsDeleted = reader.GetBoolean(14)
                });
            }

            logger.LogInformation("Employee list query completed successfully, returned {EmployeeCount} employees", employees.Count);
            return Result<IEnumerable<Employee>>.Success(employees);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute employee list query");
            return Result<IEnumerable<Employee>>.Fail($"Failed to retrieve employees: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<Employee?>> HandleAsync(EmployeeQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting single employee query for EmployeeId: {EmployeeId}", query.EmployeeId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            logger.LogDebug("Database connection established for single employee query");
            await using var cmd = connection.CreateCommand();

            cmd.CommandText = """
                SELECT id, first_name, last_name, role, email, start_date, end_date,
                       last_start_date, manager_id, manager, login_name, employee_number,
                       organization_number, organization, is_deleted
                FROM employees
                WHERE id = @id
                """;

            cmd.Parameters.AddWithValue("@id", query.EmployeeId);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                var employee = new Employee
                {
                    Id = reader.GetGuid(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Role = reader.GetString(3),
                    EMail = reader.GetString(4),
                    StartDate = DateOnly.FromDateTime(reader.GetDateTime(5)),
                    EndDate = reader.IsDBNull(6) ? null : DateOnly.FromDateTime(reader.GetDateTime(6)),
                    LastStartDate = reader.IsDBNull(7) ? null : DateOnly.FromDateTime(reader.GetDateTime(7)),
                    ManagerId = reader.IsDBNull(8) ? null : reader.GetGuid(8),
                    Manager = reader.GetString(9),
                    LoginName = reader.GetString(10),
                    EmployeeNumber = reader.GetString(11),
                    OrganizationNumber = reader.GetInt32(12),
                    Organization = reader.GetString(13),
                    IsDeleted = reader.GetBoolean(14)
                };

                logger.LogInformation("Single employee query completed successfully for EmployeeId: {EmployeeId}", query.EmployeeId);
                return Result<Employee?>.Success(employee);
            }

            logger.LogInformation("Employee not found for EmployeeId: {EmployeeId}", query.EmployeeId);
            return Result<Employee?>.Success(null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute single employee query for EmployeeId: {EmployeeId}", query.EmployeeId);
            return Result<Employee?>.Fail($"Failed to retrieve employee: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}