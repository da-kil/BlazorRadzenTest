using System.Text.Json;
using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

public class VersioningService : IVersioningService
{
    private readonly HttpClient _httpClient;
    private readonly IQuestionnaireApiService _questionnaireService;

    // In-memory storage for demo purposes - would be database in real implementation
    private readonly List<TemplateVersion> _versions = new();
    private readonly List<PublishHistory> _publishHistory = new();

    public VersioningService(IHttpClientFactory httpClientFactory, IQuestionnaireApiService questionnaireService)
    {
        _httpClient = httpClientFactory.CreateClient("CommandClient");
        _questionnaireService = questionnaireService;
    }

    public async Task<TemplateVersion?> CreateVersionAsync(Guid templateId, string changeDescription, TemplateVersionType versionType)
    {
        try
        {
            // Get current template
            var template = await _questionnaireService.GetTemplateByIdAsync(templateId);
            if (template == null) return null;

            // Mark previous current version as not current
            var previousVersions = _versions.Where(v => v.TemplateId == templateId).ToList();
            foreach (var prev in previousVersions)
            {
                prev.IsCurrentVersion = false;
            }

            // Create new version
            var versionNumber = previousVersions.Count + 1;
            var version = new TemplateVersion
            {
                TemplateId = templateId,
                VersionNumber = versionNumber,
                VersionLabel = GenerateVersionLabel(versionNumber, versionType),
                ChangeDescription = changeDescription,
                VersionType = versionType,
                IsCurrentVersion = true,
                CreatedBy = "Current User", // Would get from authentication context
                TemplateSnapshot = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true })
            };

            _versions.Add(version);

            // Record the versioning action
            await RecordPublishActionAsync(templateId, PublishHistoryAction.VersionCreated, "Current User",
                $"Created {version.VersionLabel}: {changeDescription}");

            return version;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<List<TemplateVersion>> GetVersionHistoryAsync(Guid templateId)
    {
        await Task.Delay(50); // Simulate async operation
        return _versions.Where(v => v.TemplateId == templateId)
                       .OrderByDescending(v => v.CreatedDate)
                       .ToList();
    }

    public async Task<TemplateVersion?> GetVersionAsync(Guid versionId)
    {
        await Task.Delay(50);
        return _versions.FirstOrDefault(v => v.Id == versionId);
    }

    public async Task<QuestionnaireTemplate?> GetTemplateAtVersionAsync(Guid versionId)
    {
        var version = await GetVersionAsync(versionId);
        if (version == null || string.IsNullOrEmpty(version.TemplateSnapshot))
            return null;

        try
        {
            return JsonSerializer.Deserialize<QuestionnaireTemplate>(version.TemplateSnapshot,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> RevertToVersionAsync(Guid templateId, Guid versionId)
    {
        try
        {
            var targetVersion = await GetVersionAsync(versionId);
            var restoredTemplate = await GetTemplateAtVersionAsync(versionId);

            if (targetVersion == null || restoredTemplate == null)
                return false;

            // Update the template with the version content
            restoredTemplate.Id = templateId; // Ensure correct ID
            restoredTemplate.LastModified = DateTime.Now;

            var updated = await _questionnaireService.UpdateTemplateAsync(restoredTemplate);
            if (updated != null)
            {
                // Record the revert action
                await RecordPublishActionAsync(templateId, PublishHistoryAction.VersionReverted, "Current User",
                    $"Reverted to {targetVersion.VersionLabel}");

                // Create a new version for this revert
                await CreateVersionAsync(templateId, $"Reverted to {targetVersion.VersionLabel}", TemplateVersionType.Minor);

                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetVersionDiffAsync(Guid fromVersionId, Guid toVersionId)
    {
        var fromTemplate = await GetTemplateAtVersionAsync(fromVersionId);
        var toTemplate = await GetTemplateAtVersionAsync(toVersionId);

        if (fromTemplate == null || toTemplate == null)
            return "Unable to compare versions";

        // Simple diff implementation - in real app would use proper diff library
        var changes = new List<string>();

        if (fromTemplate.Name != toTemplate.Name)
            changes.Add($"Name: '{fromTemplate.Name}' → '{toTemplate.Name}'");

        if (fromTemplate.Description != toTemplate.Description)
            changes.Add($"Description: Changed");

        if (fromTemplate.Category != toTemplate.Category)
            changes.Add($"Category: '{fromTemplate.Category}' → '{toTemplate.Category}'");

        if (fromTemplate.Sections.Count != toTemplate.Sections.Count)
            changes.Add($"Sections: {fromTemplate.Sections.Count} → {toTemplate.Sections.Count}");

        var totalQuestionsFrom = fromTemplate.Sections.Sum(s => s.Questions.Count);
        var totalQuestionsTo = toTemplate.Sections.Sum(s => s.Questions.Count);
        if (totalQuestionsFrom != totalQuestionsTo)
            changes.Add($"Total Questions: {totalQuestionsFrom} → {totalQuestionsTo}");

        return changes.Any() ? string.Join("\n", changes) : "No significant changes detected";
    }

    public async Task<List<PublishHistory>> GetPublishHistoryAsync(Guid templateId)
    {
        await Task.Delay(50);
        return _publishHistory.Where(h => h.TemplateId == templateId)
                             .OrderByDescending(h => h.ActionDate)
                             .ToList();
    }

    public async Task<PublishHistory?> RecordPublishActionAsync(Guid templateId, PublishHistoryAction action, string performedBy, string notes = "", bool wasScheduled = false)
    {
        await Task.Delay(50);

        var history = new PublishHistory
        {
            TemplateId = templateId,
            Action = action,
            PerformedBy = performedBy,
            Notes = notes,
            WasScheduled = wasScheduled
        };

        _publishHistory.Add(history);
        return history;
    }

    private static string GenerateVersionLabel(int versionNumber, TemplateVersionType versionType)
    {
        return versionType switch
        {
            TemplateVersionType.Major => $"v{versionNumber}.0",
            TemplateVersionType.Minor => $"v{Math.Max(1, versionNumber / 10)}.{versionNumber % 10}",
            TemplateVersionType.Patch => $"v{Math.Max(1, versionNumber / 100)}.{(versionNumber % 100) / 10}.{versionNumber % 10}",
            _ => $"v{versionNumber}"
        };
    }
}