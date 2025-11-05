namespace ti8m.BeachBreak.QueryApi.Dto;

public class QuestionResponseDto
{
    public Guid QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }

    /// <summary>
    /// Strongly-typed response data that is independent of domain value objects.
    /// The concrete type corresponds to the QuestionType.
    /// </summary>
    public QuestionResponseDataDto? ResponseData { get; set; }

    public DateTime LastModified { get; set; } = DateTime.Now;
}