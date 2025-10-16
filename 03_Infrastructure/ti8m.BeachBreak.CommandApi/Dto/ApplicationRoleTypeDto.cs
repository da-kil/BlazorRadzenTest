namespace ti8m.BeachBreak.CommandApi.Dto;

public enum ApplicationRoleTypeDto
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