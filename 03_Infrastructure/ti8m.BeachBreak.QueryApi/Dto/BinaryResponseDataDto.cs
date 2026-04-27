namespace ti8m.BeachBreak.QueryApi.Dto;

public class BinaryResponseDataDto : QuestionResponseDataDto
{
    public Dictionary<string, string?> Selections { get; set; } = new();
}
