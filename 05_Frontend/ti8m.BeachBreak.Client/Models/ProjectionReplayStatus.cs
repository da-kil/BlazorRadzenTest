namespace ti8m.BeachBreak.Client.Models;

public class ProjectionReplayStatus
{
    public Guid Id { get; set; }
    public string ProjectionName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long TotalEvents { get; set; }
    public long ProcessedEvents { get; set; }
    public int ProgressPercentage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string InitiatedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
