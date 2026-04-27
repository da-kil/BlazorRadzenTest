namespace ti8m.BeachBreak.Core.Domain.QuestionConfiguration;

/// <summary>
/// Configuration for a binary (Yes/No) question section.
/// Supports multiple independent binary items, each with customizable option labels.
/// </summary>
public sealed class BinaryConfiguration : IQuestionConfiguration
{
    public QuestionType QuestionType => QuestionType.Binary;
    public List<BinaryItem> Items { get; set; } = new();

    public bool IsValid() => true;
}
