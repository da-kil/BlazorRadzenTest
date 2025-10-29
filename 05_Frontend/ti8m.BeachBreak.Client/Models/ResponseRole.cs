namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Represents the participant role for questionnaire responses.
/// Used as a type-safe wrapper around the string-based RoleResponses dictionary keys.
/// This is separate from ApplicationRole (organizational hierarchy) and CompletionRole (section assignment).
/// </summary>
/// <remarks>
/// - ApplicationRole: Who you are in the organization (Employee, TeamLead, HR, etc.)
/// - CompletionRole: Which sections are assigned to which participant type (Employee, Manager, Both)
/// - ResponseRole: Which response set you're accessing in the RoleResponses dictionary
/// </remarks>
public enum ResponseRole
{
    /// <summary>
    /// Employee response set - used for employee self-assessments and employee sections
    /// </summary>
    Employee = 0,

    /// <summary>
    /// Manager response set - used for manager evaluations and manager sections
    /// </summary>
    Manager = 1
}
