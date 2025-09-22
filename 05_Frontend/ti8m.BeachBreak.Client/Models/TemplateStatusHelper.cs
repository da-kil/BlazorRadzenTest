namespace ti8m.BeachBreak.Client.Models;

public static class TemplateStatusHelper
{
    public static string GetStatusBadgeClass(TemplateStatus status) => status switch
    {
        TemplateStatus.Published => "badge bg-success",
        TemplateStatus.Draft => "badge bg-warning text-dark",
        TemplateStatus.Archived => "badge bg-danger",
        _ => "badge bg-info"
    };

    public static string GetStatusText(TemplateStatus status) => status switch
    {
        TemplateStatus.Published => "PUBLISHED",
        TemplateStatus.Draft => "DRAFT",
        TemplateStatus.Archived => "ARCHIVED",
        _ => "UNKNOWN"
    };

    public static string GetStatusIcon(TemplateStatus status) => status switch
    {
        TemplateStatus.Published => "publish",
        TemplateStatus.Draft => "edit",
        TemplateStatus.Archived => "archive",
        _ => "help"
    };

    public static string GetStatusDescription(TemplateStatus status) => status switch
    {
        TemplateStatus.Published => "Available for assignments and visible in catalog",
        TemplateStatus.Draft => "In development - not yet available for assignments",
        TemplateStatus.Archived => "Archived - not available for use",
        _ => "Status unknown"
    };

    public static List<string> GetAvailableActions(TemplateStatus status) => status switch
    {
        TemplateStatus.Draft => new List<string> { "Save", "Publish", "Archive" },
        TemplateStatus.Published => new List<string> { "Unpublish", "Archive" },
        TemplateStatus.Archived => new List<string> { "Restore" },
        _ => new List<string>()
    };

    public static bool CanPerformAction(TemplateStatus status, string action) => action.ToLower() switch
    {
        "save" => status == TemplateStatus.Draft,
        "publish" => status == TemplateStatus.Draft,
        "unpublish" => status == TemplateStatus.Published,
        "archive" => status != TemplateStatus.Archived,
        "restore" => status == TemplateStatus.Archived,
        "edit" => status == TemplateStatus.Draft,
        _ => false
    };
}