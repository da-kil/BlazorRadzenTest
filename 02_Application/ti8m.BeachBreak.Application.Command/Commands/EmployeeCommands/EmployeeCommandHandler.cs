using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.EmployeeCommands;

public class EmployeeCommandHandler :
    ICommandHandler<BulkInsertEmployeesCommand, Result>,
    ICommandHandler<BulkUpdateEmployeesCommand, Result>,
    ICommandHandler<BulkDeleteEmployeesCommand, Result>,
    ICommandHandler<SaveEmployeeResponseCommand, Result<Guid>>,
    ICommandHandler<SubmitEmployeeResponseCommand, Result>
{
    private static readonly Guid namespaceGuid = new("BF24D00E-4E5E-4B4D-AFC2-798860B2DA73");
    private readonly IEmployeeAggregateRepository repository;
    private readonly ILogger<EmployeeCommandHandler> logger;

    public EmployeeCommandHandler(IEmployeeAggregateRepository repository, ILogger<EmployeeCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(BulkInsertEmployeesCommand command, CancellationToken cancellationToken = default)
    {
        int countInserted = 0;
        int countUpdated = 0;
        int countUndeleted = 0;
        int countDeleted = 0;
        foreach (var e in command.Employees)
        {
            Guid employeeId = Deterministic.Create(namespaceGuid, e.EmployeeId);
            var employee = await repository.LoadAsync<Employee>(employeeId, cancellationToken: cancellationToken);

            if (employee is not null && employee.IsDeleted)
            {
                logger.LogInformation("Undeleting employee {EmployeeId}", employee.EmployeeId);

                employee.Undelete();
                UpdateEmployee(e, employee);

                if (employee.UncommittedEvents.Any())
                {
                    await repository.StoreAsync(employee, cancellationToken);
                    countUndeleted++;
                }
            }
            else if (employee is not null)
            {
                logger.LogInformation("Update employee {EmployeeId}", employee.EmployeeId);

                UpdateEmployee(e, employee);

                if (employee.UncommittedEvents.Any())
                {
                    await repository.StoreAsync(employee, cancellationToken);
                    countUpdated++;
                }
            }
            else
            {
                logger.LogInformation("Insert employee {EmployeeId}", e.EmployeeId);

                var startDate = ParseStartDate(e.StartDate);
                var lastStartDate = ParseLastStartDate(e.LastStartDate);
                var endDate = ParseEndDate(e.EndDate);

                employee = new Employee(
                    e.Id,
                    e.EmployeeId,
                    e.FirstName,
                    e.LastName,
                    e.Role,
                    e.EMail,
                    startDate,
                    endDate,
                    lastStartDate,
                    e.ManagerId,
                    e.LoginName,
                    int.Parse(e.OrganizationNumber));

                await repository.StoreAsync(employee, cancellationToken);
                countInserted++;
            }
        }

        var ids = command.Employees.Select(o => Deterministic.Create(namespaceGuid, o.EmployeeId)).ToArray();
        var employeesToDelete = await repository.FindEntriesToDeleteAsync<Employee>(ids, cancellationToken: cancellationToken);

        if (employeesToDelete is not null)
        {
            foreach (var employee in employeesToDelete)
            {
                employee.Delete();
                await repository.StoreAsync(employee, cancellationToken);
                countDeleted++;
            }
        }

        logger.LogInformation("Employee bulk import inserted: {CountInserted}, undeleted: {CountUndeleted}, updated: {countUpdated}, deleted: {CountDeleted}", countInserted, countUndeleted, countUpdated, countDeleted);
        return Result.Success();
    }

    public async Task<Result> HandleAsync(BulkUpdateEmployeesCommand command, CancellationToken cancellationToken = default)
    {
        foreach (var e in command.Employees)
        {
            var employee = await repository.LoadRequiredAsync<Employee>(e.Id, cancellationToken: cancellationToken);
            UpdateEmployee(e, employee);
            await repository.StoreAsync(employee, cancellationToken);
        }
        return Result.Success();
    }

    public async Task<Result> HandleAsync(BulkDeleteEmployeesCommand command, CancellationToken cancellationToken = default)
    {
        foreach (var e in command.EmployeeIds)
        {
            var employee = await repository.LoadRequiredAsync<Employee>(e, cancellationToken: cancellationToken);
            employee.Delete();
            await repository.StoreAsync(employee, cancellationToken);
        }
        return Result.Success();
    }

    public async Task<Result<Guid>> HandleAsync(SaveEmployeeResponseCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting save employee response for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", command.EmployeeId, command.AssignmentId);

        try
        {
            return null;
            //await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            //await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            //try
            //{
            //    await using var cmd = connection.CreateCommand();
            //    cmd.Transaction = transaction;

            //    cmd.CommandText = """
            //        WITH upsert AS (
            //            UPDATE questionnaire_responses
            //            SET section_responses = @sectionResponses,
            //                last_modified = @lastModified,
            //                status = @status
            //            WHERE assignment_id = @assignmentId AND employee_id = @employeeId
            //            RETURNING id
            //        ),
            //        insert_attempt AS (
            //            INSERT INTO questionnaire_responses (
            //                id, assignment_id, template_id, employee_id, status, section_responses, last_modified, created_at
            //            )
            //            SELECT @id, @assignmentId, @templateId, @employeeId, @status, @sectionResponses, @lastModified, @lastModified
            //            WHERE NOT EXISTS (SELECT 1 FROM questionnaire_responses WHERE assignment_id = @assignmentId AND employee_id = @employeeId)
            //            RETURNING id
            //        )
            //        SELECT id FROM upsert
            //        UNION ALL
            //        SELECT id FROM insert_attempt
            //        """;

            //    var responseId = Guid.NewGuid();
            //    var sectionResponsesJson = System.Text.Json.JsonSerializer.Serialize(command.SectionResponses);

            //    cmd.Parameters.AddWithValue("@id", responseId);
            //    cmd.Parameters.AddWithValue("@assignmentId", command.AssignmentId);
            //    cmd.Parameters.AddWithValue("@templateId", command.TemplateId);
            //    cmd.Parameters.AddWithValue("@employeeId", command.EmployeeId);
            //    cmd.Parameters.AddWithValue("@status", command.Status.ToString());
            //    cmd.Parameters.Add("@sectionResponses", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = sectionResponsesJson;
            //    cmd.Parameters.AddWithValue("@lastModified", DateTime.UtcNow);

            //    var result = await cmd.ExecuteScalarAsync(cancellationToken);
            //    var returnedId = (Guid)result!;

            //    await transaction.CommitAsync(cancellationToken);
            //    logger.LogInformation("Employee response saved successfully with Id: {ResponseId}", returnedId);

            //    return Result<Guid>.Success(returnedId);
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, "Error occurred during save employee response transaction, rolling back");
            //    await transaction.RollbackAsync(cancellationToken);
            //    throw;
            //}
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
            return null;
            //await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            //await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            //try
            //{
            //    await using var updateResponseCmd = connection.CreateCommand();
            //    updateResponseCmd.Transaction = transaction;
            //    updateResponseCmd.CommandText = """
            //        UPDATE questionnaire_responses
            //        SET status = 'Submitted',
            //            submitted_date = @submittedDate,
            //            last_modified = @lastModified
            //        WHERE assignment_id = @assignmentId AND employee_id = @employeeId
            //        """;

            //    updateResponseCmd.Parameters.AddWithValue("@submittedDate", DateTime.UtcNow);
            //    updateResponseCmd.Parameters.AddWithValue("@lastModified", DateTime.UtcNow);
            //    updateResponseCmd.Parameters.AddWithValue("@assignmentId", command.AssignmentId);
            //    updateResponseCmd.Parameters.AddWithValue("@employeeId", command.EmployeeId);

            //    var responseRows = await updateResponseCmd.ExecuteNonQueryAsync(cancellationToken);

            //    if (responseRows == 0)
            //    {
            //        await transaction.RollbackAsync(cancellationToken);
            //        return Result.Fail("No response found to submit", StatusCodes.Status404NotFound);
            //    }

            //    await using var updateAssignmentCmd = connection.CreateCommand();
            //    updateAssignmentCmd.Transaction = transaction;
            //    updateAssignmentCmd.CommandText = """
            //        UPDATE questionnaire_assignments
            //        SET status = 'Completed',
            //            completed_date = @completedDate
            //        WHERE id = @assignmentId
            //        """;

            //    updateAssignmentCmd.Parameters.AddWithValue("@completedDate", DateTime.UtcNow);
            //    updateAssignmentCmd.Parameters.AddWithValue("@assignmentId", command.AssignmentId);

            //    await updateAssignmentCmd.ExecuteNonQueryAsync(cancellationToken);

            //    await transaction.CommitAsync(cancellationToken);
            //    logger.LogInformation("Employee response submitted successfully for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", command.EmployeeId, command.AssignmentId);

            //    return Result.Success("Response submitted successfully");
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, "Error occurred during submit employee response transaction, rolling back");
            //    await transaction.RollbackAsync(cancellationToken);
            //    throw;
            //}
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to submit employee response for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", command.EmployeeId, command.AssignmentId);
            return Result.Fail($"Failed to submit employee response: {ex.Message}", StatusCodes.Status500InternalServerError);
        }
    }

    private static void UpdateEmployee(SyncEmployee e, Employee employee)
    {
        // Update only the properties that exist in both command Employee and domain Employee aggregate
        employee.ChangeName(e.FirstName, e.LastName);
        employee.ChangeEmail(e.EMail);
        employee.ChangeRole(e.Role);
        employee.ChangeManager(e.ManagerId);
        employee.ChangeLoginName(e.LoginName);
        employee.ChangeDepartment(int.Parse(e.OrganizationNumber));
        employee.ChangeStartDate(ParseStartDate(e.StartDate));
        employee.ChangeEndDate(ParseEndDate(e.EndDate));
    }

    private static DateOnly ParseStartDate(string syncStartDate)
    {
        _ = DateOnly.TryParseExact(syncStartDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate);
        return startDate;
    }

    private static DateOnly ParseLastStartDate(string syncLastStartDate)
    {
        _ = DateOnly.TryParseExact(syncLastStartDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var lastStartDate);
        return lastStartDate;
    }

    private static DateOnly? ParseEndDate(string? syncEndDate)
    {
        bool hasDateOut = DateOnly.TryParseExact(syncEndDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate);
        return hasDateOut ? endDate : null;
    }
}