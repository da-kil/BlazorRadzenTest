using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.Events;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate.ValueObjects;

namespace ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

public partial class EmployeeFeedback : AggregateRoot
{
    public Guid EmployeeId { get; private set; }
    public FeedbackSourceType SourceType { get; private set; }
    public FeedbackProviderInfo ProviderInfo { get; private set; } = null!;
    public DateTime FeedbackDate { get; private set; }
    public ConfigurableFeedbackData FeedbackData { get; private set; } = null!;

    // Metadata
    public Guid RecordedByEmployeeId { get; private set; }
    public DateTime RecordedDate { get; private set; }
    public DateTime? LastModifiedDate { get; private set; }
    public Guid? LastModifiedByEmployeeId { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    public Guid? DeletedByEmployeeId { get; private set; }
    public string? DeleteReason { get; private set; }

    private EmployeeFeedback() { }

    // Factory method for creating new feedback
    public static EmployeeFeedback RecordFeedback(
        Guid feedbackId,
        Guid employeeId,
        FeedbackSourceType sourceType,
        FeedbackProviderInfo providerInfo,
        DateTime feedbackDate,
        ConfigurableFeedbackData feedbackData,
        Guid recordedByEmployeeId)
    {
        // Validate business rules
        if (feedbackId == Guid.Empty)
            throw new ArgumentException("FeedbackId cannot be empty", nameof(feedbackId));

        if (employeeId == Guid.Empty)
            throw new ArgumentException("EmployeeId cannot be empty", nameof(employeeId));

        if (recordedByEmployeeId == Guid.Empty)
            throw new ArgumentException("RecordedByEmployeeId cannot be empty", nameof(recordedByEmployeeId));

        if (string.IsNullOrWhiteSpace(providerInfo.ProviderName))
            throw new ArgumentException("Provider name is required", nameof(providerInfo));

        if (!feedbackData.HasAnyContent)
            throw new ArgumentException("Feedback must contain at least one rating or comment", nameof(feedbackData));

        if (feedbackDate > DateTime.UtcNow)
            throw new ArgumentException("Feedback date cannot be in the future", nameof(feedbackDate));

        // Validate project context for project colleague feedback
        if (sourceType == FeedbackSourceType.ProjectColleague && !providerInfo.HasProjectContext)
            throw new ArgumentException("Project context is required for Project Colleague feedback", nameof(providerInfo));

        var feedback = new EmployeeFeedback();

        feedback.RaiseEvent(new EmployeeFeedbackRecorded(
            feedbackId,
            employeeId,
            sourceType,
            providerInfo,
            feedbackDate,
            feedbackData,
            recordedByEmployeeId,
            DateTime.UtcNow));

        return feedback;
    }

    public void UpdateFeedback(
        FeedbackProviderInfo providerInfo,
        DateTime feedbackDate,
        ConfigurableFeedbackData feedbackData,
        Guid updatedByEmployeeId)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot update deleted feedback");

        if (string.IsNullOrWhiteSpace(providerInfo.ProviderName))
            throw new ArgumentException("Provider name is required", nameof(providerInfo));

        if (!feedbackData.HasAnyContent)
            throw new ArgumentException("Feedback must contain at least one rating or comment", nameof(feedbackData));

        if (feedbackDate > DateTime.UtcNow)
            throw new ArgumentException("Feedback date cannot be in the future", nameof(feedbackDate));

        // Validate project context for project colleague feedback
        if (SourceType == FeedbackSourceType.ProjectColleague && !providerInfo.HasProjectContext)
            throw new ArgumentException("Project context is required for Project Colleague feedback", nameof(providerInfo));

        RaiseEvent(new EmployeeFeedbackUpdated(
            Id,
            providerInfo,
            feedbackDate,
            feedbackData,
            updatedByEmployeeId,
            DateTime.UtcNow));
    }

    public void DeleteFeedback(Guid deletedByEmployeeId, string? deleteReason = null)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Feedback is already deleted");

        if (deletedByEmployeeId == Guid.Empty)
            throw new ArgumentException("DeletedByEmployeeId cannot be empty", nameof(deletedByEmployeeId));

        RaiseEvent(new EmployeeFeedbackDeleted(
            Id,
            deletedByEmployeeId,
            DateTime.UtcNow,
            deleteReason));
    }

    public void Apply(EmployeeFeedbackRecorded @event)
    {
        Id = @event.FeedbackId;
        EmployeeId = @event.EmployeeId;
        SourceType = @event.SourceType;
        ProviderInfo = @event.ProviderInfo;
        FeedbackDate = @event.FeedbackDate;
        FeedbackData = @event.FeedbackData;
        RecordedByEmployeeId = @event.RecordedByEmployeeId;
        RecordedDate = @event.RecordedDate;
    }

    public void Apply(EmployeeFeedbackUpdated @event)
    {
        ProviderInfo = @event.ProviderInfo;
        FeedbackDate = @event.FeedbackDate;
        FeedbackData = @event.FeedbackData;
        LastModifiedByEmployeeId = @event.UpdatedByEmployeeId;
        LastModifiedDate = @event.UpdatedDate;
    }

    public void Apply(EmployeeFeedbackDeleted @event)
    {
        IsDeleted = true;
        DeletedDate = @event.DeletedDate;
        DeletedByEmployeeId = @event.DeletedByEmployeeId;
        DeleteReason = @event.DeleteReason;
    }

    public bool HasRatings => FeedbackData.HasAnyRating;
    public bool HasComments => FeedbackData.HasAnyComment;
    public decimal? AverageRating => FeedbackData.AverageRating;
    public int RatedItemsCount => FeedbackData.RatedItemsCount;

    public bool IsProjectFeedback => SourceType == FeedbackSourceType.ProjectColleague;
    public bool RequiresProjectContext => IsProjectFeedback;

    public string GetSourceTypeDisplayName() => SourceType switch
    {
        FeedbackSourceType.Customer => "Customer",
        FeedbackSourceType.Peer => "Peer",
        FeedbackSourceType.ProjectColleague => "Project Colleague",
        _ => "Unknown"
    };
}