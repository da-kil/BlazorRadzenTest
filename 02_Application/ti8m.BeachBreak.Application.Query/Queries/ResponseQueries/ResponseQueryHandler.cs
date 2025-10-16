using Marten;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;

public class ResponseQueryHandler :
    IQueryHandler<GetAllResponsesQuery, List<QuestionnaireResponse>>,
    IQueryHandler<GetResponseByIdQuery, QuestionnaireResponse?>,
    IQueryHandler<GetResponseByAssignmentIdQuery, QuestionnaireResponse?>
{
    private readonly IDocumentStore _documentStore;
    private readonly ILogger<ResponseQueryHandler> _logger;

    public ResponseQueryHandler(IDocumentStore documentStore, ILogger<ResponseQueryHandler> logger)
    {
        _documentStore = documentStore;
        _logger = logger;
    }

    public async Task<List<QuestionnaireResponse>> HandleAsync(GetAllResponsesQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all questionnaire responses");

        try
        {
            using var session = _documentStore.LightweightSession();
            var readModels = await session.Query<QuestionnaireResponseReadModel>()
                .OrderByDescending(r => r.LastModified)
                .ToListAsync(token: cancellationToken);

            var responses = readModels.Select(MapToResponse).ToList();
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
            using var session = _documentStore.LightweightSession();
            var readModel = await session.LoadAsync<QuestionnaireResponseReadModel>(query.Id, cancellationToken);

            if (readModel == null)
            {
                _logger.LogWarning("No questionnaire response found with ID: {Id}", query.Id);
                return null;
            }

            var response = MapToResponse(readModel);
            _logger.LogInformation("Retrieved questionnaire response: {Id}", response.Id);
            return response;
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
            using var session = _documentStore.LightweightSession();
            var readModel = await session.Query<QuestionnaireResponseReadModel>()
                .Where(r => r.AssignmentId == query.AssignmentId)
                .FirstOrDefaultAsync(token: cancellationToken);

            if (readModel == null)
            {
                _logger.LogInformation("No questionnaire response found for assignment: {AssignmentId}", query.AssignmentId);
                return null;
            }

            var response = MapToResponse(readModel);
            _logger.LogInformation("Retrieved questionnaire response for assignment: {AssignmentId}", query.AssignmentId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving questionnaire response for assignment: {AssignmentId}", query.AssignmentId);
            return null;
        }
    }

    private static QuestionnaireResponse MapToResponse(QuestionnaireResponseReadModel readModel)
    {
        return new QuestionnaireResponse
        {
            Id = readModel.Id,
            AssignmentId = readModel.AssignmentId,
            TemplateId = readModel.TemplateId,
            EmployeeId = readModel.EmployeeId,
            SectionResponses = readModel.SectionResponses.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)kvp.Value
            ),
            LastModified = readModel.LastModified,
            StartedDate = readModel.CreatedAt
        };
    }
}