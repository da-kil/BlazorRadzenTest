using Marten;
using Marten.Events.Projections;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Domain.EmployeeAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate.Events;

namespace ti8m.BeachBreak.Infrastructure.Marten.Projections;

/// <summary>
/// Marten projection that creates a ReviewChangeLog entry for each answer edit during review.
/// Each ManagerEditedAnswerDuringReview event creates a new log entry.
/// Uses event-based projection with enrichment from QuestionnaireTemplate and QuestionnaireResponse.
/// </summary>
public class ReviewChangeLogProjection : EventProjection
{
    /// <summary>
    /// Creates a new ReviewChangeLog entry when a manager edits an answer during review.
    /// Enriches the log with section/question titles from template and old value from response.
    /// </summary>
    public async Task Project(ManagerEditedAnswerDuringReview @event, IDocumentOperations operations)
    {
        // Fetch the assignment to get the template ID
        var assignment = await operations.LoadAsync<QuestionnaireAssignmentReadModel>(@event.AggregateId);

        string sectionTitle = "Unknown Section";
        string questionTitle = "Unknown Question";
        string? oldValue = null;

        if (assignment != null)
        {
            // Fetch the template to get section title
            var template = await operations.LoadAsync<QuestionnaireTemplateReadModel>(assignment.TemplateId);
            if (template != null)
            {
                var section = template.Sections?.FirstOrDefault(s => s.Id == @event.SectionId);
                if (section != null)
                {
                    sectionTitle = section.TitleEnglish ?? "Unknown Section";
                    questionTitle = section.TitleEnglish ?? "Unknown Question"; // Section IS the question
                }
            }

            // Fetch the response to get the old value
            var response = await operations.Query<QuestionnaireResponseReadModel>()
                .FirstOrDefaultAsync(r => r.AssignmentId == @event.AggregateId);

            if (response != null)
            {
                // Navigate the 2-level dictionary structure: SectionId -> CompletionRole -> QuestionResponseValue
                // Map ApplicationRole to CompletionRole for compatibility with Response aggregate
                var completionRole = @event.OriginalCompletionRole == ApplicationRole.Employee ? CompletionRole.Employee : CompletionRole.Manager;
                if (response.SectionResponses.TryGetValue(@event.SectionId, out var roleResponses))
                {
                    if (roleResponses.TryGetValue(completionRole, out var answerObj))
                    {
                        oldValue = answerObj?.ToString();
                    }
                }
            }
        }

        var changeLog = new ReviewChangeLogReadModel
        {
            Id = Guid.NewGuid(),
            AssignmentId = @event.AggregateId,
            SectionId = @event.SectionId,
            QuestionId = @event.QuestionId,
            SectionTitle = sectionTitle,
            QuestionTitle = questionTitle,
            OriginalCompletionRole = @event.OriginalCompletionRole.ToString(),
            OldValue = oldValue,
            NewValue = @event.NewAnswer,
            ChangedAt = @event.EditedDate,
            ChangedByEmployeeId = @event.EditedByEmployeeId
        };

        // Store the new document
        operations.Store(changeLog);
    }
}
