using ti8m.BeachBreak.Client.Models;

namespace ti8m.BeachBreak.Client.Services;

/// <summary>
/// Utility class for calculating questionnaire metrics and statistics.
/// Centralizes duplicate calculation logic across Team and Organization questionnaire pages.
/// </summary>
public static class QuestionnaireMetricsCalculator
{
    /// <summary>
    /// Counts assignments that have been completed (finalized).
    /// </summary>
    /// <param name="assignments">Collection of questionnaire assignments</param>
    /// <returns>Number of completed assignments</returns>
    public static int CountCompleted(IEnumerable<QuestionnaireAssignment> assignments)
        => assignments.Count(a => a.WorkflowState == WorkflowState.Finalized);

    /// <summary>
    /// Counts assignments that are pending (not finalized and not overdue).
    /// </summary>
    /// <param name="assignments">Collection of questionnaire assignments</param>
    /// <param name="now">Current date/time for due date comparison</param>
    /// <returns>Number of pending assignments</returns>
    public static int CountPending(IEnumerable<QuestionnaireAssignment> assignments, DateTime now)
        => assignments.Count(a =>
            a.WorkflowState != WorkflowState.Finalized &&
            (!a.DueDate.HasValue || a.DueDate.Value >= now));

    /// <summary>
    /// Counts assignments that are overdue (past due date and not finalized).
    /// </summary>
    /// <param name="assignments">Collection of questionnaire assignments</param>
    /// <param name="now">Current date/time for due date comparison</param>
    /// <returns>Number of overdue assignments</returns>
    public static int CountOverdue(IEnumerable<QuestionnaireAssignment> assignments, DateTime now)
        => assignments.Count(a =>
            a.DueDate.HasValue &&
            a.DueDate.Value < now &&
            a.WorkflowState != WorkflowState.Finalized);

    /// <summary>
    /// Calculates completion percentage as a rounded decimal.
    /// </summary>
    /// <param name="completed">Number of completed items</param>
    /// <param name="total">Total number of items</param>
    /// <returns>Completion percentage (0-100) rounded to 1 decimal place</returns>
    public static double CalculateCompletionPercentage(int completed, int total)
        => total > 0 ? Math.Round((double)completed / total * 100, 1) : 0;

    /// <summary>
    /// Calculates all metrics for a collection of assignments in one pass.
    /// More efficient than calling individual methods when you need all metrics.
    /// </summary>
    /// <param name="assignments">Collection of questionnaire assignments</param>
    /// <param name="now">Current date/time for due date comparison</param>
    /// <returns>Metrics containing counts and completion percentage</returns>
    public static QuestionnaireMetrics CalculateMetrics(IEnumerable<QuestionnaireAssignment> assignments, DateTime now)
    {
        var assignmentList = assignments.ToList(); // Avoid multiple enumeration
        var completed = CountCompleted(assignmentList);
        var total = assignmentList.Count;

        return new QuestionnaireMetrics
        {
            Total = total,
            Completed = completed,
            Pending = CountPending(assignmentList, now),
            Overdue = CountOverdue(assignmentList, now),
            CompletionPercentage = CalculateCompletionPercentage(completed, total)
        };
    }
}

/// <summary>
/// Container for questionnaire metrics calculations.
/// </summary>
public record QuestionnaireMetrics
{
    public int Total { get; init; }
    public int Completed { get; init; }
    public int Pending { get; init; }
    public int Overdue { get; init; }
    public double CompletionPercentage { get; init; }
}