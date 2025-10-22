namespace ti8m.BeachBreak.Core.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for projection replay functionality
/// </summary>
public class ProjectionReplaySettings
{
    public const string SectionName = "ProjectionReplay";

    /// <summary>
    /// Whether projection replay is enabled. Should be false in production by default.
    /// </summary>
    public bool Enabled { get; set; }
}
