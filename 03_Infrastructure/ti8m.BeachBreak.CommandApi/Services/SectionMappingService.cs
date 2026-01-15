using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Domain.QuestionnaireResponseAggregate.ValueObjects;

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
    /// Organizes section responses by their proper structure based on the questionnaire template.
    /// </summary>
    /// <param name="assignmentId">The assignment ID (used for logging and fallback validation)</param>
    /// <param name="templateId">Optional template ID for optimization. When provided, skips assignment lookup.</param>
    /// <param name="sectionResponses">The section responses to organize (sectionId -> response)</param>
    /// <param name="role">The completion role (Employee/Manager)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>>> OrganizeResponsesBySectionsAsync(
        Guid assignmentId,
        Guid? templateId,
        Dictionary<Guid, QuestionResponseValue> sectionResponses,
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
                    return CreateFallbackSectionStructure(sectionResponses, role);
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
                return CreateFallbackSectionStructure(sectionResponses, role);
            }

            // Step 3: Create section response structure (Section IS the question - no nested mapping)
            var organizedResponses = new Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>>();
            var template = templateResult.Payload;

            foreach (var section in template.Sections.OrderBy(s => s.Order))
            {
                // Check if this role should complete this section
                var completionRole = Enum.Parse<CompletionRole>(section.CompletionRole);
                if (completionRole != CompletionRole.Both && completionRole != role)
                {
                    continue; // Skip sections not meant for this role
                }

                // Check if this section has a response
                if (sectionResponses.TryGetValue(section.Id, out var response))
                {
                    if (!organizedResponses.ContainsKey(section.Id))
                    {
                        organizedResponses[section.Id] = new Dictionary<CompletionRole, QuestionResponseValue>();
                    }

                    organizedResponses[section.Id][role] = response;
                }
            }

            // If no sections matched, use fallback
            if (!organizedResponses.Any())
            {
                logger.LogWarning("No matching sections found for role {Role}, using fallback", role);
                return CreateFallbackSectionStructure(sectionResponses, role);
            }

            logger.LogInformation("Successfully organized {SectionCount} sections for role {Role}",
                organizedResponses.Count, role);

            return organizedResponses;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error organizing responses by sections for assignment {AssignmentId}, using fallback", assignmentId);
            return CreateFallbackSectionStructure(sectionResponses, role);
        }
    }

    /// <summary>
    /// Creates a fallback section structure when template lookup fails.
    /// Returns section responses as-is
    /// </summary>
    private Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>> CreateFallbackSectionStructure(
        Dictionary<Guid, QuestionResponseValue> sectionResponses,
        CompletionRole role)
    {
        var result = new Dictionary<Guid, Dictionary<CompletionRole, QuestionResponseValue>>();

        foreach (var kvp in sectionResponses)
        {
            result[kvp.Key] = new Dictionary<CompletionRole, QuestionResponseValue>
            {
                [role] = kvp.Value
            };
        }

        return result;
    }
}