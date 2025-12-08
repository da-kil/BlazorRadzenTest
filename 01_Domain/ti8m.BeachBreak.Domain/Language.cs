namespace ti8m.BeachBreak.Domain;

/// <summary>
/// Defines supported languages for multilingual content.
/// Values are explicitly set to ensure consistency across all layers in the CQRS/Event Sourcing architecture.
/// </summary>
public enum Language
{
    /// <summary>
    /// English language - default fallback language
    /// </summary>
    English = 0,

    /// <summary>
    /// German language
    /// </summary>
    German = 1
}