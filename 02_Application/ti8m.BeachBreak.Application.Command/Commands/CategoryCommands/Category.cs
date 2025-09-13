namespace ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    public int SortOrder { get; set; } = 0;
}