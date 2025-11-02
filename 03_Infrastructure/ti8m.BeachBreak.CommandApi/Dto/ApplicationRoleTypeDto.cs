namespace ti8m.BeachBreak.CommandApi.Dto;

public enum ApplicationRoleTypeDto
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