using Microsoft.Extensions.Logging;
using Npgsql;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class QuestionnaireAssignmentCommandHandler :
    ICommandHandler<CreateQuestionnaireAssignmentCommand, Result>,
    ICommandHandler<SendAssignmentReminderCommand, Result>,
    ICommandHandler<SendBulkAssignmentReminderCommand, Result>
{
    private readonly ILogger<QuestionnaireAssignmentCommandHandler> logger;
    private readonly NpgsqlDataSource dataSource;

    public QuestionnaireAssignmentCommandHandler(ILogger<QuestionnaireAssignmentCommandHandler> logger, NpgsqlDataSource dataSource)
    {
        this.logger = logger;
        this.dataSource = dataSource;
    }

    public async Task<Result> HandleAsync(CreateQuestionnaireAssignmentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var questionnaireAssignment = command.QuestionnaireAssignment;

            logger.LogInformation("Creating assignments for {EmployeeCount} employees with template {TemplateId}",
                questionnaireAssignment.EmployeeIds.Count,
                questionnaireAssignment.TemplateId);

            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

            var createdAssignments = new List<Guid>();

            foreach (var employeeId in questionnaireAssignment.EmployeeIds)
            {
                // First, get employee details
                var employeeDetails = await GetEmployeeDetailsAsync(connection, employeeId, cancellationToken);

                if (employeeDetails == null)
                {
                    logger.LogWarning("Employee {EmployeeId} not found, skipping assignment creation", employeeId);
                    continue;
                }

                var assignmentId = Guid.NewGuid();

                const string insertSql = """
                    INSERT INTO questionnaire_assignments (
                        id, template_id, employee_id, employee_name, employee_email,
                        assigned_date, due_date, status, assigned_by, notes
                    ) VALUES (
                        @id, @templateId, @employeeId, @employeeName, @employeeEmail,
                        @assignedDate, @dueDate, @status, @assignedBy, @notes
                    )
                    """;

                await using var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = insertSql;

                insertCommand.Parameters.AddWithValue("@id", assignmentId);
                insertCommand.Parameters.AddWithValue("@templateId", questionnaireAssignment.TemplateId);
                insertCommand.Parameters.AddWithValue("@employeeId", Guid.Parse(employeeId));
                insertCommand.Parameters.AddWithValue("@employeeName", employeeDetails.FullName);
                insertCommand.Parameters.AddWithValue("@employeeEmail", employeeDetails.Email);
                insertCommand.Parameters.AddWithValue("@assignedDate", DateTime.UtcNow);
                insertCommand.Parameters.AddWithValue("@dueDate", questionnaireAssignment.DueDate.HasValue ? questionnaireAssignment.DueDate.Value : DBNull.Value);
                insertCommand.Parameters.AddWithValue("@status", "Assigned");
                insertCommand.Parameters.AddWithValue("@assignedBy", questionnaireAssignment.AssignedBy ?? "System");
                insertCommand.Parameters.AddWithValue("@notes", questionnaireAssignment.Notes ?? (object)DBNull.Value);

                await insertCommand.ExecuteNonQueryAsync(cancellationToken);
                createdAssignments.Add(assignmentId);

                logger.LogInformation("Created assignment {AssignmentId} for employee {EmployeeId}", assignmentId, employeeId);
            }

            logger.LogInformation("Successfully created {AssignmentCount} assignments", createdAssignments.Count);
            return Result.Success($"Successfully created {createdAssignments.Count} assignments");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating questionnaire assignments");
            return Result.Fail("Failed to create assignments: " + ex.Message, 500);
        }
    }

    private async Task<EmployeeDetails?> GetEmployeeDetailsAsync(NpgsqlConnection connection, string employeeId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT first_name, last_name, email
            FROM employees
            WHERE id = @employeeId AND is_deleted = false
            """;

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@employeeId", Guid.Parse(employeeId));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new EmployeeDetails
            {
                FullName = $"{reader.GetString(0)} {reader.GetString(1)}",
                Email = reader.GetString(2)
            };
        }

        return null;
    }

    public async Task<Result> HandleAsync(SendAssignmentReminderCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending reminder for AssignmentId: {AssignmentId}", command.AssignmentId);

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                await using var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;

                cmd.CommandText = """
                    INSERT INTO assignment_reminders (
                        id, assignment_id, message, sent_by, sent_date, reminder_type
                    ) VALUES (
                        @id, @assignmentId, @message, @sentBy, @sentDate, @reminderType
                    )
                    """;

                cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                cmd.Parameters.AddWithValue("@assignmentId", command.AssignmentId);
                cmd.Parameters.AddWithValue("@message", command.Message);
                cmd.Parameters.AddWithValue("@sentBy", command.SentBy);
                cmd.Parameters.AddWithValue("@sentDate", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@reminderType", "Individual");

                await cmd.ExecuteNonQueryAsync(cancellationToken);

                await using var updateCmd = connection.CreateCommand();
                updateCmd.Transaction = transaction;
                updateCmd.CommandText = """
                    UPDATE questionnaire_assignments
                    SET last_reminder_date = @lastReminderDate
                    WHERE id = @assignmentId
                    """;

                updateCmd.Parameters.AddWithValue("@lastReminderDate", DateTime.UtcNow);
                updateCmd.Parameters.AddWithValue("@assignmentId", command.AssignmentId);

                await updateCmd.ExecuteNonQueryAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                logger.LogInformation("Reminder sent successfully for AssignmentId: {AssignmentId}", command.AssignmentId);

                return Result.Success("Reminder sent successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during send reminder transaction, rolling back");
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send reminder for AssignmentId: {AssignmentId}", command.AssignmentId);
            return Result.Fail($"Failed to send reminder: {ex.Message}", 500);
        }
    }

    public async Task<Result> HandleAsync(SendBulkAssignmentReminderCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending bulk reminders for {AssignmentCount} assignments", command.AssignmentIds.Count());

        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                var reminderId = Guid.NewGuid();
                var sentDate = DateTime.UtcNow;

                foreach (var assignmentId in command.AssignmentIds)
                {
                    await using var insertCmd = connection.CreateCommand();
                    insertCmd.Transaction = transaction;
                    insertCmd.CommandText = """
                        INSERT INTO assignment_reminders (
                            id, assignment_id, message, sent_by, sent_date, reminder_type
                        ) VALUES (
                            @id, @assignmentId, @message, @sentBy, @sentDate, @reminderType
                        )
                        """;

                    insertCmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                    insertCmd.Parameters.AddWithValue("@assignmentId", assignmentId);
                    insertCmd.Parameters.AddWithValue("@message", command.Message);
                    insertCmd.Parameters.AddWithValue("@sentBy", command.SentBy);
                    insertCmd.Parameters.AddWithValue("@sentDate", sentDate);
                    insertCmd.Parameters.AddWithValue("@reminderType", "Bulk");

                    await insertCmd.ExecuteNonQueryAsync(cancellationToken);

                    await using var updateCmd = connection.CreateCommand();
                    updateCmd.Transaction = transaction;
                    updateCmd.CommandText = """
                        UPDATE questionnaire_assignments
                        SET last_reminder_date = @lastReminderDate
                        WHERE id = @assignmentId
                        """;

                    updateCmd.Parameters.AddWithValue("@lastReminderDate", sentDate);
                    updateCmd.Parameters.AddWithValue("@assignmentId", assignmentId);

                    await updateCmd.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                logger.LogInformation("Bulk reminders sent successfully for {AssignmentCount} assignments", command.AssignmentIds.Count());

                return Result.Success($"Bulk reminders sent successfully for {command.AssignmentIds.Count()} assignments");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during send bulk reminder transaction, rolling back");
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send bulk reminders for {AssignmentCount} assignments", command.AssignmentIds.Count());
            return Result.Fail($"Failed to send bulk reminders: {ex.Message}", 500);
        }
    }

    private class EmployeeDetails
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
