using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Application.Query.Projections;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;

public class QuestionnaireTemplateQueryHandler :
    IQueryHandler<QuestionnaireTemplateListQuery, Result<IEnumerable<QuestionnaireTemplate>>>,
    IQueryHandler<QuestionnaireTemplateQuery, Result<QuestionnaireTemplate>>,
    IQueryHandler<PublishedQuestionnaireTemplatesQuery, Result<IEnumerable<QuestionnaireTemplate>>>,
    IQueryHandler<DraftQuestionnaireTemplatesQuery, Result<IEnumerable<QuestionnaireTemplate>>>,
    IQueryHandler<ArchivedQuestionnaireTemplatesQuery, Result<IEnumerable<QuestionnaireTemplate>>>,
    IQueryHandler<AssignableQuestionnaireTemplatesQuery, Result<IEnumerable<QuestionnaireTemplate>>>
{
    private readonly IQuestionnaireTemplateRepository repository;
    private readonly ILogger<QuestionnaireTemplateQueryHandler> logger;

    public QuestionnaireTemplateQueryHandler(IQuestionnaireTemplateRepository repository, ILogger<QuestionnaireTemplateQueryHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<QuestionnaireTemplate>>> HandleAsync(QuestionnaireTemplateListQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogQuestionnaireTemplateListQueryStarting();

        try
        {
            var readModels = await repository.GetAllAsync(cancellationToken);
            var templates = readModels.Select(MapToQueryModel).ToList();

            logger.LogQuestionnaireTemplateListQuerySucceeded(templates.Count);
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogQuestionnaireTemplateListQueryFailed(ex);
            return Result<IEnumerable<QuestionnaireTemplate>>.Fail($"Failed to retrieve questionnaire templates: {ex.Message}", 500);
        }
    }

    public async Task<Result<QuestionnaireTemplate>> HandleAsync(QuestionnaireTemplateQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogQuestionnaireTemplateQueryStarting(query.Id);

        try
        {
            var readModel = await repository.GetByIdAsync(query.Id, cancellationToken);
            if (readModel == null)
            {
                logger.LogQuestionnaireTemplateNotFound(query.Id);
                return Result<QuestionnaireTemplate>.Fail($"Questionnaire template with ID {query.Id} not found", 404);
            }

            var template = MapToQueryModel(readModel);
            logger.LogQuestionnaireTemplateQuerySucceeded(query.Id);
            return Result<QuestionnaireTemplate>.Success(template);
        }
        catch (Exception ex)
        {
            logger.LogQuestionnaireTemplateQueryFailed(query.Id, ex);
            return Result<QuestionnaireTemplate>.Fail($"Failed to retrieve questionnaire template: {ex.Message}", 500);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireTemplate>>> HandleAsync(PublishedQuestionnaireTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogPublishedQuestionnaireTemplatesQueryStarting();

        try
        {
            var readModels = await repository.GetPublishedAsync(cancellationToken);
            var templates = readModels.Select(MapToQueryModel).ToList();

            logger.LogPublishedQuestionnaireTemplatesQuerySucceeded(templates.Count);
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogPublishedQuestionnaireTemplatesQueryFailed(ex);
            return Result<IEnumerable<QuestionnaireTemplate>>.Fail($"Failed to retrieve published questionnaire templates: {ex.Message}", 500);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireTemplate>>> HandleAsync(DraftQuestionnaireTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogDraftQuestionnaireTemplatesQueryStarting();

        try
        {
            var readModels = await repository.GetDraftAsync(cancellationToken);
            var templates = readModels.Select(MapToQueryModel).ToList();

            logger.LogDraftQuestionnaireTemplatesQuerySucceeded(templates.Count);
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogDraftQuestionnaireTemplatesQueryFailed(ex);
            return Result<IEnumerable<QuestionnaireTemplate>>.Fail($"Failed to retrieve draft questionnaire templates: {ex.Message}", 500);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireTemplate>>> HandleAsync(ArchivedQuestionnaireTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogArchivedQuestionnaireTemplatesQueryStarting();

        try
        {
            var readModels = await repository.GetArchivedAsync(cancellationToken);
            var templates = readModels.Select(MapToQueryModel).ToList();

            logger.LogArchivedQuestionnaireTemplatesQuerySucceeded(templates.Count);
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogArchivedQuestionnaireTemplatesQueryFailed(ex);
            return Result<IEnumerable<QuestionnaireTemplate>>.Fail($"Failed to retrieve archived questionnaire templates: {ex.Message}", 500);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireTemplate>>> HandleAsync(AssignableQuestionnaireTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogAssignableQuestionnaireTemplatesQueryStarting();

        try
        {
            var readModels = await repository.GetAssignableAsync(cancellationToken);
            var templates = readModels.Select(MapToQueryModel).ToList();

            logger.LogAssignableQuestionnaireTemplatesQuerySucceeded(templates.Count);
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogAssignableQuestionnaireTemplatesQueryFailed(ex);
            return Result<IEnumerable<QuestionnaireTemplate>>.Fail($"Failed to retrieve assignable questionnaire templates: {ex.Message}", 500);
        }
    }

    private static QuestionnaireTemplate MapToQueryModel(QuestionnaireTemplateReadModel readModel)
    {
        return new QuestionnaireTemplate
        {
            Id = readModel.Id,
            Name = readModel.Name,
            Description = readModel.Description,
            CategoryId = readModel.CategoryId,
            CreatedDate = readModel.CreatedDate,
            Status = readModel.Status,
            PublishedDate = readModel.PublishedDate,
            LastPublishedDate = readModel.LastPublishedDate,
            PublishedBy = readModel.PublishedBy,
            Sections = readModel.Sections,
            Settings = readModel.Settings
        };
    }
}