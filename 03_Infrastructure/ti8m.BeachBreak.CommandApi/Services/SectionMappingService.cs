using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.CommandApi.Services;

/// <summary>
/// Service for organizing question responses by their proper sections based on questionnaire templates.
/// Consolidates section mapping logic to avoid duplication across controllers.
/// </summary>
public class SectionMappingService
{
    private readonly IQueryDispatcher queryDispatcher;
    private readonly ILogger<SectionMappingService> logger;

    public SectionMappingService(
        IQueryDispatcher queryDispatcher,
        ILogger<SectionMappingService> logger)
    {
        this.queryDispatcher = queryDispatcher;
        this.logger = logger;
    }

    /// <summary>
    /// Organizes question responses by their proper sections based on the questionnaire template.
    /// Maps questions to sections and filters by completion role.
    /// </summary>
    /// <param name="assignmentId">The assignment ID (used for logging and fallback validation)</param>
    /// <param name="templateId">Optional template ID for optimization. When provided, skips assignment lookup.</param>
    /// <param name="questionResponses">The question responses to organize</param>
    /// <param name="role">The completion role (Employee/Manager)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>>> OrganizeResponsesBySectionsAsync(
        Guid assignmentId,
        Guid? templateId,
        Dictionary<Guid, QuestionResponseValue> questionResponses,
        CompletionRole role,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guid actualTemplateId;

            if (templateId.HasValue)
            {
                // Optimization: Use provided templateId - skip assignment lookup!
                actualTemplateId = templateId.Value;
                logger.LogInformation("Using provided TemplateId {TemplateId} for assignment {AssignmentId} - skipping assignment lookup",
                    actualTemplateId, assignmentId);
            }
            else
            {
                // Fallback: Query assignment to get template ID (original behavior)
                logger.LogInformation("TemplateId not provided, looking up assignment {AssignmentId} to get template", assignmentId);

                var assignmentResult = await queryDispatcher.QueryAsync(
                    new QuestionnaireAssignmentQuery(assignmentId),
                    cancellationToken);

                if (assignmentResult?.Payload == null)
                {
                    logger.LogWarning("Assignment {AssignmentId} not found, using single section fallback", assignmentId);
                    return CreateFallbackSectionStructure(questionResponses, role);
                }

                actualTemplateId = assignmentResult.Payload.TemplateId;
                logger.LogInformation("Found TemplateId {TemplateId} for assignment {AssignmentId}", actualTemplateId, assignmentId);
            }

            // Get the template with section structure
            var templateResult = await queryDispatcher.QueryAsync(
                new QuestionnaireTemplateQuery(actualTemplateId),
                cancellationToken);

            if (templateResult?.Payload?.Sections == null || !templateResult.Payload.Sections.Any())
            {
                logger.LogWarning("Template {TemplateId} has no sections, using single section fallback", actualTemplateId);
                return CreateFallbackSectionStructure(questionResponses, role);
            }

            // Step 3: Create section-to-question mapping
            var sectionResponses = new Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>>();
            var template = templateResult.Payload;

            foreach (var section in template.Sections.OrderBy(s => s.Order))
            {
                // Check if this role should complete this section
                var completionRole = Enum.Parse<CompletionRole>(section.CompletionRole);
                if (completionRole != CompletionRole.Both && completionRole != role)
                {
                    continue; // Skip sections not meant for this role
                }

                // Find questions in this section that have responses
                var sectionQuestions = new Dictionary<Guid, QuestionResponseValue>();

                foreach (var question in section.Questions.OrderBy(q => q.Order))
                {
                    if (questionResponses.TryGetValue(question.Id, out var response))
                    {
                        sectionQuestions[question.Id] = response;
                    }
                }

                // Only add the section if it has responses
                if (sectionQuestions.Any())
                {
                    if (!sectionResponses.ContainsKey(section.Id))
                    {
                        sectionResponses[section.Id] = new Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>();
                    }

                    sectionResponses[section.Id][role] = sectionQuestions;
                }
            }

            // If no sections matched, use fallback
            if (!sectionResponses.Any())
            {
                logger.LogWarning("No matching sections found for role {Role}, using fallback", role);
                return CreateFallbackSectionStructure(questionResponses, role);
            }

            logger.LogInformation("Successfully organized {QuestionCount} questions into {SectionCount} sections for role {Role}",
                questionResponses.Count, sectionResponses.Count, role);

            return sectionResponses;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error organizing responses by sections for assignment {AssignmentId}, using fallback", assignmentId);
            return CreateFallbackSectionStructure(questionResponses, role);
        }
    }

    /// <summary>
    /// Creates a fallback section structure when template lookup fails.
    /// Puts all questions in a single section for backwards compatibility.
    /// </summary>
    private Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>> CreateFallbackSectionStructure(
        Dictionary<Guid, QuestionResponseValue> questionResponses,
        CompletionRole role)
    {
        var fallbackSectionId = Guid.NewGuid();
        return new Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>>
        {
            [fallbackSectionId] = new Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>
            {
                [role] = questionResponses
            }
        };
    }
}