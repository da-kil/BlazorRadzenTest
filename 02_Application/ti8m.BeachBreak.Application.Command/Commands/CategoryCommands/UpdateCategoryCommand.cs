namespace ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;

public class UpdateCategoryCommand : ICommand<Result>
{
    public Category Category { get; init; }

    public UpdateCategoryCommand(Category category)
    {
        Category = category;
    }
}