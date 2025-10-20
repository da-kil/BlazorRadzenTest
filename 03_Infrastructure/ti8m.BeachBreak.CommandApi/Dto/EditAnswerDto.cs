namespace ti8m.BeachBreak.CommandApi.Dto;

public class EditAnswerDto
{
    public Guid SectionId { get; set; }
    public Guid QuestionId { get; set; }
    public string OriginalCompletionRole { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}
