using ti8m.BeachBreak.Core.Infrastructure.ValueObjects;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;

public class QuestionSection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public MultilingualText Title { get; set; } = new();
    public MultilingualText Description { get; set; } = new();
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public List<QuestionItem> Questions { get; set; } = new();
}
