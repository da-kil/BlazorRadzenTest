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
    public async Task<Dictionary<Guid, Dictionary<CompletionRole, Dictionary<Guid, QuestionResponseValue>>>> OrganizeResponsesBySectionsAsync(
        Guid assignmentId,
        Dictionary<Guid, QuestionResponseValue> questionResponses,
        CompletionRole role,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Step 1: Get the assignment to find the template ID
            var assignmentResult = await queryDispatcher.QueryAsync(
                new QuestionnaireAssignmentQuery(assignmentId),
                cancellationToken);

            if (assignmentResult?.Payload == null)
            {
                logger.LogWarning("Assignment {AssignmentId} not found, using single section fallback", assignmentId);
                return CreateFallbackSectionStructure(questionResponses, role);
            }

            // Step 2: Get the template with section structure
            var templateResult = await queryDispatcher.QueryAsync(
                new QuestionnaireTemplateQuery(assignmentResult.Payload.TemplateId),
                cancellationToken);

            if (templateResult?.Payload?.Sections == null || !templateResult.Payload.Sections.Any())
            {
                logger.LogWarning("Template {TemplateId} has no sections, using single section fallback", assignmentResult.Payload.TemplateId);
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