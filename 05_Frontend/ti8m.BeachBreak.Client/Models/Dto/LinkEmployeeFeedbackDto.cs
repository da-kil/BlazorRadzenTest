namespace ti8m.BeachBreak.Client.Models.Dto;

public record LinkEmployeeFeedbackDto
{
    public Guid QuestionId { get; init; }
    public Guid FeedbackId { get; init; }
}
