namespace ti8m.BeachBreak.Client.Models.Dto;

public record UnlinkEmployeeFeedbackDto
{
    public Guid QuestionId { get; init; }
    public Guid FeedbackId { get; init; }
}
