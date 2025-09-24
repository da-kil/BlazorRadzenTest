namespace ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;

public class UpdateCategoryCommand : ICommand<Result>
{
    public UpdateCategoryDto Category { get; init; }

    public UpdateCategoryCommand(UpdateCategoryDto category)
    {
        Category = category;
    }
}

public class UpdateCategoryDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameDe { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionDe { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}