namespace ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;

public class CreateCategoryCommand : ICommand<Result>
{
    public CreateCategoryDto Category { get; init; }

    public CreateCategoryCommand(CreateCategoryDto category)
    {
        Category = category;
    }
}

public class CreateCategoryDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameDe { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionDe { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}