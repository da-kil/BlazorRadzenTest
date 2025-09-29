namespace ti8m.BeachBreak.Client.Models;

public class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Number { get; set; } = string.Empty;
    public string? ManagerId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsIgnored { get; set; } = false;
    public bool IsDeleted { get; set; } = false;

    public string DisplayName => $"{Number} - {Name}";
}