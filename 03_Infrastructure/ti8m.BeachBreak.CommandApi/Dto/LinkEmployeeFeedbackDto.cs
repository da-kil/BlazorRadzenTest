namespace ti8m.BeachBreak.CommandApi.Dto;

public record LinkEmployeeFeedbackDto
{
    public Guid QuestionId { get; init; }
    public Guid FeedbackId { get; init; }
}
