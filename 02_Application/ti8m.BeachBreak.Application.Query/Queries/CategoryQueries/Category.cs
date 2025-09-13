namespace ti8m.BeachBreak.Application.Query.Queries.CategoryQueries;

public class Category
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameDe { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionDe { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModified { get; set; }
    public int SortOrder { get; set; }
}