namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireTemplateCommands;

public class QuestionnaireTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public TemplateStatus Status { get; private set; } = TemplateStatus.Draft;
    public DateTime? PublishedDate { get; private set; }
    public DateTime? LastPublishedDate { get; private set; }
    public string PublishedBy { get; private set; } = string.Empty;

    public List<QuestionSection> Sections { get; set; } = new();
    public QuestionnaireSettings Settings { get; set; } = new();

    public bool CanBeAssignedToEmployee() => Status == TemplateStatus.Published;

    public bool CanBeEdited() => Status == TemplateStatus.Draft;

    public void Publish(string publishedBy)
    {
        if (string.IsNullOrWhiteSpace(publishedBy))
            throw new ArgumentException("Publisher name is required", nameof(publishedBy));

        if (Status == TemplateStatus.Archived)
            throw new InvalidOperationException("Cannot publish an archived template");

        var now = DateTime.UtcNow;
        Status = TemplateStatus.Published;
        PublishedBy = publishedBy;
        LastPublishedDate = now;

        if (PublishedDate == null)
            PublishedDate = now;
    }

    public void UnpublishToDraft()
    {
        if (Status != TemplateStatus.Published)
            throw new InvalidOperationException("Only published templates can be unpublished to draft");

        Status = TemplateStatus.Draft;
    }

    public void Archive()
    {
        Status = TemplateStatus.Archived;
    }

    public void RestoreFromArchive()
    {
        if (Status != TemplateStatus.Archived)
            throw new InvalidOperationException("Only archived templates can be restored");

        Status = TemplateStatus.Draft;
    }
}
