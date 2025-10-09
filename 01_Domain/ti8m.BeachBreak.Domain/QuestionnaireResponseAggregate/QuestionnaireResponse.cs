using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.Events;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate;

/// <summary>
/// Aggregate root representing responses to a questionnaire assignment.
/// Encapsulates the business logic for recording, updating, and submitting questionnaire responses
/// from both employees and managers with role-based separation.
/// </summary>
public class QuestionnaireResponse : AggregateRoot
{
    public Guid AssignmentId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public ResponseStatus Status { get; private set; } = ResponseStatus.InProgress;

    // Role-based section responses: SectionId -> CompletionRole -> QuestionId -> Answer
    public Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, object>>> SectionResponses { get; private set; } = new();

    public DateTime InitiatedDate { get; private set; }
    public DateTime LastModified { get; private set; }
    public DateTime? SubmittedDate { get; private set; }

    // Required for event sourcing reconstitution
    private QuestionnaireResponse() { }

    /// <summary>
    /// Initiates a new questionnaire response for an employee assignment.
    /// </summary>
    public QuestionnaireResponse(
        Guid id,
        Guid assignmentId,
        Guid employeeId,
        DateTime initiatedDate)
    {
        RaiseEvent(new QuestionnaireResponseInitiated(
            id,
            assignmentId,
            employeeId,
            initiatedDate));
    }

    /// <summary>
    /// Records or updates responses for a specific section of the questionnaire.
    /// </summary>
    public void RecordSectionResponse(Guid sectionId, CompletionRole role, Dictionary<Guid, object> questionResponses)
    {
        if (Status == ResponseStatus.Submitted)
            throw new InvalidOperationException("Cannot modify a submitted response");

        if (questionResponses == null)
            throw new ArgumentNullException(nameof(questionResponses));

        // Allow empty responses for work-in-progress saves
        RaiseEvent(new SectionResponseRecorded(
            sectionId,
            role,
            questionResponses,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Submits the questionnaire response for review.
    /// </summary>
    public void Submit()
    {
        if (Status == ResponseStatus.Submitted)
            throw new InvalidOperationException("Response has already been submitted");

        if (SectionResponses.Count == 0)
            throw new InvalidOperationException("Cannot submit an empty response");

        RaiseEvent(new QuestionnaireResponseSubmitted(DateTime.UtcNow));
    }

    // Event application methods (Apply pattern for event sourcing)

    public void Apply(QuestionnaireResponseInitiated @event)
    {
        Id = @event.AggregateId;
        AssignmentId = @event.AssignmentId;
        EmployeeId = @event.EmployeeId;
        InitiatedDate = @event.InitiatedDate;
        LastModified = @event.InitiatedDate;
        Status = ResponseStatus.InProgress;
    }

    public void Apply(SectionResponseRecorded @event)
    {
        // Ensure the section exists in the dictionary
        if (!SectionResponses.ContainsKey(@event.SectionId))
        {
            SectionResponses[@event.SectionId] = new Dictionary<CompletionRole, Dictionary<Guid, object>>();
        }

        // Store responses under the specific role
        SectionResponses[@event.SectionId][@event.Role] = @event.QuestionResponses;
        LastModified = @event.RecordedDate;
    }

    public void Apply(QuestionnaireResponseSubmitted @event)
    {
        Status = ResponseStatus.Submitted;
        SubmittedDate = @event.SubmittedDate;
        LastModified = @event.SubmittedDate;
    }
}
