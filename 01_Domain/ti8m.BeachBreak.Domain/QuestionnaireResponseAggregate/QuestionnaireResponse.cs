using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.Events;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate;

/// <summary>
/// Aggregate root representing responses to a questionnaire assignment.
/// Stores role-separated answers for questionnaire sections.
/// Workflow state is managed by QuestionnaireAssignment aggregate.
/// </summary>
public class QuestionnaireResponse : AggregateRoot
{
    public Guid AssignmentId { get; private set; }
    public Guid TemplateId { get; private set; }
    public Guid EmployeeId { get; private set; }

    // Role-based section responses: SectionId -> CompletionRole -> QuestionId -> TypedAnswer
    public Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>> SectionResponses { get; private set; } = new();

    public DateTime InitiatedDate { get; private set; }
    public DateTime LastModified { get; private set; }

    // Required for event sourcing reconstitution
    private QuestionnaireResponse() { }

    /// <summary>
    /// Initiates a new questionnaire response for an employee assignment.
    /// </summary>
    public QuestionnaireResponse(
        Guid id,
        Guid assignmentId,
        Guid templateId,
        Guid employeeId,
        DateTime initiatedDate)
    {
        RaiseEvent(new QuestionnaireResponseInitiated(
            id,
            assignmentId,
            templateId,
            employeeId,
            initiatedDate));
    }

    /// <summary>
    /// Records or updates responses for a specific section of the questionnaire.
    /// Note: Workflow state validation should be done by the calling command handler.
    /// </summary>
    public void RecordSectionResponse(Guid sectionId, CompletionRole role, Dictionary<Guid, QuestionResponseValue> questionResponses)
    {
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
    /// Determines which sections are complete based on this response's data.
    /// Follows the Information Expert principle - Response owns the data, so it validates itself.
    /// Template is passed in as the specification/rule book.
    /// </summary>
    /// <param name="template">The template defining sections and required questions</param>
    /// <param name="completionRole">The role whose responses should be validated (Employee or Manager)</param>
    /// <returns>List of completed section IDs</returns>
    public List<Guid> GetCompletedSections(
        QuestionnaireTemplateAggregate.QuestionnaireTemplate template,
        CompletionRole completionRole)
    {
        if (template == null)
            throw new ArgumentNullException(nameof(template));

        var completedSectionIds = new List<Guid>();

        foreach (var section in template.Sections)
        {
            // Get required questions for this section
            var requiredQuestions = section.Questions.Where(q => q.IsRequired).ToList();

            // If no required questions, section is automatically complete
            if (!requiredQuestions.Any())
            {
                completedSectionIds.Add(section.Id);
                continue;
            }

            // Check if this section has responses for the specified role
            if (!SectionResponses.TryGetValue(section.Id, out var roleResponses))
            {
                continue; // No responses for this section, not complete
            }

            if (!roleResponses.TryGetValue(completionRole, out var questionResponses))
            {
                continue; // No responses for this role, not complete
            }

            // Check if ALL required questions have been answered
            var allRequiredQuestionsAnswered = requiredQuestions.All(question =>
            {
                if (!questionResponses.TryGetValue(question.Id, out var response))
                {
                    return false; // Question not answered
                }

                return IsQuestionAnswered(question, response);
            });

            if (allRequiredQuestionsAnswered)
            {
                completedSectionIds.Add(section.Id);
            }
        }

        return completedSectionIds;
    }

    /// <summary>
    /// Determines if a question has been answered based on its type and configuration.
    /// Business rules for question completion.
    /// </summary>
    private bool IsQuestionAnswered(QuestionnaireTemplateAggregate.QuestionItem question, QuestionResponseValue response)
    {
        return question.Type switch
        {
            QuestionType.Assessment => IsAssessmentComplete(question, response),
            QuestionType.TextQuestion => IsTextQuestionComplete(question, response),
            QuestionType.Goal => true, // Goal questions never block completion (can be added during review)
            _ => true
        };
    }

    private bool IsTextQuestionComplete(QuestionnaireTemplateAggregate.QuestionItem question, QuestionResponseValue response)
    {
        if (response is not QuestionResponseValue.TextResponse textResponse)
        {
            return false;
        }

        // Get text sections count from configuration
        var textSectionsCount = GetConfigurationCollectionCount(question, "TextSections");
        if (textSectionsCount == 0)
        {
            return true; // No sections defined, consider complete
        }

        // Single section - check if it has a value
        if (textSectionsCount == 1)
        {
            return textResponse.TextSections.Any() && !string.IsNullOrWhiteSpace(textResponse.TextSections.FirstOrDefault());
        }

        // Multiple sections - check if at least one section has content
        for (int i = 0; i < textSectionsCount; i++)
        {
            if (i < textResponse.TextSections.Count && !string.IsNullOrWhiteSpace(textResponse.TextSections[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAssessmentComplete(QuestionnaireTemplateAggregate.QuestionItem question, QuestionResponseValue response)
    {
        if (response is not QuestionResponseValue.AssessmentResponse assessmentResponse)
        {
            return false;
        }

        // Get competency count from configuration
        var competencyCount = GetConfigurationCollectionCount(question, "Competencies");
        if (competencyCount == 0)
        {
            return true; // No competencies defined, consider complete
        }

        // Check if at least one competency is rated
        return assessmentResponse.Competencies.Any(c => c.Value.Rating > 0);
    }

    private int GetConfigurationCollectionCount(QuestionnaireTemplateAggregate.QuestionItem question, string key)
    {
        if (question.Configuration.TryGetValue(key, out var obj))
        {
            if (obj is System.Collections.ICollection collection)
            {
                return collection.Count;
            }
        }

        return 0;
    }

    // Event application methods (Apply pattern for event sourcing)

    public void Apply(QuestionnaireResponseInitiated @event)
    {
        Id = @event.AggregateId;
        AssignmentId = @event.AssignmentId;
        TemplateId = @event.TemplateId;
        EmployeeId = @event.EmployeeId;
        InitiatedDate = @event.InitiatedDate;
        LastModified = @event.InitiatedDate;
    }

    public void Apply(SectionResponseRecorded @event)
    {
        // Ensure the section exists in the dictionary
        if (!SectionResponses.ContainsKey(@event.SectionId))
        {
            SectionResponses[@event.SectionId] = new Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>();
        }

        // Store responses under the specific role
        SectionResponses[@event.SectionId][@event.Role] = @event.QuestionResponses;
        LastModified = @event.RecordedDate;
    }
}
