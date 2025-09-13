namespace ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;

public class CreateCategoryCommand : ICommand<Result>
{
    public Category Category { get; init; }

    public CreateCategoryCommand(Category category)
    {
        Category = category;
    }
}