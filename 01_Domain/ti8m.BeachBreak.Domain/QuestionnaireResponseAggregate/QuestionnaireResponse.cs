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

    /// <summary>
    /// Determines if a text question has been completed based on required text sections.
    /// Validation rules:
    /// 1. If NO text sections configured: Automatically complete (nothing to validate)
    /// 2. If text sections configured but NONE are required: Automatically complete (nothing required)
    /// 3. If required text sections exist: Checks that ALL required sections have content
    /// </summary>
    /// <remarks>
    /// This validation respects item-level IsRequired flags on text sections, not just question-level flags.
    /// Example: A question with 3 text sections where only 2 are required will be complete when those 2 are filled,
    /// even if the optional 3rd section is empty.
    /// If all 3 sections are optional (IsRequired=false), the question is automatically complete.
    /// </remarks>
    private bool IsTextQuestionComplete(QuestionnaireTemplateAggregate.QuestionItem question, QuestionResponseValue response)
    {
        if (response is not QuestionResponseValue.TextResponse textResponse)
        {
            return false;
        }

        // Get text sections with IsRequired flags from configuration
        var textSectionItems = GetTextSectionsFromConfiguration(question);
        if (textSectionItems.Count == 0)
        {
            return true; // No items configured = nothing to validate = complete
        }

        // Get only required text sections
        var requiredTextSections = textSectionItems.Where(ts => ts.IsRequired).ToList();

        // If no required text sections exist, question is automatically complete
        if (!requiredTextSections.Any())
        {
            return true; // Nothing required = automatically complete
        }

        // Check that ALL required text sections have non-empty values
        return requiredTextSections.All(ts =>
        {
            var index = ts.Index;
            return index < textResponse.TextSections.Count &&
                   !string.IsNullOrWhiteSpace(textResponse.TextSections[index]);
        });
    }

    /// <summary>
    /// Determines if an assessment question has been completed based on required competencies.
    /// Validation rules:
    /// 1. If NO competencies configured: Automatically complete (nothing to validate)
    /// 2. If competencies configured but NONE are required: Automatically complete (nothing required)
    /// 3. If required competencies exist: Checks that ALL required competencies are rated (rating > 0)
    /// </summary>
    /// <remarks>
    /// This validation respects item-level IsRequired flags on competencies, not just question-level flags.
    /// Example: An assessment with 5 competencies where only 3 are required will be complete when those 3 are rated,
    /// even if the optional 2 competencies are unrated (rating = 0).
    /// If all 5 competencies are optional (IsRequired=false), the question is automatically complete.
    ///
    /// FIXED BUG (2025-11-10): Previously validated ALL competencies regardless of IsRequired flag,
    /// and required at least one to be filled even when all were optional.
    /// </remarks>
    private bool IsAssessmentComplete(QuestionnaireTemplateAggregate.QuestionItem question, QuestionResponseValue response)
    {
        if (response is not QuestionResponseValue.AssessmentResponse assessmentResponse)
        {
            return false;
        }

        // Get competencies with IsRequired flags from configuration
        var competencies = GetCompetenciesFromConfiguration(question);
        if (competencies.Count == 0)
        {
            return true; // No items configured = nothing to validate = complete
        }

        // Get only required competencies
        var requiredCompetencies = competencies.Where(c => c.IsRequired).ToList();

        // If no required competencies exist, question is automatically complete
        if (!requiredCompetencies.Any())
        {
            return true; // Nothing required = automatically complete
        }

        // Check that ALL required competencies have been rated
        return requiredCompetencies.All(c =>
            assessmentResponse.Competencies.TryGetValue(c.Key, out var competencyResponse) &&
            competencyResponse.Rating > 0);
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

    private List<CompetencyItem> GetCompetenciesFromConfiguration(QuestionItem question)
    {
        if (question.Configuration.TryGetValue("Competencies", out var obj))
        {
            if (obj is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var competencies = new List<CompetencyItem>();
                foreach (var item in jsonElement.EnumerateArray())
                {
                    string? key = null;
                    bool isRequired = false;

                    if (item.TryGetProperty("Key", out var keyProperty) && keyProperty.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        key = keyProperty.GetString();
                    }

                    if (item.TryGetProperty("IsRequired", out var isRequiredProperty))
                    {
                        if (isRequiredProperty.ValueKind == System.Text.Json.JsonValueKind.True)
                        {
                            isRequired = true;
                        }
                        else if (isRequiredProperty.ValueKind == System.Text.Json.JsonValueKind.False)
                        {
                            isRequired = false;
                        }
                    }

                    if (!string.IsNullOrEmpty(key))
                    {
                        competencies.Add(new CompetencyItem(key, isRequired));
                    }
                }
                return competencies;
            }
        }

        return new List<CompetencyItem>();
    }

    private List<TextSectionItem> GetTextSectionsFromConfiguration(QuestionItem question)
    {
        if (question.Configuration.TryGetValue("TextSections", out var obj))
        {
            if (obj is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var sections = new List<TextSectionItem>();
                int index = 0;
                foreach (var item in jsonElement.EnumerateArray())
                {
                    bool isRequired = false;

                    if (item.TryGetProperty("IsRequired", out var isRequiredProperty))
                    {
                        if (isRequiredProperty.ValueKind == System.Text.Json.JsonValueKind.True)
                        {
                            isRequired = true;
                        }
                        else if (isRequiredProperty.ValueKind == System.Text.Json.JsonValueKind.False)
                        {
                            isRequired = false;
                        }
                    }

                    sections.Add(new TextSectionItem(index, isRequired));
                    index++;
                }
                return sections;
            }
        }

        return new List<TextSectionItem>();
    }

    private record CompetencyItem(string Key, bool IsRequired);
    private record TextSectionItem(int Index, bool IsRequired);

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
