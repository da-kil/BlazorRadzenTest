using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireResponseCommands;

public class QuestionnaireResponseCommandHandler :
    ICommandHandler<SaveEmployeeResponseCommand, Result<Guid>>,
    ICommandHandler<SubmitEmployeeResponseCommand, Result>
{
    private readonly ILogger<QuestionnaireResponseCommandHandler> logger;

    public QuestionnaireResponseCommandHandler(ILogger<QuestionnaireResponseCommandHandler> logger)
    {
        this.logger = logger;
    }

    public async Task<Result<Guid>> HandleAsync(SaveEmployeeResponseCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting save employee response for EmployeeId: {EmployeeId}, AssignmentId: {AssignmentId}", command.EmployeeId, command.AssignmentId);

        try
        {
            // TODO: Implement actual questionnaire response save logic
            // This was previously commented out in the EmployeeCommandHandler
            logger.LogWarning("SaveEmployeeResponseCommand handler not yet implemented - returning null for now");
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
            // TODO: Implement actual questionnaire response submit logic
            // This was previously commented out in the EmployeeCommandHandler
            logger.LogWarning("SubmitEmployeeResponseCommand handler not yet implemented - returning null for now");
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
}