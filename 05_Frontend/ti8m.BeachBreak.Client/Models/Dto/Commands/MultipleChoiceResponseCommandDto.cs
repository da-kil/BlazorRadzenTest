namespace ti8m.BeachBreak.Client.Models.Dto.Commands;

public class MultipleChoiceResponseCommandDto
{
    public Dictionary<string, List<string>> SelectionsByQuestion { get; set; } = new();
}
