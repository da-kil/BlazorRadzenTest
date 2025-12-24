using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Service interface for managing feedback templates
/// </summary>
public interface IFeedbackTemplateService
{
    /// <summary>
    /// Gets all templates that the current user has access to (based on ownership rules)
    /// </summary>
    Task<List<FeedbackTemplate>> GetMyTemplatesAsync();

    /// <summary>
    /// Gets a specific template by ID
    /// </summary>
    Task<FeedbackTemplate?> GetTemplateByIdAsync(Guid id);

    /// <summary>
    /// Gets templates that support a specific feedback source type and are published
    /// </summary>
    Task<List<FeedbackTemplate>> GetTemplatesBySourceTypeAsync(FeedbackSourceType sourceType);

    /// <summary>
    /// Gets templates that support a specific feedback source type and are published,
    /// filtered by visibility rules (HR+ templates visible to all, TeamLead templates visible only to creator)
    /// </summary>
    Task<List<FeedbackTemplate>> GetVisibleTemplatesBySourceTypeAsync(FeedbackSourceType sourceType);

    /// <summary>
    /// Creates a new feedback template
    /// </summary>
    Task<FeedbackTemplate> CreateTemplateAsync(FeedbackTemplate template);

    /// <summary>
    /// Updates an existing feedback template (must be in draft status)
    /// </summary>
    Task<FeedbackTemplate?> UpdateTemplateAsync(FeedbackTemplate template);

    /// <summary>
    /// Publishes a template, making it available for use
    /// </summary>
    Task<FeedbackTemplate?> PublishTemplateAsync(Guid templateId);

    /// <summary>
    /// Archives a template, removing it from active use
    /// </summary>
    Task<bool> ArchiveTemplateAsync(Guid templateId);

    /// <summary>
    /// Deletes a template (soft delete)
    /// </summary>
    Task<bool> DeleteTemplateAsync(Guid templateId);

    /// <summary>
    /// Clones an existing template with a new ID
    /// </summary>
    /// <param name="templateId">The ID of the template to clone</param>
    /// <param name="namePrefix">Optional prefix for the cloned template name</param>
    /// <returns>The ID of the newly created template</returns>
    Task<Guid?> CloneTemplateAsync(Guid templateId, string? namePrefix = null);
}
