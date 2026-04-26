namespace ti8m.BeachBreak.QueryApi.Dto;

public class MultipleChoiceResponseDataDto : QuestionResponseDataDto
{
    public List<string> SelectedKeys { get; set; } = new();
}
