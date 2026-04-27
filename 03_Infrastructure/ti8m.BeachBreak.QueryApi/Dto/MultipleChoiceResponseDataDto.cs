namespace ti8m.BeachBreak.QueryApi.Dto;

public class MultipleChoiceResponseDataDto : QuestionResponseDataDto
{
    public Dictionary<string, List<string>> SelectionsByQuestion { get; set; } = new();
}
