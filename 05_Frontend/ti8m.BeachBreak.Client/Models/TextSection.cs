namespace ti8m.BeachBreak.Client.Models;

public class TextSection
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public int Order { get; set; }
}