using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireAggregate;

public class QuestionItem : Entity<Guid>
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public QuestionType Type { get; private set; }
    public int Order { get; private set; }
    public bool IsRequired { get; private set; } = true;
    public Dictionary<string, object> Configuration { get; private set; } = new();
    public List<string> Options { get; private set; } = new();

    private QuestionItem() { }

    public QuestionItem(
        Guid id,
        string title,
        string description,
        QuestionType type,
        int order,
        bool isRequired,
        Dictionary<string, object>? configuration = null,
        List<string>? options = null)
    {
        Id = id;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? string.Empty;
        Type = type;
        Order = order;
        IsRequired = isRequired;
        Configuration = configuration ?? new();
        Options = options ?? new();
    }

    public void UpdateTitle(string title)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
    }

    public void UpdateDescription(string description)
    {
        Description = description ?? string.Empty;
    }

    public void UpdateOrder(int order)
    {
        Order = order;
    }

    public void UpdateIsRequired(bool isRequired)
    {
        IsRequired = isRequired;
    }

    public void UpdateConfiguration(Dictionary<string, object> configuration)
    {
        Configuration = configuration ?? new();
    }

    public void UpdateOptions(List<string> options)
    {
        Options = options ?? new();
    }
}