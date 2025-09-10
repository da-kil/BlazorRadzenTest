namespace ti8m.BeachBreak.Client.Models;

public class ParticipantInfo
{
    public string EmployeeName { get; set; } = string.Empty;
    public string Function { get; set; } = string.Empty;
    public string SupervisorName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; } = DateTime.Now;
    public string Location { get; set; } = string.Empty;
}

public class CompetencyRating
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class GoalAchievement
{
    public string Description { get; set; } = string.Empty;
    public int AchievementPercentage { get; set; }
    public string EmployeeJustification { get; set; } = string.Empty;
    public string SupervisorJustification { get; set; } = string.Empty;
}

public class CareerPlan
{
    public string CareerAmbitions { get; set; } = string.Empty;
    public string DevelopmentNeeds { get; set; } = string.Empty;
    public string ChallengesAndObstacles { get; set; } = string.Empty;
    public string StrengthsAndDevelopmentAreas { get; set; } = string.Empty;
    public string SupervisorFeedback { get; set; } = string.Empty;
}

public class AnnualGoal
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string MeasurementCriteria { get; set; } = string.Empty;
    public decimal WeightingPercentage { get; set; }
}