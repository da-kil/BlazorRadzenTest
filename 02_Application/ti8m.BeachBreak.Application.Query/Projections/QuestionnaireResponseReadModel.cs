using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.Events;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Application.Query.Projections;

/// <summary>
/// Read model projection for questionnaire responses.
/// Automatically maintained by Marten from domain events using snapshot projection.
/// Stores role-based responses for employee and manager separately.
/// Workflow state is managed by QuestionnaireAssignment aggregate.
/// </summary>
public class QuestionnaireResponseReadModel
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid EmployeeId { get; set; }

    // Role-based section responses: SectionId -> CompletionRole -> QuestionId -> Answer
    public Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, object>>> SectionResponses { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }

    // Apply methods for Marten snapshot projection
    public void Apply(QuestionnaireResponseInitiated @event)
    {
        Id = @event.AggregateId;
        AssignmentId = @event.AssignmentId;
        TemplateId = @event.TemplateId;
        EmployeeId = @event.EmployeeId;
        CreatedAt = @event.InitiatedDate;
        LastModified = @event.InitiatedDate;
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
}
