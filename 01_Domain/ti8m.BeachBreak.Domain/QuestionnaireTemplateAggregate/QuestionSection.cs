using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

public class QuestionSection : Entity<Guid>
{
    public Translation Title { get; private set; } = new("", "");
    public Translation Description { get; private set; } = new("", "");
    public int Order { get; private set; }
    public CompletionRole CompletionRole { get; private set; } = CompletionRole.Employee;
    public QuestionType Type { get; private set; }
    public IQuestionConfiguration Configuration { get; private set; } = new AssessmentConfiguration();
    public bool IsInstanceSpecific { get; private set; } = false;

    private QuestionSection() { }

    public QuestionSection(
        Guid id,
        Translation title,
        Translation description,
        int order,
        CompletionRole completionRole = CompletionRole.Employee,
        QuestionType type = QuestionType.Assessment,
        IQuestionConfiguration? configuration = null,
        bool isInstanceSpecific = false)
    {
        Id = id;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? new Translation("", "");
        Order = order;
        CompletionRole = completionRole;
        Type = type;
        Configuration = configuration ?? new AssessmentConfiguration();
        IsInstanceSpecific = isInstanceSpecific;

        ValidateConfigurationMatchesType();
    }

    /// <summary>
    /// Factory method for creating custom sections added during assignment initialization.
    /// Custom sections are instance-specific and excluded from aggregate reports.
    /// </summary>
    public static QuestionSection CreateCustomSection(
        Guid id,
        Translation title,
        Translation description,
        int order,
        CompletionRole completionRole,
        QuestionType type,
        IQuestionConfiguration configuration)
    {
        if (type == QuestionType.Goal)
        {
            throw new InvalidOperationException("Goal type questions cannot be added as custom sections");
        }

        return new QuestionSection(
            id,
            title,
            description,
            order,
            completionRole,
            type,
            configuration,
            isInstanceSpecific: true);
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

    public void UpdateType(QuestionType type)
    {
        Type = type;
        ValidateConfigurationMatchesType();
    }

    public void UpdateConfiguration(IQuestionConfiguration configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        ValidateConfigurationMatchesType();
    }

    public void ValidateConfigurationMatchesType()
    {
        if (Type != Configuration.QuestionType)
        {
            throw new InvalidOperationException(
                $"Configuration type mismatch: Section Type is {Type} but Configuration is for {Configuration.QuestionType}");
        }
    }

    public void UpdateCompletionRole(CompletionRole completionRole)
    {
        CompletionRole = completionRole;
    }
}