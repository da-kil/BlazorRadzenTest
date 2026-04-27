namespace ti8m.BeachBreak.Client.Models.DTOs;

public class MultipleChoiceResponseDataDto : QuestionResponseDataDto
{
    public Dictionary<string, List<string>> SelectionsByQuestion { get; set; } = new();
}
