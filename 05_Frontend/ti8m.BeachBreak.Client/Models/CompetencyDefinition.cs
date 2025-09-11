namespace ti8m.BeachBreak.Client.Models;

public class CompetencyDefinition
{
    public CompetencyDefinition(string key, string title, string description)
    {
        Key = key;
        Title = title;
        Description = description;
    }

    public string Key { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}