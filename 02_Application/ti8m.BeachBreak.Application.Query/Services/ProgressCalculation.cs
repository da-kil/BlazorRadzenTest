namespace ti8m.BeachBreak.Application.Query.Services;

/// <summary>
/// Represents role-based progress calculation for a questionnaire assignment.
/// Progress is calculated separately for employee and manager based on their respective questions.
/// </summary>
public class ProgressCalculation
{
    /// <summary>
    /// Progress percentage for questions assigned to the employee role.
    /// Range: 0-100
    /// </summary>
    public double EmployeeProgress { get; set; }

    /// <summary>
    /// Progress percentage for questions assigned to the manager role.
    /// Range: 0-100
    /// </summary>
    public double ManagerProgress { get; set; }

    /// <summary>
    /// Overall progress combining both roles.
    /// Weighted average based on number of questions per role.
    /// Range: 0-100
    /// </summary>
    public double OverallProgress { get; set; }

    /// <summary>
    /// Total number of required questions the employee must answer.
    /// </summary>
    public int EmployeeTotalQuestions { get; set; }

    /// <summary>
    /// Number of required questions the employee has answered.
    /// </summary>
    public int EmployeeAnsweredQuestions { get; set; }

    /// <summary>
    /// Total number of required questions the manager must answer.
    /// </summary>
    public int ManagerTotalQuestions { get; set; }

    /// <summary>
    /// Number of required questions the manager has answered.
    /// </summary>
    public int ManagerAnsweredQuestions { get; set; }
}
