using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.CommandApi.Dto;

public class AddGoalDto
{
    public Guid QuestionId { get; set; }
    public string AddedByRole { get; set; } = string.Empty; // ApplicationRole as string (Employee, TeamLead, HR, HRLead, Admin)

    /// <summary>
    /// Helper method to parse AddedByRole string to ApplicationRole enum.
    /// </summary>
    public ApplicationRole GetAddedByRoleEnum()
    {
        return Enum.TryParse<ApplicationRole>(AddedByRole, ignoreCase: true, out var role)
            ? role
            : ApplicationRole.Employee; // Safe default
    }
    public DateTime TimeframeFrom { get; set; }
    public DateTime TimeframeTo { get; set; }
    public string ObjectiveDescription { get; set; } = string.Empty;
    public string MeasurementMetric { get; set; } = string.Empty;

    /// <summary>
    /// Weighting percentage (0-100). Optional during in-progress states (defaults to 0).
    /// Should be set during InReview state by manager.
    /// </summary>
    public decimal? WeightingPercentage { get; set; }
}
