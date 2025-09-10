namespace ti8m.BeachBreak.QueryApi.Dto;

public class QuestionResponseDto
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