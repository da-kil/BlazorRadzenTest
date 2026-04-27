namespace ti8m.BeachBreak.Client.Models;

public sealed class BinaryConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.Binary;
    public List<BinaryItem> Items { get; set; } = new();
}
