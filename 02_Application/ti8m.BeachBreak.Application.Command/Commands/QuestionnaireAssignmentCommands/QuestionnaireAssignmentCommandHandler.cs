using Microsoft.Extensions.Logging;
using Npgsql;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

public class QuestionnaireAssignmentCommandHandler : ICommandHandler<CreateQuestionnaireAssignmentCommand, Result>
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

    private class EmployeeDetails
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
