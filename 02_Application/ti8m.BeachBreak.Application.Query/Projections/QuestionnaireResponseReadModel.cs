using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.Events;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

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

    public void Apply(ManagerEditedAnswerDuringReview @event)
    {
        // Ensure the section exists in the dictionary
        if (!SectionResponses.ContainsKey(@event.SectionId))
        {
            SectionResponses[@event.SectionId] = new Dictionary<CompletionRole, Dictionary<Guid, object>>();
        }

        // Ensure the role exists in the section
        if (!SectionResponses[@event.SectionId].ContainsKey(@event.OriginalCompletionRole))
        {
            SectionResponses[@event.SectionId][@event.OriginalCompletionRole] = new Dictionary<Guid, object>();
        }

        // Deserialize the answer if it's a JSON string (for complex types like Assessment, GoalAchievement, TextQuestion with sections)
        object answerValue = @event.NewAnswer;
        if (@event.NewAnswer.StartsWith("{") || @event.NewAnswer.StartsWith("["))
        {
            try
            {
                var deserializedValue = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(@event.NewAnswer);
                if (deserializedValue != null)
                {
                    answerValue = deserializedValue;
                }
            }
            catch
            {
                // If deserialization fails, store as string
                answerValue = @event.NewAnswer;
            }
        }

        // Update the answer for the specific question
        SectionResponses[@event.SectionId][@event.OriginalCompletionRole][@event.QuestionId] = answerValue;
        LastModified = @event.EditedDate;
    }
}
