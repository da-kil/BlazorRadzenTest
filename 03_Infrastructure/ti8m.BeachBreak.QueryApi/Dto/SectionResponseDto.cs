namespace ti8m.BeachBreak.QueryApi.Dto;

public class SectionResponseDto
{
    public Guid SectionId { get; set; }
    public bool IsCompleted { get; set; }
    public Dictionary<Guid, QuestionResponseDto> QuestionResponses { get; set; } = new();
}
