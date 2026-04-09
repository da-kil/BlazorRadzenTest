using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.Shared;

public class PublicationInfo : ValueObject
{
    public TemplateStatus Status { get; private set; }
    public DateTime? PublishedDate { get; private set; }
    public DateTime? LastPublishedDate { get; private set; }
    public Guid? PublishedByEmployeeId { get; private set; }

    public PublicationInfo(TemplateStatus status, DateTime? publishedDate, DateTime? lastPublishedDate, Guid? publishedByEmployeeId)
    {
        Status = status;
        PublishedDate = publishedDate;
        LastPublishedDate = lastPublishedDate;
        PublishedByEmployeeId = publishedByEmployeeId;
    }

    public bool IsPublished => Status == TemplateStatus.Published;
    public bool CanBeArchived => Status != TemplateStatus.Archived;

    public static PublicationInfo Draft() => new(TemplateStatus.Draft, null, null, null);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Status;
        yield return PublishedDate;
        yield return LastPublishedDate;
        yield return PublishedByEmployeeId;
    }
}
