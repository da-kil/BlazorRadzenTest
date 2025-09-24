namespace ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;

public class CreateCategoryCommand : ICommand<Result>
{
    public CommandCategory Category { get; init; }

    public CreateCategoryCommand(CommandCategory category)
    {
        Category = category;
    }
}