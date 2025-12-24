using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Application.Query.Repositories;

/// <summary>
/// Repository for querying employee feedback read models.
/// Supports filtering by employee, source type, date range, and pagination.
/// </summary>
public interface IEmployeeFeedbackRepository : IRepository
{
    /// <summary>
    /// Gets a single feedback record by ID.
    /// </summary>
    /// <param name="feedbackId">The feedback ID to retrieve</param>
    /// <param name="includeDeleted">Whether to include deleted feedback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The feedback read model or null if not found</returns>
    Task<EmployeeFeedbackReadModel?> GetFeedbackByIdAsync(
        Guid feedbackId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets feedback records with filtering and pagination.
    /// </summary>
    /// <param name="employeeId">Filter by specific employee (optional)</param>
    /// <param name="sourceType">Filter by feedback source type (optional)</param>
    /// <param name="fromDate">Filter by feedback date range start (optional)</param>
    /// <param name="toDate">Filter by feedback date range end (optional)</param>
    /// <param name="providerName">Filter by provider name - partial match (optional)</param>
    /// <param name="projectName">Filter by project name - partial match (optional)</param>
    /// <param name="includeDeleted">Whether to include deleted feedback</param>
    /// <param name="currentFiscalYearOnly">Filter to current fiscal year only</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="sortField">Field to sort by (default: FeedbackDate)</param>
    /// <param name="sortAscending">Sort direction (default: false - descending)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of feedback read models matching the criteria</returns>
    Task<List<EmployeeFeedbackReadModel>> GetEmployeeFeedbackAsync(
        Guid? employeeId = null,
        FeedbackSourceType? sourceType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? providerName = null,
        string? projectName = null,
        bool includeDeleted = false,
        bool currentFiscalYearOnly = false,
        int pageNumber = 1,
        int pageSize = 50,
        string sortField = "FeedbackDate",
        bool sortAscending = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current fiscal year feedback for a specific employee.
    /// Optimized for questionnaire review integration.
    /// </summary>
    /// <param name="employeeId">The employee to get feedback for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of feedback from the current fiscal year</returns>
    Task<List<EmployeeFeedbackReadModel>> GetCurrentYearFeedbackAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);
}
