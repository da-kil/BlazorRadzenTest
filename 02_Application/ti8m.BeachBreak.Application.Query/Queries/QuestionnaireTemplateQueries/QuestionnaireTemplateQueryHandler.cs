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
        try
        {
            var readModels = await repository.GetAllAsync(cancellationToken);
            var templates = readModels.Select(MapToQueryModel);

            logger.LogInformation("Retrieved {Count} questionnaire templates", templates.Count());
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve questionnaire templates");
            return Result<IEnumerable<QuestionnaireTemplate>>.Fail($"Failed to retrieve questionnaire templates: {ex.Message}", 500);
        }
    }

    public async Task<Result<QuestionnaireTemplate>> HandleAsync(QuestionnaireTemplateQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var readModel = await repository.GetByIdAsync(query.Id, cancellationToken);
            if (readModel == null)
            {
                logger.LogWarning("Questionnaire template with ID {Id} not found", query.Id);
                return Result<QuestionnaireTemplate>.Fail($"Questionnaire template with ID {query.Id} not found", 404);
            }

            var template = MapToQueryModel(readModel);
            logger.LogInformation("Retrieved questionnaire template with ID {Id}", query.Id);
            return Result<QuestionnaireTemplate>.Success(template);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve questionnaire template with ID {Id}", query.Id);
            return Result<QuestionnaireTemplate>.Fail($"Failed to retrieve questionnaire template: {ex.Message}", 500);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireTemplate>>> HandleAsync(PublishedQuestionnaireTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var readModels = await repository.GetPublishedAsync(cancellationToken);
            var templates = readModels.Select(MapToQueryModel);

            logger.LogInformation("Retrieved {Count} published questionnaire templates", templates.Count());
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve published questionnaire templates");
            return Result<IEnumerable<QuestionnaireTemplate>>.Fail($"Failed to retrieve published questionnaire templates: {ex.Message}", 500);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireTemplate>>> HandleAsync(DraftQuestionnaireTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var readModels = await repository.GetDraftAsync(cancellationToken);
            var templates = readModels.Select(MapToQueryModel);

            logger.LogInformation("Retrieved {Count} draft questionnaire templates", templates.Count());
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve draft questionnaire templates");
            return Result<IEnumerable<QuestionnaireTemplate>>.Fail($"Failed to retrieve draft questionnaire templates: {ex.Message}", 500);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireTemplate>>> HandleAsync(ArchivedQuestionnaireTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var readModels = await repository.GetArchivedAsync(cancellationToken);
            var templates = readModels.Select(MapToQueryModel);

            logger.LogInformation("Retrieved {Count} archived questionnaire templates", templates.Count());
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve archived questionnaire templates");
            return Result<IEnumerable<QuestionnaireTemplate>>.Fail($"Failed to retrieve archived questionnaire templates: {ex.Message}", 500);
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireTemplate>>> HandleAsync(AssignableQuestionnaireTemplatesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var readModels = await repository.GetAssignableAsync(cancellationToken);
            var templates = readModels.Select(MapToQueryModel);

            logger.LogInformation("Retrieved {Count} assignable questionnaire templates", templates.Count());
            return Result<IEnumerable<QuestionnaireTemplate>>.Success(templates);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve assignable questionnaire templates");
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
            Category = readModel.CategoryId.ToString(), // TODO: This should be resolved to category name
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