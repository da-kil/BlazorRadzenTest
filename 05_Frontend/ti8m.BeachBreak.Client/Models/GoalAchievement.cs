namespace ti8m.BeachBreak.Client.Models;

public class GoalAchievement
{
    public string Description { get; set; } = string.Empty;
    public int AchievementPercentage { get; set; }
    public string EmployeeJustification { get; set; } = string.Empty;
    public string SupervisorJustification { get; set; } = string.Empty;
}