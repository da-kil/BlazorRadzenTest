namespace ti8m.BeachBreak.Client.Models;

public class CompetencyDefinition
{
    // Parameterless constructor for JSON deserialization
    public CompetencyDefinition()
    {
        Key = string.Empty;
        Title = string.Empty;
        Description = string.Empty;
    }

    public CompetencyDefinition(string key, string title, string description, bool isRequired, int order = 0)
    {
        Key = key;
        Title = title;
        Description = description;
        IsRequired = isRequired;
        Order = order;
    }

    public string Key { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
}