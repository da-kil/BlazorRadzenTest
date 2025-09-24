namespace ti8m.BeachBreak.Client.Models;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NameEn { get; set; } = string.Empty;
    public string NameDe { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionDe { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? LastModified { get; set; }
    public int SortOrder { get; set; } = 0;
}