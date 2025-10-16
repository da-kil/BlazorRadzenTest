using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.CommandApi.Dto;

public class QuestionSectionDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public CompletionRole CompletionRole { get; set; } = CompletionRole.Employee;
    public List<QuestionItemDto> Questions { get; set; } = new();
}