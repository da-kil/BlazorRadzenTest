using ti8m.BeachBreak.Core.Infrastructure.ValueObjects;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class QuestionnaireTemplate
{
    public MultilingualText Name { get; set; } = new();
    public MultilingualText Description { get; set; } = new();
    public string Category { get; set; } = string.Empty;

    // Semantic status properties
    public bool IsActive { get; set; } = true;           // System availability
    public bool IsPublished { get; set; } = false;      // Ready for assignments
    public DateTime? PublishedDate { get; set; }        // First publish timestamp
    public DateTime? LastPublishedDate { get; set; }    // Most recent publish
    public string PublishedBy { get; set; } = string.Empty; // Who published it

    public List<QuestionSection> Sections { get; set; } = new();
    public QuestionnaireSettings Settings { get; set; } = new();
}
