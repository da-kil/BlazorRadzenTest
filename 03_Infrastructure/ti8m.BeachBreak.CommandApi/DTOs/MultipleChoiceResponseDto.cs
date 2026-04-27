namespace ti8m.BeachBreak.CommandApi.DTOs;

public class MultipleChoiceResponseDto
{
    public Dictionary<string, List<string>> SelectionsByQuestion { get; set; } = new();
}
