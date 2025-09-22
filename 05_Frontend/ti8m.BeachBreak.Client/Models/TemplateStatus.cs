namespace ti8m.BeachBreak.Client.Models;

public enum TemplateStatus
{
    Draft,              // Active but not published
    Published,          // Active and published
    PublishedInactive,  // Published but temporarily disabled
    Inactive            // Completely disabled
}