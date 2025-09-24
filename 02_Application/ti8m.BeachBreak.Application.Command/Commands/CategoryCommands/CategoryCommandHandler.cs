using Microsoft.AspNetCore.Http;
using ti8m.BeachBreak.Application.Command.Repositories;
using ti8m.BeachBreak.Domain;
using ti8m.BeachBreak.Domain.CategoryAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;

public class CategoryCommandHandler :
    ICommandHandler<CreateCategoryCommand, Result>,
    ICommandHandler<UpdateCategoryCommand, Result>,
    ICommandHandler<DeactivateCategoryCommand, Result>
{
    private readonly ICategoryAggregateRepository repository;

    public CategoryCommandHandler(ICategoryAggregateRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Result> HandleAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create the category using the domain aggregate
            var categoryId = Guid.NewGuid();
            var name = new Translation(command.Category.NameDe, command.Category.NameEn);
            var description = new Translation(command.Category.DescriptionDe, command.Category.DescriptionEn);

            var category = new Category(
                categoryId,
                name,
                description,
                command.Category.SortOrder
            );

            await repository.StoreAsync(category, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create category: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(UpdateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await repository.LoadAsync<Category>(command.Category.Id, cancellationToken: cancellationToken);

            if (category == null)
            {
                return Result.Fail($"Category with ID {command.Category.Id} not found", StatusCodes.Status404NotFound);
            }

            category.ChangeName(new Translation(command.Category.NameDe, command.Category.NameEn));
            category.ChangeDescription(new Translation(command.Category.DescriptionDe, command.Category.DescriptionEn));
            category.ChangeSortOrder(command.Category.SortOrder);
            
            await repository.StoreAsync(category, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to update category: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }

    public async Task<Result> HandleAsync(DeactivateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await repository.LoadAsync<Category>(command.CategoryId, cancellationToken: cancellationToken);

            if (category == null)
            {
                return Result.Fail($"Category with ID {command.CategoryId} not found", StatusCodes.Status404NotFound);
            }

            category.Deactivate();

            await repository.StoreAsync(category, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to deactivate category: {ex.Message}", StatusCodes.Status400BadRequest);
        }
    }
}