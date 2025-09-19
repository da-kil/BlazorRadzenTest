using Npgsql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

public class EmployeeCommandHandler :
    ICommandHandler<BulkInsertEmployeesCommand, Result>,
    ICommandHandler<BulkUpdateEmployeesCommand, Result>,
    ICommandHandler<BulkDeleteEmployeesCommand, Result>,
    ICommandHandler<SaveEmployeeResponseCommand, Result<Guid>>,
    ICommandHandler<SubmitEmployeeResponseCommand, Result>
{
    private readonly NpgsqlDataSource dataSource;
    private readonly ILogger<EmployeeCommandHandler> logger;

    public EmployeeCommandHandler(NpgsqlDataSource dataSource, ILogger<EmployeeCommandHandler> logger)
    {
        this.dataSource = dataSource;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(BulkInsertEmployeesCommand command, CancellationToken cancellationToken = default)
    {
        var employeeCount = command.Employees.Count();
        logger.LogInformation("Starting bulk insert operation for {EmployeeCount} employees", employeeCount);

        try
        {
            if (!command.Employees.Any())
            {
                logger.LogWarning("Bulk insert attempted with no employees provided");
                return Result.Fail("No employees provided for bulk insert", StatusCodes.Status400BadRequest);
            }

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            logger.LogDebug("Database connection established for bulk insert");

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            logger.LogDebug("Database transaction started for bulk insert");

            try
            {
                var sql = new StringBuilder();
                sql.AppendLine("INSERT INTO employees (");
                sql.AppendLine("    id, first_name, last_name, role, email, start_date, end_date,");
                sql.AppendLine("    last_start_date, manager_id, manager, login_name, employee_number,");
                sql.AppendLine("    organization_number, organization, is_deleted");
                sql.AppendLine(") VALUES");

                var parameters = new List<NpgsqlParameter>();
                var valuesClauses = new List<string>();
                var paramIndex = 0;

                foreach (var employee in command.Employees)
                {
                    valuesClauses.Add($"(@id{paramIndex}, @first_name{paramIndex}, @last_name{paramIndex}, @role{paramIndex}, @email{paramIndex}, @start_date{paramIndex}, @end_date{paramIndex}, @last_start_date{paramIndex}, @manager_id{paramIndex}, @manager{paramIndex}, @login_name{paramIndex}, @employee_number{paramIndex}, @organization_number{paramIndex}, @organization{paramIndex}, @is_deleted{paramIndex})");

                    parameters.Add(new NpgsqlParameter($"@id{paramIndex}", employee.Id));
                    parameters.Add(new NpgsqlParameter($"@first_name{paramIndex}", employee.FirstName));
                    parameters.Add(new NpgsqlParameter($"@last_name{paramIndex}", employee.LastName));
                    parameters.Add(new NpgsqlParameter($"@role{paramIndex}", employee.Role));
                    parameters.Add(new NpgsqlParameter($"@email{paramIndex}", employee.EMail));
                    parameters.Add(new NpgsqlParameter($"@start_date{paramIndex}", employee.StartDate));
                    parameters.Add(new NpgsqlParameter($"@end_date{paramIndex}", (object?)employee.EndDate ?? DBNull.Value));
                    parameters.Add(new NpgsqlParameter($"@last_start_date{paramIndex}", (object?)employee.LastStartDate ?? DBNull.Value));
                    parameters.Add(new NpgsqlParameter($"@manager_id{paramIndex}", (object?)employee.ManagerId ?? DBNull.Value));
                    parameters.Add(new NpgsqlParameter($"@manager{paramIndex}", employee.Manager));
                    parameters.Add(new NpgsqlParameter($"@login_name{paramIndex}", employee.LoginName));
                    parameters.Add(new NpgsqlParameter($"@employee_number{paramIndex}", employee.EmployeeNumber));
                    parameters.Add(new NpgsqlParameter($"@organization_number{paramIndex}", employee.OrganizationNumber));
                    parameters.Add(new NpgsqlParameter($"@organization{paramIndex}", employee.Organization));
                    parameters.Add(new NpgsqlParameter($"@is_deleted{paramIndex}", employee.IsDeleted));

                    paramIndex++;
                }

                sql.AppendLine(string.Join(",\n", valuesClauses));

                await using var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddRange(parameters.ToArray());

                var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                logger.LogDebug("Bulk insert SQL executed successfully, {RowsAffected} rows affected", rowsAffected);

                await transaction.CommitAsync(cancellationToken);
                logger.LogInformation("Bulk insert completed successfully for {EmployeeCount} employees, {RowsAffected} rows inserted", employeeCount, rowsAffected);

                return Result.Success($"Successfully inserted {rowsAffected} employees");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during bulk insert transaction, rolling back");
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to bulk insert {EmployeeCount} employees", employeeCount);
            return Result.Fail($"Failed to bulk insert employees: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(BulkUpdateEmployeesCommand command, CancellationToken cancellationToken = default)
    {
        var employeeCount = command.Employees.Count();
        logger.LogInformation("Starting bulk update operation for {EmployeeCount} employees", employeeCount);

        try
        {
            if (!command.Employees.Any())
            {
                logger.LogWarning("Bulk update attempted with no employees provided");
                return Result.Fail("No employees provided for bulk update", StatusCodes.Status400BadRequest);
            }

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            logger.LogDebug("Database connection established for bulk update");

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            logger.LogDebug("Database transaction started for bulk update");

            try
            {
                var totalRowsAffected = 0;

                foreach (var employee in command.Employees)
                {
                    await using var cmd = connection.CreateCommand();
                    cmd.Transaction = transaction;
                    cmd.CommandText = """
                        UPDATE employees SET
                            first_name = @first_name,
                            last_name = @last_name,
                            role = @role,
                            email = @email,
                            start_date = @start_date,
                            end_date = @end_date,
                            last_start_date = @last_start_date,
                            manager_id = @manager_id,
                            manager = @manager,
                            login_name = @login_name,
                            employee_number = @employee_number,
                            organization_number = @organization_number,
                            organization = @organization,
                            is_deleted = @is_deleted
                        WHERE id = @id
                        """;

                    cmd.Parameters.AddWithValue("@id", employee.Id);
                    cmd.Parameters.AddWithValue("@first_name", employee.FirstName);
                    cmd.Parameters.AddWithValue("@last_name", employee.LastName);
                    cmd.Parameters.AddWithValue("@role", employee.Role);
                    cmd.Parameters.AddWithValue("@email", employee.EMail);
                    cmd.Parameters.AddWithValue("@start_date", employee.StartDate);
                    cmd.Parameters.AddWithValue("@end_date", (object?)employee.EndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@last_start_date", (object?)employee.LastStartDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@manager_id", (object?)employee.ManagerId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@manager", employee.Manager);
                    cmd.Parameters.AddWithValue("@login_name", employee.LoginName);
                    cmd.Parameters.AddWithValue("@employee_number", employee.EmployeeNumber);
                    cmd.Parameters.AddWithValue("@organization_number", employee.OrganizationNumber);
                    cmd.Parameters.AddWithValue("@organization", employee.Organization);
                    cmd.Parameters.AddWithValue("@is_deleted", employee.IsDeleted);

                    var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                    totalRowsAffected += rowsAffected;
                }

                await transaction.CommitAsync(cancellationToken);
                logger.LogInformation("Bulk update completed successfully for {EmployeeCount} employees, {TotalRowsAffected} rows updated", employeeCount, totalRowsAffected);

                return Result.Success($"Successfully updated {totalRowsAffected} employees");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during bulk update transaction, rolling back");
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to bulk update {EmployeeCount} employees", employeeCount);
            return Result.Fail($"Failed to bulk update employees: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(BulkDeleteEmployeesCommand command, CancellationToken cancellationToken = default)
    {
        var employeeIdCount = command.EmployeeIds.Count();
        logger.LogInformation("Starting bulk delete operation for {EmployeeIdCount} employee IDs", employeeIdCount);

        try
        {
            if (!command.EmployeeIds.Any())
            {
                logger.LogWarning("Bulk delete attempted with no employee IDs provided");
                return Result.Fail("No employee IDs provided for bulk delete", StatusCodes.Status400BadRequest);
            }

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            logger.LogDebug("Database connection established for bulk delete");

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            logger.LogDebug("Database transaction started for bulk delete");

            try
            {
                var sql = new StringBuilder();
                sql.AppendLine("DELETE FROM employees WHERE id IN (");

                var parameters = new List<NpgsqlParameter>();
                var paramPlaceholders = new List<string>();
                var paramIndex = 0;

                foreach (var employeeId in command.EmployeeIds)
                {
                    paramPlaceholders.Add($"@id{paramIndex}");
                    parameters.Add(new NpgsqlParameter($"@id{paramIndex}", employeeId));
                    paramIndex++;
                }

                sql.AppendLine(string.Join(", ", paramPlaceholders));
                sql.AppendLine(")");

                await using var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddRange(parameters.ToArray());

                var rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                logger.LogDebug("Bulk delete SQL executed successfully, {RowsAffected} rows affected", rowsAffected);

                await transaction.CommitAsync(cancellationToken);
                logger.LogInformation("Bulk delete completed successfully for {EmployeeIdCount} employee IDs, {RowsAffected} rows deleted", employeeIdCount, rowsAffected);

                return Result.Success($"Successfully deleted {rowsAffected} employees");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during bulk delete transaction, rolling back");
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to bulk delete {EmployeeIdCount} employee IDs", employeeIdCount);
            return Result.Fail($"Failed to bulk delete employees: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result<Guid>> HandleAsync(SaveEmployeeResponseCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting save employee response for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", command.EmployeeId, command.AssignmentId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                await using var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;

                cmd.CommandText = """
                    INSERT INTO questionnaire_responses (
                        id, assignment_id, template_id, employee_id, status, section_responses, last_modified
                    ) VALUES (
                        @id, @assignmentId, @templateId, @employeeId, @status, @sectionResponses, @lastModified
                    )
                    ON CONFLICT (assignment_id, employee_id) DO UPDATE SET
                        section_responses = @sectionResponses,
                        last_modified = @lastModified,
                        status = @status
                    RETURNING id
                    """;

                var responseId = Guid.NewGuid();
                var sectionResponsesJson = System.Text.Json.JsonSerializer.Serialize(command.SectionResponses);

                cmd.Parameters.AddWithValue("@id", responseId);
                cmd.Parameters.AddWithValue("@assignmentId", command.AssignmentId);
                cmd.Parameters.AddWithValue("@templateId", command.TemplateId);
                cmd.Parameters.AddWithValue("@employeeId", command.EmployeeId);
                cmd.Parameters.AddWithValue("@status", command.Status.ToString());
                cmd.Parameters.AddWithValue("@sectionResponses", sectionResponsesJson);
                cmd.Parameters.AddWithValue("@lastModified", DateTime.UtcNow);

                var result = await cmd.ExecuteScalarAsync(cancellationToken);
                var returnedId = (Guid)result!;

                await transaction.CommitAsync(cancellationToken);
                logger.LogInformation("Employee response saved successfully with Id: {ResponseId}", returnedId);

                return Result<Guid>.Success(returnedId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during save employee response transaction, rolling back");
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save employee response for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", command.EmployeeId, command.AssignmentId);
            return Result<Guid>.Fail($"Failed to save employee response: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result> HandleAsync(SubmitEmployeeResponseCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting submit employee response for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", command.EmployeeId, command.AssignmentId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                await using var updateResponseCmd = connection.CreateCommand();
                updateResponseCmd.Transaction = transaction;
                updateResponseCmd.CommandText = """
                    UPDATE questionnaire_responses
                    SET status = 'Submitted',
                        submitted_date = @submittedDate,
                        last_modified = @lastModified
                    WHERE assignment_id = @assignmentId AND employee_id = @employeeId
                    """;

                updateResponseCmd.Parameters.AddWithValue("@submittedDate", DateTime.UtcNow);
                updateResponseCmd.Parameters.AddWithValue("@lastModified", DateTime.UtcNow);
                updateResponseCmd.Parameters.AddWithValue("@assignmentId", command.AssignmentId);
                updateResponseCmd.Parameters.AddWithValue("@employeeId", command.EmployeeId);

                var responseRows = await updateResponseCmd.ExecuteNonQueryAsync(cancellationToken);

                if (responseRows == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result.Fail("No response found to submit", StatusCodes.Status404NotFound);
                }

                await using var updateAssignmentCmd = connection.CreateCommand();
                updateAssignmentCmd.Transaction = transaction;
                updateAssignmentCmd.CommandText = """
                    UPDATE questionnaire_assignments
                    SET status = 'Completed',
                        completed_date = @completedDate
                    WHERE id = @assignmentId
                    """;

                updateAssignmentCmd.Parameters.AddWithValue("@completedDate", DateTime.UtcNow);
                updateAssignmentCmd.Parameters.AddWithValue("@assignmentId", command.AssignmentId);

                await updateAssignmentCmd.ExecuteNonQueryAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                logger.LogInformation("Employee response submitted successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", command.EmployeeId, command.AssignmentId);

                return Result.Success("Response submitted successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during submit employee response transaction, rolling back");
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to submit employee response for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", command.EmployeeId, command.AssignmentId);
            return Result.Fail($"Failed to submit employee response: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }
}