namespace ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;

public class UpdateCategoryCommand : ICommand<Result>
{
    public CommandCategory Category { get; init; }

    public UpdateCategoryCommand(CommandCategory category)
    {
        Category = category;
    }
}