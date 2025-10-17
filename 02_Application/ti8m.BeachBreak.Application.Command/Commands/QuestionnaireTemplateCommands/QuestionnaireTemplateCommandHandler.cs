using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Services;
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

            logger.LogCreateQuestionnaireTemplate(templateId, command.QuestionnaireTemplate.Name);

            var sections = MapToQuestionSections(command.QuestionnaireTemplate.Sections);
            var settings = MapToQuestionnaireSettings(command.QuestionnaireTemplate.Settings);

            // Create the questionnaire template using the domain aggregate
            var questionnaireTemplate = new DomainQuestionnaireTemplate(
                templateId,
                command.QuestionnaireTemplate.Name,
                command.QuestionnaireTemplate.Description,
                command.QuestionnaireTemplate.CategoryId,
                command.QuestionnaireTemplate.RequiresManagerReview,
                sections,
                settings);

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
            questionnaireTemplate.ChangeName(command.QuestionnaireTemplate.Name);
            questionnaireTemplate.ChangeDescription(command.QuestionnaireTemplate.Description);
            questionnaireTemplate.ChangeCategory(command.QuestionnaireTemplate.CategoryId);

            var sections = MapToQuestionSections(command.QuestionnaireTemplate.Sections);
            var settings = MapToQuestionnaireSettings(command.QuestionnaireTemplate.Settings);

            questionnaireTemplate.UpdateSections(sections);
            questionnaireTemplate.UpdateSettings(settings);

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
            logger.LogPublishQuestionnaireTemplate(command.Id, command.PublishedBy);

            var questionnaireTemplate = await repository.LoadAsync<DomainQuestionnaireTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                logger.LogQuestionnaireTemplateNotFound(command.Id);
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            // Validate that section completion roles match review requirement before publishing
            questionnaireTemplate.ValidateSectionCompletionRoles();

            questionnaireTemplate.Publish(command.PublishedBy);
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
            section.Title,
            section.Description,
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
            item.Title,
            item.Description,
            MapQuestionType(item.Type),
            item.Order,
            item.IsRequired,
            item.Configuration,
            item.Options
        )).ToList();
    }

    private static QuestionnaireSettings MapToQuestionnaireSettings(CommandQuestionnaireSettings commandSettings)
    {
        return new QuestionnaireSettings(
            commandSettings.AllowSaveProgress,
            commandSettings.ShowProgressBar,
            commandSettings.RequireAllSections,
            commandSettings.SuccessMessage,
            commandSettings.IncompleteMessage,
            commandSettings.TimeLimit,
            commandSettings.AllowReviewBeforeSubmit
        );
    }

    private static Domain.QuestionnaireTemplateAggregate.QuestionType MapQuestionType(QuestionType dtoType) => dtoType switch
    {
        QuestionType.TextQuestion => Domain.QuestionnaireTemplateAggregate.QuestionType.TextQuestion,
        QuestionType.Assessment => Domain.QuestionnaireTemplateAggregate.QuestionType.Assessment,
        QuestionType.GoalAchievement => Domain.QuestionnaireTemplateAggregate.QuestionType.GoalAchievement,
        _ => throw new ArgumentOutOfRangeException(nameof(dtoType), dtoType, "Unknown question type")
    };
}