namespace ti8m.BeachBreak.Client.Models;

public class StatCardConfiguration
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public Func<object> ValueCalculator { get; set; } = () => "0";
}