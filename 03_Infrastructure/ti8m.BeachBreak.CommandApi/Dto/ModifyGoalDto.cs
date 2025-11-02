using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.CommandApi.Dto;

public class ModifyGoalDto
{
    public DateTime? TimeframeFrom { get; set; }
    public DateTime? TimeframeTo { get; set; }
    public string? ObjectiveDescription { get; set; }
    public string? MeasurementMetric { get; set; }
    public decimal? WeightingPercentage { get; set; }
    public string ModifiedByRole { get; set; } = string.Empty; // ApplicationRole as string (Employee, TeamLead, HR, HRLead, Admin)

    /// <summary>
    /// Helper method to parse ModifiedByRole string to ApplicationRole enum.
    /// </summary>
    public ApplicationRole GetModifiedByRoleEnum()
    {
        return Enum.TryParse<ApplicationRole>(ModifiedByRole, ignoreCase: true, out var role)
            ? role
            : ApplicationRole.Employee; // Safe default
    }
    public string? ChangeReason { get; set; } // Required only during InReview state
}
