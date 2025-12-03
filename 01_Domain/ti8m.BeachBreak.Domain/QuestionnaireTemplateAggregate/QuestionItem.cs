using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

public class QuestionItem : Entity<Guid>
{
    public Translation Title { get; private set; } = new("", "");
    public Translation Description { get; private set; } = new("", "");
    public QuestionType Type { get; private set; }
    public int Order { get; private set; }
    public bool IsRequired { get; private set; } = true;
    public Dictionary<string, object> Configuration { get; private set; } = new();

    private QuestionItem() { }

    public QuestionItem(
        Guid id,
        Translation title,
        Translation description,
        QuestionType type,
        int order,
        bool isRequired,
        Dictionary<string, object>? configuration = null)
    {
        Id = id;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? new Translation("", "");
        Type = type;
        Order = order;
        IsRequired = isRequired;
        Configuration = configuration ?? new();
    }

    public void UpdateTitle(Translation title)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
    }

    public void UpdateDescription(Translation description)
    {
        Description = description ?? new Translation("", "");
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
}