using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

public class QuestionItem : Entity<Guid>
{
    public Translation Title { get; private set; } = new("", "");
    public Translation Description { get; private set; } = new("", "");
    public QuestionType Type { get; private set; }
    public int Order { get; private set; }
    public bool IsRequired { get; private set; } = true;
    public IQuestionConfiguration Configuration { get; private set; }

    private QuestionItem()
    {
        // For event sourcing deserialization
        Configuration = new AssessmentConfiguration();
    }

    public QuestionItem(
        Guid id,
        Translation title,
        Translation description,
        QuestionType type,
        int order,
        bool isRequired,
        IQuestionConfiguration? configuration = null)
    {
        Id = id;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? new Translation("", "");
        Type = type;
        Order = order;
        IsRequired = isRequired;
        Configuration = configuration ?? CreateDefaultConfiguration(type);
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

    public void UpdateConfiguration(IQuestionConfiguration configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Creates a default configuration based on the question type.
    /// </summary>
    private static IQuestionConfiguration CreateDefaultConfiguration(QuestionType type)
    {
        return type switch
        {
            QuestionType.Assessment => new AssessmentConfiguration(),
            QuestionType.TextQuestion => new TextQuestionConfiguration(),
            QuestionType.Goal => new GoalConfiguration(),
            _ => throw new ArgumentException($"Unknown question type: {type}", nameof(type))
        };
    }
}