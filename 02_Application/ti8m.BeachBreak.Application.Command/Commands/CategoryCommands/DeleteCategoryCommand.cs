namespace ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;

public class DeleteCategoryCommand : ICommand<Result>
{
    public Guid CategoryId { get; init; }

    public DeleteCategoryCommand(Guid categoryId)
    {
        CategoryId = categoryId;
    }
}