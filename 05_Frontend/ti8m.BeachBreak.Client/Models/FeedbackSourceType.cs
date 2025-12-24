namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Represents the source type of employee feedback
/// </summary>
public enum FeedbackSourceType
{
    /// <summary>
    /// Feedback from customers
    /// </summary>
    Customer = 0,

    /// <summary>
    /// Feedback from peers (colleagues at same level)
    /// </summary>
    Peer = 1,

    /// <summary>
    /// Feedback from project colleagues (team members on same project)
    /// </summary>
    ProjectColleague = 2
}
