namespace ti8m.BeachBreak.Client.Models;

/// <summary>
/// Defines application-level roles for authorization and data visibility.
/// This is separate from the Employee.Role property which represents job title/position.
/// </summary>
public enum ApplicationRole
{
    /// <summary>
    /// Employee can only see their own data and questionnaires
    /// </summary>
    Employee = 0,

    /// <summary>
    /// Team Lead can see their entire team hierarchy (all employees reporting to them)
    /// </summary>
    TeamLead = 1,

    /// <summary>
    /// HR can see entire organization except other HR employees
    /// </summary>
    HR = 2,

    /// <summary>
    /// HR Lead can see entire organization including all HR employees
    /// </summary>
    HRLead = 3,

    /// <summary>
    /// Admin has full system access to all data and functionality
    /// </summary>
    Admin = 4
}
