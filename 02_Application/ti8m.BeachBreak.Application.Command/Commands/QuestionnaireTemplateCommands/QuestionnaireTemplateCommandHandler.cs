using Microsoft.AspNetCore.Http;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain.CategoryAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireAggregate.Services;
using DomainQTemplate = ti8m.BeachBreak.Domain.QuestionnaireAggregate.QuestionnaireTemplate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class QuestionnaireTemplateCommandHandler :
    ICommandHandler<CreateQuestionnaireTemplateCommand, Result>,
    ICommandHandler<UpdateQuestionnaireTemplateCommand, Result>,
    ICommandHandler<DeleteQuestionnaireTemplateCommand, Result>,
    ICommandHandler<PublishQuestionnaireTemplateCommand, Result>,
    ICommandHandler<UnpublishQuestionnaireTemplateCommand, Result>,
    ICommandHandler<ActivateQuestionnaireTemplateCommand, Result>,
    ICommandHandler<DeactivateQuestionnaireTemplateCommand, Result>,
    ICommandHandler<ArchiveQuestionnaireTemplateCommand, Result>,
    ICommandHandler<RestoreQuestionnaireTemplateCommand, Result>
{
    private readonly IQuestionnaireTemplateAggregateRepository repository;
    private readonly ICategoryAggregateRepository categoryRepository;
    private readonly IQuestionnaireAssignmentService assignmentService;

    public QuestionnaireTemplateCommandHandler(
        IQuestionnaireTemplateAggregateRepository repository,
        ICategoryAggregateRepository categoryRepository,
        IQuestionnaireAssignmentService assignmentService)
    {
        this.repository = repository;
        this.categoryRepository = categoryRepository;
        this.assignmentService = assignmentService;
    }

    public async Task<Result> HandleAsync(CreateQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get category name from CategoryId
            var categoryName = await GetCategoryNameAsync(command.QuestionnaireTemplate.CategoryId, cancellationToken);

            // Create domain objects from command DTOs
            var templateId = Guid.NewGuid();
            var sections = MapToQuestionSections(command.QuestionnaireTemplate.Sections);
            var settings = MapToQuestionnaireSettings(command.QuestionnaireTemplate.Settings);

            // Create the questionnaire template using the domain aggregate
            var questionnaireTemplate = new DomainQTemplate(
                templateId,
                command.QuestionnaireTemplate.Name,
                command.QuestionnaireTemplate.Description,
                categoryName,
                sections,
                settings);

            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(UpdateQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var questionnaireTemplate = await repository.LoadAsync<DomainQTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            // Get category name from CategoryId
            var categoryName = await GetCategoryNameAsync(command.QuestionnaireTemplate.CategoryId, cancellationToken);

            // Update the template using domain methods
            questionnaireTemplate.ChangeName(command.QuestionnaireTemplate.Name);
            questionnaireTemplate.ChangeDescription(command.QuestionnaireTemplate.Description);
            questionnaireTemplate.ChangeCategory(categoryName);

            var sections = MapToQuestionSections(command.QuestionnaireTemplate.Sections);
            var settings = MapToQuestionnaireSettings(command.QuestionnaireTemplate.Settings);

            questionnaireTemplate.UpdateSections(sections);
            questionnaireTemplate.UpdateSettings(settings);

            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(DeleteQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var questionnaireTemplate = await repository.LoadAsync<DomainQTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            // Use the aggregate's business logic to determine if deletion is allowed
            await questionnaireTemplate.DeleteAsync(assignmentService, cancellationToken);
            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to delete questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(PublishQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var questionnaireTemplate = await repository.LoadAsync<DomainQTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            questionnaireTemplate.Publish(command.PublishedBy);
            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (ArgumentException ex)
        {
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to publish questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(UnpublishQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var questionnaireTemplate = await repository.LoadAsync<DomainQTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            // TODO: Add business logic to check for active assignments
            // This would require a domain service or additional repository to check assignments
            questionnaireTemplate.UnpublishToDraft();
            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to unpublish questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(ActivateQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var questionnaireTemplate = await repository.LoadAsync<DomainQTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            questionnaireTemplate.RestoreFromArchive();
            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to activate questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(DeactivateQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var questionnaireTemplate = await repository.LoadAsync<DomainQTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            // TODO: Add business logic to check for active assignments
            // This would require a domain service or additional repository to check assignments
            questionnaireTemplate.Archive();
            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to deactivate questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(ArchiveQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var questionnaireTemplate = await repository.LoadAsync<DomainQTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            questionnaireTemplate.Archive();
            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to archive questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(RestoreQuestionnaireTemplateCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var questionnaireTemplate = await repository.LoadAsync<DomainQTemplate>(command.Id, cancellationToken: cancellationToken);

            if (questionnaireTemplate == null)
            {
                return Result.Fail($"Questionnaire template with ID {command.Id} not found", StatusCodes.Status404NotFound);
            }

            questionnaireTemplate.RestoreFromArchive();
            await repository.StoreAsync(questionnaireTemplate, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail(ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to restore questionnaire template: {ex.Message}", StatusCodes.Status400BadRequest);
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
            MapToQuestionItems(section.Questions)
        )).ToList();
    }

    private static List<QuestionItem> MapToQuestionItems(List<CommandQuestionItem> commandItems)
    {
        return commandItems.Select(item => new QuestionItem(
            item.Id,
            item.Title,
            item.Description,
            item.Type,
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

    private async Task<string> GetCategoryNameAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.LoadAsync<Category>(categoryId, cancellationToken: cancellationToken);
        if (category == null)
        {
            throw new InvalidOperationException($"Category with ID {categoryId} not found");
        }

        // Return the category name - using English name as default, could be made configurable
        return category.Name.English;
    }
}