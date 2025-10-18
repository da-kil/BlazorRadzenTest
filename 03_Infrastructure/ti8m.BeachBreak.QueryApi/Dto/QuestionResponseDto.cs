namespace ti8m.BeachBreak.QueryApi.Dto;

public class QuestionResponseDto
{
    public Guid QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }
    public Dictionary<string, object>? ComplexValue { get; set; } // All answer data stored here
    public DateTime LastModified { get; set; } = DateTime.Now;
}