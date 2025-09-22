namespace ti8m.BeachBreak.Client.Models;

public static class TemplateStatusHelper
{
    public static string GetStatusBadgeClass(TemplateStatus status) => status switch
    {
        TemplateStatus.Published => "badge bg-success",
        TemplateStatus.Draft => "badge bg-warning text-dark",
        TemplateStatus.PublishedInactive => "badge bg-secondary",
        TemplateStatus.Inactive => "badge bg-danger",
        _ => "badge bg-info"
    };

    public static string GetStatusText(TemplateStatus status) => status switch
    {
        TemplateStatus.Published => "PUBLISHED",
        TemplateStatus.Draft => "DRAFT",
        TemplateStatus.PublishedInactive => "DISABLED",
        TemplateStatus.Inactive => "INACTIVE",
        _ => "UNKNOWN"
    };

    public static string GetStatusIcon(TemplateStatus status) => status switch
    {
        TemplateStatus.Published => "publish",
        TemplateStatus.Draft => "edit",
        TemplateStatus.PublishedInactive => "block",
        TemplateStatus.Inactive => "disabled_by_default",
        _ => "help"
    };

    public static string GetStatusDescription(TemplateStatus status) => status switch
    {
        TemplateStatus.Published => "Available for assignments and visible in catalog",
        TemplateStatus.Draft => "In development - not yet available for assignments",
        TemplateStatus.PublishedInactive => "Published but temporarily disabled",
        TemplateStatus.Inactive => "Not available for use",
        _ => "Status unknown"
    };

    public static List<string> GetAvailableActions(TemplateStatus status) => status switch
    {
        TemplateStatus.Draft => new List<string> { "Save", "Publish" },
        TemplateStatus.Published => new List<string> { "Save", "Unpublish", "Disable" },
        TemplateStatus.PublishedInactive => new List<string> { "Enable", "Edit" },
        TemplateStatus.Inactive => new List<string> { "Activate" },
        _ => new List<string>()
    };

    public static bool CanPerformAction(TemplateStatus status, string action) => action.ToLower() switch
    {
        "save" => status == TemplateStatus.Draft || status == TemplateStatus.Published,
        "publish" => status == TemplateStatus.Draft,
        "unpublish" => status == TemplateStatus.Published,
        "disable" => status == TemplateStatus.Published,
        "enable" => status == TemplateStatus.PublishedInactive,
        "activate" => status == TemplateStatus.Inactive,
        "edit" => status != TemplateStatus.Inactive,
        _ => false
    };
}