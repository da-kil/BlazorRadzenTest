namespace ti8m.BeachBreak.Client.Models;

public class QuestionResponse
{
    public Guid QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }
    public object? Value { get; set; }
    public string? TextValue { get; set; }
    public int? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }
    public List<string>? MultipleValues { get; set; }
    public Dictionary<string, object>? ComplexValue { get; set; } // For complex questions like goals
    public DateTime LastModified { get; set; } = DateTime.Now;
}