namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Supported languages for frontend translation.
/// Values must match the backend Language enum for API compatibility.
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