namespace ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;

public class DeactivateCategoryCommand : ICommand<Result>
{
    public Guid CategoryId { get; init; }

    public DeactivateCategoryCommand(Guid categoryId)
    {
        CategoryId = categoryId;
    }
}