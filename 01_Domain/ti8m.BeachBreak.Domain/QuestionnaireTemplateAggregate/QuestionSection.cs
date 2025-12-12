using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

public class QuestionSection : Entity<Guid>
{
    public Translation Title { get; private set; } = new("", "");
    public Translation Description { get; private set; } = new("", "");
    public int Order { get; private set; }
    public bool IsRequired { get; private set; } = true;
    public CompletionRole CompletionRole { get; private set; } = CompletionRole.Employee;
    public QuestionType Type { get; private set; }
    public IQuestionConfiguration Configuration { get; private set; } = new AssessmentConfiguration();

    private QuestionSection() { }

    public QuestionSection(
        Guid id,
        Translation title,
        Translation description,
        int order,
        bool isRequired = true,
        CompletionRole completionRole = CompletionRole.Employee,
        QuestionType type = QuestionType.Assessment,
        IQuestionConfiguration? configuration = null)
    {
        Id = id;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? new Translation("", "");
        Order = order;
        IsRequired = isRequired;
        CompletionRole = completionRole;
        Type = type;
        Configuration = configuration ?? new AssessmentConfiguration();

        ValidateConfigurationMatchesType();
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