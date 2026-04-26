namespace ti8m.BeachBreak.Client.Models.DTOs;

public class MultipleChoiceResponseDataDto : QuestionResponseDataDto
{
    public List<string> SelectedKeys { get; set; } = new();
}
