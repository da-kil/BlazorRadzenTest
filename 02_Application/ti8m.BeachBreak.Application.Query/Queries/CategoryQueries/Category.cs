namespace ti8m.BeachBreak.Application.Query.Queries.CategoryQueries;

public class Category
{
    public Guid Id { get; set; }
    public string NameGerman { get; set; }
    public string NameEnglish { get; set; }
    public string DescriptionGerman { get; set; }
    public string DescriptionEnglish { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}