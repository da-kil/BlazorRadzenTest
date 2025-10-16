namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;

public class QuestionSection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public string CompletionRole { get; set; } = "Employee";
    public List<QuestionItem> Questions { get; set; } = new();
}
