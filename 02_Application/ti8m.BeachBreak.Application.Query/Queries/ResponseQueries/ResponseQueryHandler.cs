using Npgsql;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;

public class ResponseQueryHandler :
    IQueryHandler<GetAllResponsesQuery, List<QuestionnaireResponse>>,
    IQueryHandler<GetResponseByIdQuery, QuestionnaireResponse?>,
    IQueryHandler<GetResponseByAssignmentIdQuery, QuestionnaireResponse?>
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<ResponseQueryHandler> _logger;

    public ResponseQueryHandler(NpgsqlDataSource dataSource, ILogger<ResponseQueryHandler> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }

    public async Task<List<QuestionnaireResponse>> HandleAsync(GetAllResponsesQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all questionnaire responses");

        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();

            command.CommandText = """
                SELECT id, assignment_id, template_id, employee_id, status,
                       section_responses, submitted_date, last_modified, created_at
                FROM questionnaire_responses
                ORDER BY last_modified DESC
                """;

            var responses = new List<QuestionnaireResponse>();

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                responses.Add(MapToResponse(reader));
            }

            _logger.LogInformation("Retrieved {Count} questionnaire responses", responses.Count);
            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all questionnaire responses");
            return new List<QuestionnaireResponse>();
        }
    }

    public async Task<QuestionnaireResponse?> HandleAsync(GetResponseByIdQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving questionnaire response with ID: {Id}", query.Id);

        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();

            command.CommandText = """
                SELECT id, assignment_id, template_id, employee_id, status,
                       section_responses, submitted_date, last_modified, created_at
                FROM questionnaire_responses
                WHERE id = @id
                """;

            command.Parameters.AddWithValue("@id", query.Id);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var response = MapToResponse(reader);
                _logger.LogInformation("Retrieved questionnaire response: {Id}", response.Id);
                return response;
            }

            _logger.LogWarning("No questionnaire response found with ID: {Id}", query.Id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving questionnaire response with ID: {Id}", query.Id);
            return null;
        }
    }

    public async Task<QuestionnaireResponse?> HandleAsync(GetResponseByAssignmentIdQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving questionnaire response for assignment: {AssignmentId}", query.AssignmentId);

        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();

            command.CommandText = """
                SELECT id, assignment_id, template_id, employee_id, status,
                       section_responses, submitted_date, last_modified, created_at
                FROM questionnaire_responses
                WHERE assignment_id = @assignmentId
                """;

            command.Parameters.AddWithValue("@assignmentId", query.AssignmentId);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var response = MapToResponse(reader);
                _logger.LogInformation("Retrieved questionnaire response for assignment: {AssignmentId}", query.AssignmentId);
                return response;
            }

            _logger.LogInformation("No questionnaire response found for assignment: {AssignmentId}", query.AssignmentId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving questionnaire response for assignment: {AssignmentId}", query.AssignmentId);
            return null;
        }
    }

    private static QuestionnaireResponse MapToResponse(NpgsqlDataReader reader)
    {
        var sectionResponsesJson = reader.GetString(reader.GetOrdinal("section_responses"));
        var sectionResponses = JsonSerializer.Deserialize<Dictionary<Guid, object>>(sectionResponsesJson)
                              ?? new Dictionary<Guid, object>();

        return new QuestionnaireResponse
        {
            Id = reader.GetGuid(reader.GetOrdinal("id")),
            AssignmentId = reader.GetGuid(reader.GetOrdinal("assignment_id")),
            TemplateId = reader.GetGuid(reader.GetOrdinal("template_id")),
            EmployeeId = reader.GetGuid(reader.GetOrdinal("employee_id")),
            Status = Enum.Parse<ResponseStatus>(reader.GetString(reader.GetOrdinal("status"))),
            SectionResponses = sectionResponses,
            SubmittedDate = reader.IsDBNull(reader.GetOrdinal("submitted_date")) ? null : reader.GetDateTime(reader.GetOrdinal("submitted_date")),
            LastModified = reader.GetDateTime(reader.GetOrdinal("last_modified")),
            StartedDate = reader.GetDateTime(reader.GetOrdinal("created_at"))
        };
    }
}