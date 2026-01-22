using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Core.Domain.QuestionConfiguration;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.Events;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;
using AssessmentConfiguration = ti8m.BeachBreak.Core.Domain.QuestionConfiguration.AssessmentConfiguration;
using TextQuestionConfiguration = ti8m.BeachBreak.Core.Domain.QuestionConfiguration.TextQuestionConfiguration;

namespace ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate;

/// <summary>
/// Aggregate root representing responses to a questionnaire assignment.
/// Stores role-separated answers for questionnaire sections.
/// Workflow state is managed by QuestionnaireAssignment aggregate.
/// </summary>
public partial class QuestionnaireResponse : AggregateRoot
{
    public Guid AssignmentId { get; private set; }
    public Guid TemplateId { get; private set; }
    public Guid EmployeeId { get; private set; }

    // Role-based section responses: SectionId -> CompletionRole -> TypedAnswer
    public Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>> SectionResponses { get; private set; } = new();

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
    public void RecordSectionResponse(Guid sectionId, CompletionRole role, QuestionResponseValue sectionResponse)
    {
        if (sectionResponse == null)
            throw new ArgumentNullException(nameof(sectionResponse));

        // Allow empty responses for work-in-progress saves
        RaiseEvent(new SectionResponseRecorded(
            sectionId,
            role,
            sectionResponse,
            DateTime.UtcNow));
    }

    /// <summary>
    /// Determines which sections are complete based on this response's data.
    /// Follows the Information Expert principle - Response owns the data, so it validates itself.
    /// Template is passed in as the specification/rule book.
    /// </summary>
    /// <param name="template">The template defining sections (which ARE the questions)</param>
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
            // Check if this section has responses for the specified role
            if (!SectionResponses.TryGetValue(section.Id, out var roleResponses))
            {
                continue; // No responses for this section, not complete
            }

            if (!roleResponses.TryGetValue(completionRole, out var sectionResponse))
            {
                continue; // No response for this role, not complete
            }

            // Check if the section has been answered
            if (IsSectionAnswered(section, sectionResponse))
            {
                completedSectionIds.Add(section.Id);
            }
        }

        return completedSectionIds;
    }

    /// <summary>
    /// Determines if a section (which IS the question) has been answered based on its type and configuration.
    /// Business rules for section completion.
    /// </summary>
    private bool IsSectionAnswered(QuestionnaireTemplateAggregate.QuestionSection section, QuestionResponseValue response)
    {
        return section.Type switch
        {
            QuestionType.Assessment => IsAssessmentComplete(section, response),
            QuestionType.TextQuestion => IsTextQuestionComplete(section, response),
            QuestionType.Goal => true, // Goal sections never block completion (can be added during review)
            _ => true
        };
    }

    /// <summary>
    /// Determines if a text section has been completed based on required text sections.
    /// Validation rules:
    /// 1. If NO text sections configured: Automatically complete (nothing to validate)
    /// 2. If text sections configured but NONE are required: Automatically complete (nothing required)
    /// 3. If required text sections exist: Checks that ALL required sections have content
    /// </summary>
    /// <remarks>
    /// This validation respects item-level IsRequired flags on text sections, not just section-level flags.
    /// Example: A section with 3 text sections where only 2 are required will be complete when those 2 are filled,
    /// even if the optional 3rd section is empty.
    /// If all 3 sections are optional (IsRequired=false), the section is automatically complete.
    /// </remarks>
    private bool IsTextQuestionComplete(QuestionnaireTemplateAggregate.QuestionSection section, QuestionResponseValue response)
    {
        if (response is not QuestionResponseValue.TextResponse textResponse)
        {
            return false;
        }

        // Get text sections with IsRequired flags from configuration
        var textSectionItems = GetTextSectionsFromConfiguration(section);
        if (textSectionItems.Count == 0)
        {
            return true; // No items configured = nothing to validate = complete
        }

        // Get only required text sections
        var requiredTextSections = textSectionItems.Where(ts => ts.IsRequired).ToList();

        // If no required text sections exist, section is automatically complete
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
    /// Determines if an assessment section has been completed based on required competencies.
    /// Validation rules:
    /// 1. If NO competencies configured: Automatically complete (nothing to validate)
    /// 2. If competencies configured but NONE are required: Automatically complete (nothing required)
    /// 3. If required competencies exist: Checks that ALL required competencies are rated (rating > 0)
    /// </summary>
    /// <remarks>
    /// This validation respects item-level IsRequired flags on competencies, not just section-level flags.
    /// Example: An assessment with 5 competencies where only 3 are required will be complete when those 3 are rated,
    /// even if the optional 2 competencies are unrated (rating = 0).
    /// If all 5 competencies are optional (IsRequired=false), the section is automatically complete.
    /// </remarks>
    private bool IsAssessmentComplete(QuestionnaireTemplateAggregate.QuestionSection section, QuestionResponseValue response)
    {
        if (response is not QuestionResponseValue.AssessmentResponse assessmentResponse)
        {
            return false;
        }

        // Get evaluations with IsRequired flags from configuration
        var evaluations = GetEvaluationsFromConfiguration(section);
        if (evaluations.Count == 0)
        {
            return true; // No items configured = nothing to validate = complete
        }

        // Get only required evaluations
        var requiredEvaluations = evaluations.Where(e => e.IsRequired).ToList();

        // If no required evaluations exist, section is automatically complete
        if (!requiredEvaluations.Any())
        {
            return true; // Nothing required = automatically complete
        }

        // Check that ALL required evaluations have been rated
        return requiredEvaluations.All(e =>
            assessmentResponse.Evaluations.TryGetValue(e.Key, out var evaluationResponse) &&
            evaluationResponse.Rating > 0);
    }

    private List<EvaluationItem> GetEvaluationsFromConfiguration(QuestionSection section)
    {
        // Use strongly-typed configuration - much simpler!
        if (section.Configuration is AssessmentConfiguration config)
        {
            return config.Evaluations
                .Where(e => e.IsRequired)
                .Select(e => new EvaluationItem(e.Key, e.IsRequired))
                .ToList();
        }

        return new List<EvaluationItem>();
    }

    private List<TextSectionItem> GetTextSectionsFromConfiguration(QuestionSection section)
    {
        // Use strongly-typed configuration - much simpler!
        if (section.Configuration is TextQuestionConfiguration config)
        {
            return config.TextSections
                .OrderBy(section => section.Order) // Respect Order property instead of insertion order
                .Select((section, index) => new TextSectionItem(index, section.IsRequired))
                .ToList();
        }

        return new List<TextSectionItem>();
    }

    private record EvaluationItem(string Key, bool IsRequired);
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
            SectionResponses[@event.SectionId] = new Dictionary<CompletionRole, QuestionResponseValue>();
        }

        // Store response under the specific
        SectionResponses[@event.SectionId][@event.CompletionRole] = @event.SectionResponse;
        LastModified = @event.RecordedDate;
    }
}
