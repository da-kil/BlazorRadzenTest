using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Services;
using ti8m.BeachBreak.Domain;
using DomainQuestionnaireTemplate = ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.QuestionnaireTemplate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class QuestionnaireTemplateCommandHandler :
    ICommandHandler<CreateQuestionnaireTemplateCommand, Result>,
    ICommandHandler<UpdateQuestionnaireTemplateCommand, Result>,
    ICommandHandler<DeleteQuestionnaireTemplateCommand, Result>,
    ICommandHandler<PublishQuestionnaireTemplateCommand, Result>,
    ICommandHandler<UnpublishQuestionnaireTemplateCommand, Result>,
    ICommandHandler<ArchiveQuestionnaireTemplateCommand, Result>,
    ICommandHandler<RestoreQuestionnaireTemplateCommand, Result>,
    ICommandHandler<CloneQuestionnaireTemplateCommand, Result<Guid>>
{
    private readonly IQuestionnaireTemplateAggregateRepository repository;
    private readonly IQuestionnaireAssignmentService assignmentService;
    private readonly ILogger<QuestionnaireTemplateCommandHandler> logger;

    public QuestionnaireTemplateCommandHandler(
        IQuestionnaireTemplateAggregateRepository repository,
        IQuestionnaireAssignmentService assignmentService,
        ILogger<QuestionnaireTemplateCommandHandler> logger)
    {
        this.repository = repository;
        this.assignmentService = assignmentService;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(CreateQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate ID if not provided (allows client to provide ID for efficient refetch)
            var templateId = command.QuestionnaireTemplate.Id != Guid.Empty
                ? command.QuestionnaireTemplate.Id
                : Guid.NewGuid();

            logger.LogCreateQuestionnaireTemplate(templateId, command.QuestionnaireTemplate.NameEnglish);

            var sections = MapToQuestionSections(command.QuestionnaireTemplate.Sections);

            // Create the questionnaire template using the domain aggregate
            var questionnaireTemplate = new DomainQuestionnaireTemplate(
                templateId,
                new Translation(command.QuestionnaireTemplate.NameGerman, command.QuestionnaireTemplate.NameEnglish),
                new Translation(command.QuestionnaireTemplate.DescriptionGerman, command.QuestionnaireTemplate.DescriptionEnglish),
                command.QuestionnaireTemplate.CategoryId,
                command.QuestionnaireTemplate.RequiresManagerReview,
                sections);

            // Validate that section completion roles match review requirement
            questionnaireTemplate.ValidateSectionCompletionRoles();

            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            logger.LogQuestionnaireTemplateCreated(templateId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogCreateQuestionnaireTemplateFailed(command.QuestionnaireTemplate.Id, ex.Message, ex);
            return Result.Fail($"Failed to create questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(UpdateQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogUpdateQuestionnaireTemplate(command.Id);

            var questionnaireTemplate = await repository.LoadAsync<DomainQuestionnaireTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                logger.LogQuestionnaireTemplateNotFound(command.Id);
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            // Update the template using domain methods
            questionnaireTemplate.ChangeName(new Translation(command.QuestionnaireTemplate.NameGerman, command.QuestionnaireTemplate.NameEnglish));
            questionnaireTemplate.ChangeDescription(new Translation(command.QuestionnaireTemplate.DescriptionGerman, command.QuestionnaireTemplate.DescriptionEnglish));
            questionnaireTemplate.ChangeCategory(command.QuestionnaireTemplate.CategoryId);

            // Handle RequiresManagerReview change (validates no active assignments exist)
            await questionnaireTemplate.ChangeReviewRequirementAsync(
                command.QuestionnaireTemplate.RequiresManagerReview,
                assignmentService,
                cancellationToken);

            var sections = MapToQuestionSections(command.QuestionnaireTemplate.Sections);

            questionnaireTemplate.UpdateSections(sections);

            // Validate that section completion roles match review requirement
            questionnaireTemplate.ValidateSectionCompletionRoles();

            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            logger.LogQuestionnaireTemplateUpdated(command.Id);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogUpdateQuestionnaireTemplateFailed(command.Id, ex.Message, ex);
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogUpdateQuestionnaireTemplateFailed(command.Id, ex.Message, ex);
            return Result.Fail($"Failed to update questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(DeleteQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDeleteQuestionnaireTemplate(command.Id);

            var questionnaireTemplate = await repository.LoadAsync<DomainQuestionnaireTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                logger.LogQuestionnaireTemplateNotFound(command.Id);
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            // Use the aggregate's business logic to determine if deletion is allowed
            await questionnaireTemplate.DeleteAsync(assignmentService, cancellationToken);
            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            logger.LogQuestionnaireTemplateDeleted(command.Id);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogDeleteQuestionnaireTemplateFailed(command.Id, ex.Message, ex);
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogDeleteQuestionnaireTemplateFailed(command.Id, ex.Message, ex);
            return Result.Fail($"Failed to delete questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(PublishQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogPublishQuestionnaireTemplate(command.Id, command.PublishedByEmployeeId.ToString());

            var questionnaireTemplate = await repository.LoadAsync<DomainQuestionnaireTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                logger.LogQuestionnaireTemplateNotFound(command.Id);
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            // Validate that section completion roles match review requirement before publishing
            questionnaireTemplate.ValidateSectionCompletionRoles();

            // Pass employee ID directly to domain (now accepts Guid)
            questionnaireTemplate.Publish(command.PublishedByEmployeeId);
            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            logger.LogQuestionnaireTemplatePublished(command.Id);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogPublishQuestionnaireTemplateFailed(command.Id, ex.Message, ex);
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (ArgumentException ex)
        {
            logger.LogPublishQuestionnaireTemplateFailed(command.Id, ex.Message, ex);
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogPublishQuestionnaireTemplateFailed(command.Id, ex.Message, ex);
            return Result.Fail($"Failed to publish questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(UnpublishQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogUnpublishQuestionnaireTemplate(command.Id);

            var questionnaireTemplate = await repository.LoadAsync<DomainQuestionnaireTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                logger.LogQuestionnaireTemplateNotFound(command.Id);
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            await questionnaireTemplate.UnpublishToDraftAsync(assignmentService);
            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            logger.LogQuestionnaireTemplateUnpublished(command.Id);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogUnpublishQuestionnaireTemplateFailed(command.Id, ex.Message, ex);
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogUnpublishQuestionnaireTemplateFailed(command.Id, ex.Message, ex);
            return Result.Fail($"Failed to unpublish questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(ArchiveQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogArchiveQuestionnaireTemplate(command.Id);

            var questionnaireTemplate = await repository.LoadAsync<DomainQuestionnaireTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                logger.LogQuestionnaireTemplateNotFound(command.Id);
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            questionnaireTemplate.Archive();
            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            logger.LogQuestionnaireTemplateArchived(command.Id);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogArchiveQuestionnaireTemplateFailed(command.Id, ex.Message, ex);
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogArchiveQuestionnaireTemplateFailed(command.Id, ex.Message, ex);
            return Result.Fail($"Failed to archive questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(RestoreQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogRestoreQuestionnaireTemplate(command.Id);

            var questionnaireTemplate = await repository.LoadAsync<DomainQuestionnaireTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                logger.LogQuestionnaireTemplateNotFound(command.Id);
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            questionnaireTemplate.RestoreFromArchive();
            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            logger.LogQuestionnaireTemplateRestored(command.Id);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogRestoreQuestionnaireTemplateFailed(command.Id, ex.Message, ex);
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogRestoreQuestionnaireTemplateFailed(command.Id, ex.Message, ex);
            return Result.Fail($"Failed to restore questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result<Guid>> HandleAsync(CloneQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogCloneQuestionnaireTemplate(command.SourceTemplateId);

            // Load source template
            var sourceTemplate = await repository.LoadAsync<DomainQuestionnaireTemplate>(
                command.SourceTemplateId,
                cancellationToken: cancellationToken);

            if (sourceTemplate == null)
            {
                logger.LogQuestionnaireTemplateNotFound(command.SourceTemplateId);
                return Result<Guid>.Fail(
                    $"Source template with ID {command.SourceTemplateId} not found",
                    StatusCodes.Status404NotFound);
            }

            if (sourceTemplate.IsDeleted)
            {
                logger.LogCloneQuestionnaireTemplateFailed(
                    command.SourceTemplateId,
                    "Cannot clone deleted template");
                return Result<Guid>.Fail(
                    "Cannot clone a deleted template",
                    StatusCodes.Status400BadRequest);
            }

            // Clone the template (domain logic handles deep copy)
            var newTemplateId = Guid.NewGuid();
            var clonedTemplate = DomainQuestionnaireTemplate.CloneFrom(
                newTemplateId,
                sourceTemplate,
                command.NamePrefix ?? "Copy of ");

            // Store the new template
            await repository.StoreAsync(clonedTemplate, cancellationToken);

            logger.LogQuestionnaireTemplateCloned(command.SourceTemplateId, newTemplateId);
            return Result<Guid>.Success(newTemplateId);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogCloneQuestionnaireTemplateFailed(command.SourceTemplateId, ex.Message, ex);
            return Result<Guid>.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogCloneQuestionnaireTemplateFailed(command.SourceTemplateId, ex.Message, ex);
            return Result<Guid>.Fail(
                $"Failed to clone questionnaire template: {ex.Message}",
                StatusCodes.Status400BadRequest);
        }
    }

    private static List<QuestionSection> MapToQuestionSections(List<CommandQuestionSection> commandSections)
    {
        return commandSections.Select(section => new QuestionSection(
            section.Id,
            new Translation(section.TitleGerman, section.TitleEnglish),
            new Translation(section.DescriptionGerman, section.DescriptionEnglish),
            section.Order,
            section.IsRequired,
            section.CompletionRole,
            MapToQuestionItems(section.Questions)
        )).ToList();
    }

    private static List<QuestionItem> MapToQuestionItems(List<CommandQuestionItem> commandItems)
    {
        return commandItems.Select(item => new QuestionItem(
            item.Id,
            new Translation(item.TitleGerman, item.TitleEnglish),
            new Translation(item.DescriptionGerman, item.DescriptionEnglish),
            MapQuestionType(item.Type),
            item.Order,
            item.IsRequired,
            item.Configuration
        )).ToList();
    }

    private static Domain.QuestionnaireTemplateAggregate.QuestionType MapQuestionType(QuestionType dtoType) => dtoType switch
    {
        QuestionType.TextQuestion => Domain.QuestionnaireTemplateAggregate.QuestionType.TextQuestion,
        QuestionType.Assessment => Domain.QuestionnaireTemplateAggregate.QuestionType.Assessment,
        QuestionType.Goal => Domain.QuestionnaireTemplateAggregate.QuestionType.Goal,
        _ => throw new ArgumentOutOfRangeException(nameof(dtoType), dtoType, "Unknown question type")
    };
}