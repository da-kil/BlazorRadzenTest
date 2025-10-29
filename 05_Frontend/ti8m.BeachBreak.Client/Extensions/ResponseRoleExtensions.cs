using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Extensions;

/// <summary>
/// Extension methods for ResponseRole enum to provide type-safe conversions
/// to string keys used in RoleResponses dictionaries
/// </summary>
public static class ResponseRoleExtensions
{
    /// <summary>
    /// Converts ResponseRole enum to the string key used in RoleResponses dictionary.
    /// This provides type safety while maintaining compatibility with the existing
    /// string-based dictionary structure used across CommandApi, QueryApi, and database.
    /// </summary>
    /// <param name="role">The ResponseRole to convert</param>
    /// <returns>String key: "Employee" or "Manager"</returns>
    public static string ToRoleKey(this ResponseRole role) => role switch
    {
        ResponseRole.Employee => "Employee",
        ResponseRole.Manager => "Manager",
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Invalid ResponseRole value")
    };

    /// <summary>
    /// Converts CompletionRole to ResponseRole.
    /// Used to determine which response set to access based on section assignment.
    /// </summary>
    /// <param name="completionRole">The CompletionRole to convert</param>
    /// <returns>
    /// - CompletionRole.Employee → ResponseRole.Employee
    /// - CompletionRole.Manager → ResponseRole.Manager
    /// - CompletionRole.Both → ResponseRole.Manager (defaults to Manager perspective)
    /// </returns>
    public static ResponseRole ToResponseRole(this CompletionRole completionRole) => completionRole switch
    {
        CompletionRole.Employee => ResponseRole.Employee,
        CompletionRole.Manager => ResponseRole.Manager,
        CompletionRole.Both => ResponseRole.Manager, // Default to Manager for Both sections
        _ => throw new ArgumentOutOfRangeException(nameof(completionRole), completionRole, "Invalid CompletionRole value")
    };

    /// <summary>
    /// Converts ApplicationRole to ResponseRole based on organizational privileges.
    /// Determines which response set a user should access based on their organizational role.
    /// </summary>
    /// <param name="applicationRole">The ApplicationRole to convert</param>
    /// <returns>
    /// - Employee → ResponseRole.Employee
    /// - TeamLead, HR, HRLead, Admin → ResponseRole.Manager
    /// </returns>
    public static ResponseRole ToResponseRole(this ApplicationRole applicationRole) => applicationRole switch
    {
        ApplicationRole.Employee => ResponseRole.Employee,
        ApplicationRole.TeamLead => ResponseRole.Manager,
        ApplicationRole.HR => ResponseRole.Manager,
        ApplicationRole.HRLead => ResponseRole.Manager,
        ApplicationRole.Admin => ResponseRole.Manager,
        _ => throw new ArgumentOutOfRangeException(nameof(applicationRole), applicationRole, "Invalid ApplicationRole value")
    };

    /// <summary>
    /// Parses a string role key back to ResponseRole enum.
    /// Used when deserializing data from APIs or database.
    /// </summary>
    /// <param name="roleKey">The string key: "Employee" or "Manager"</param>
    /// <returns>The corresponding ResponseRole</returns>
    /// <exception cref="ArgumentException">Thrown if roleKey is not recognized</exception>
    public static ResponseRole ParseRoleKey(string roleKey) => roleKey switch
    {
        "Employee" => ResponseRole.Employee,
        "Manager" => ResponseRole.Manager,
        _ => throw new ArgumentException($"Unrecognized role key: '{roleKey}'. Expected 'Employee' or 'Manager'.", nameof(roleKey))
    };
}
