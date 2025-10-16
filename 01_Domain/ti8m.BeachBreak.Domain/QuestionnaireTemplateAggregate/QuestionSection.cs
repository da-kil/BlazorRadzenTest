using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

public class QuestionSection : Entity<Guid>
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Order { get; private set; }
    public bool IsRequired { get; private set; } = true;
    public CompletionRole CompletionRole { get; private set; } = CompletionRole.Employee;
    public List<QuestionItem> Questions { get; private set; } = new();

    private QuestionSection() { }

    public QuestionSection(
        Guid id,
        string title,
        string description,
        int order,
        bool isRequired = true,
        CompletionRole completionRole = CompletionRole.Employee,
        List<QuestionItem>? questions = null)
    {
        Id = id;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? string.Empty;
        Order = order;
        IsRequired = isRequired;
        CompletionRole = completionRole;
        Questions = questions ?? new();
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

    public void AddQuestion(QuestionItem question)
    {
        if (question == null) throw new ArgumentNullException(nameof(question));
        Questions.Add(question);
    }

    public void RemoveQuestion(Guid questionId)
    {
        Questions.RemoveAll(q => q.Id == questionId);
    }

    public void UpdateQuestions(List<QuestionItem> questions)
    {
        Questions = questions ?? new();
    }

    public void UpdateCompletionRole(CompletionRole completionRole)
    {
        CompletionRole = completionRole;
    }
}