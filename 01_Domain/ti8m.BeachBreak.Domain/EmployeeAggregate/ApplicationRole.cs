namespace ti8m.BeachBreak.Domain.EmployeeAggregate;

/// <summary>
/// Defines application-level roles for authorization and data visibility.
/// This is separate from the Employee.Role property which represents job title/position.
/// </summary>
public enum ApplicationRole
{
    /// <summary>
    /// Employee can only see their own data and questionnaires
    /// </summary>
    Employee = 1,

    /// <summary>
    /// Team Lead can see their entire team hierarchy (all employees reporting to them)
    /// </summary>
    TeamLead = 2,

    /// <summary>
    /// HR can see entire organization except other HR employees
    /// </summary>
    HR = 3,

    /// <summary>
    /// HR Lead can see entire organization including all HR employees
    /// </summary>
    HRLead = 4,

    /// <summary>
    /// Admin has full system access to all data and functionality
    /// </summary>
    Admin = 5
}
