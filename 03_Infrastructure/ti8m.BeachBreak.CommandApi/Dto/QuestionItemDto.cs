using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

namespace ti8m.BeachBreak.CommandApi.Dto;

public class QuestionItemDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TitleGerman { get; set; } = string.Empty;
    public string TitleEnglish { get; set; } = string.Empty;
    public string DescriptionGerman { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;
    public QuestionTypeDto Type { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
    public IQuestionConfiguration Configuration { get; set; } = new AssessmentConfiguration();
}