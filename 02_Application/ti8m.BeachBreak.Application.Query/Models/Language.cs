namespace ti8m.BeachBreak.Application.Query.Models;

/// <summary>
/// Supported languages for Query side of CQRS architecture.
/// This maintains architectural independence from domain enums while providing type safety.
/// Values must match domain Language enum for proper mapping.
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