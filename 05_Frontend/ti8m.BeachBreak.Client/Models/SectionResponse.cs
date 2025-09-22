namespace ti8m.BeachBreak.Client.Models;

public class SectionResponse
{
    public Guid SectionId { get; set; }
    public bool IsCompleted { get; set; }
    public Dictionary<Guid, QuestionResponse> QuestionResponses { get; set; } = new();
}