using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.ResponseQueries;

public class ResponseQueryHandler :
    IQueryHandler<GetAllResponsesQuery, List<QuestionnaireResponse>>,
    IQueryHandler<GetResponseByIdQuery, QuestionnaireResponse?>,
    IQueryHandler<GetResponseByAssignmentIdQuery, QuestionnaireResponse?>
{
    private readonly IQuestionnaireResponseRepository responseRepository;
    private readonly ILogger<ResponseQueryHandler> logger;

    public ResponseQueryHandler(IQuestionnaireResponseRepository responseRepository, ILogger<ResponseQueryHandler> logger)
    {
        this.responseRepository = responseRepository;
        this.logger = logger;
    }

    public async Task<List<QuestionnaireResponse>> HandleAsync(GetAllResponsesQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving all questionnaire responses");

        try
        {
            var readModels = await responseRepository.GetAllResponsesAsync(cancellationToken);

            var responses = readModels.Select(MapToResponse).ToList();
            logger.LogInformation("Retrieved {Count} questionnaire responses", responses.Count);
            return responses;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all questionnaire responses");
            return new List<QuestionnaireResponse>();
        }
    }

    public async Task<QuestionnaireResponse?> HandleAsync(GetResponseByIdQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving questionnaire response with ID: {Id}", query.Id);

        try
        {
            var readModel = await responseRepository.GetByIdAsync(query.Id, cancellationToken);

            if (readModel == null)
            {
                logger.LogWarning("No questionnaire response found with ID: {Id}", query.Id);
                return null;
            }

            var response = MapToResponse(readModel);
            logger.LogInformation("Retrieved questionnaire response: {Id}", response.Id);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving questionnaire response with ID: {Id}", query.Id);
            return null;
        }
    }

    public async Task<QuestionnaireResponse?> HandleAsync(GetResponseByAssignmentIdQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Retrieving questionnaire response for assignment: {AssignmentId}", query.AssignmentId);

        try
        {
            var readModel = await responseRepository.GetByAssignmentIdAsync(query.AssignmentId, cancellationToken);

            if (readModel == null)
            {
                logger.LogInformation("No questionnaire response found for assignment: {AssignmentId}", query.AssignmentId);
                return null;
            }

            var response = MapToResponse(readModel);
            logger.LogInformation("Retrieved questionnaire response for assignment: {AssignmentId}", query.AssignmentId);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving questionnaire response for assignment: {AssignmentId}", query.AssignmentId);
            return null;
        }
    }

    /// <summary>
    /// Direct mapping from ReadModel to strongly-typed QuestionnaireResponse.
    /// No conversion needed since both use the same strongly-typed structure.
    /// </summary>
    private static QuestionnaireResponse MapToResponse(QuestionnaireResponseReadModel readModel)
    {
        return new QuestionnaireResponse
        {
            Id = readModel.Id,
            AssignmentId = readModel.AssignmentId,
            TemplateId = readModel.TemplateId,
            EmployeeId = readModel.EmployeeId,
            SectionResponses = readModel.SectionResponses,
            LastModified = readModel.LastModified,
            StartedDate = readModel.CreatedAt
        };
    }
}