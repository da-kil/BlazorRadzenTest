namespace ti8m.BeachBreak.Client.Models;

public class AnnualGoal
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string MeasurementCriteria { get; set; } = string.Empty;
    public decimal WeightingPercentage { get; set; }
    public string CustomTitle { get; set; } = string.Empty;
    public bool? IsExpanded { get; set; } = false;
    public bool IsEditingTitle { get; set; } = false;
    public bool IsSelected { get; set; } = false;
    public bool ShowAutoSave { get; set; } = false;
}