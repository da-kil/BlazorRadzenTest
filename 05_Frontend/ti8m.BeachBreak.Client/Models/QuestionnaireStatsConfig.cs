namespace ti8m.BeachBreak.Client.Models;

public class QuestionnaireStatsConfig
{
    public List<StatCardConfiguration> StatCards { get; set; } = new();
    public int Columns { get; set; } = 4;
}