using ti8m.BeachBreak.Core.Infrastructure.ValueObjects;

namespace ti8m.BeachBreak.Client.Models;

public class CompetencyDefinition
{
    public CompetencyDefinition(string key, MultilingualText title, MultilingualText description, bool isRequired, int order = 0)
    {
        Key = key;
        Title = title;
        Description = description;
        IsRequired = isRequired;
        Order = order;
    }

    // For backward compatibility
    public CompetencyDefinition(string key, string title, string description, bool isRequired, int order = 0)
        : this(key, new MultilingualText(title), new MultilingualText(description), isRequired, order)
    {
    }

    public string Key { get; set; }
    public MultilingualText Title { get; set; }
    public MultilingualText Description { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
}