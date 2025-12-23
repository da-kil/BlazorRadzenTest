using Marten;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Domain.EmployeeFeedbackAggregate;

namespace ti8m.BeachBreak.Infrastructure.Marten.ReadModelRepositories;

internal class EmployeeFeedbackRepository(IDocumentStore store) : IEmployeeFeedbackRepository
{
    public async Task<EmployeeFeedbackReadModel?> GetFeedbackByIdAsync(
        Guid feedbackId,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();

        var query = session.Query<EmployeeFeedbackReadModel>()
            .Where(f => f.Id == feedbackId);

        if (!includeDeleted)
        {
            query = query.Where(f => !f.IsDeleted);
        }

        var feedback = await query.SingleOrDefaultAsync(cancellationToken);
        if (feedback != null)
        {
            await PopulateEmployeeNamesAsync(session, new[] { feedback }, cancellationToken);
        }
        return feedback;
    }

    public async Task<List<EmployeeFeedbackReadModel>> GetEmployeeFeedbackAsync(
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
        CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();

        var baseQuery = session.Query<EmployeeFeedbackReadModel>();

        // Apply pagination calculations
        var skip = (pageNumber - 1) * pageSize;

        // Build the complete query based on filters and sorting
        // This approach avoids intermediate query variable assignments which cause type issues with Marten

        var results = await ApplyFiltersAndSorting(
            session,
            baseQuery,
            employeeId,
            sourceType,
            fromDate,
            toDate,
            providerName,
            projectName,
            includeDeleted,
            currentFiscalYearOnly,
            sortField,
            sortAscending,
            skip,
            pageSize,
            cancellationToken);

        return results.ToList();
    }

    private async Task<List<EmployeeFeedbackReadModel>> ApplyFiltersAndSorting(
        IDocumentSession session,
        IQueryable<EmployeeFeedbackReadModel> baseQuery,
        Guid? employeeId,
        FeedbackSourceType? sourceType,
        DateTime? fromDate,
        DateTime? toDate,
        string? providerName,
        string? projectName,
        bool includeDeleted,
        bool currentFiscalYearOnly,
        string sortField,
        bool sortAscending,
        int skip,
        int pageSize,
        CancellationToken cancellationToken)
    {
        // Apply all filters in one go
        var query = baseQuery;

        if (!includeDeleted)
        {
            query = query.Where(f => !f.IsDeleted);
        }

        if (employeeId.HasValue)
        {
            var empId = employeeId.Value;
            query = query.Where(f => f.EmployeeId == empId);
        }

        if (sourceType.HasValue)
        {
            var srcType = sourceType.Value;
            query = query.Where(f => f.SourceType == srcType);
        }

        if (fromDate.HasValue)
        {
            var from = fromDate.Value;
            query = query.Where(f => f.FeedbackDate >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value;
            query = query.Where(f => f.FeedbackDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(providerName))
        {
            var providerNameLower = providerName.ToLower();
            query = query.Where(f => f.ProviderName.ToLower().Contains(providerNameLower));
        }

        if (!string.IsNullOrWhiteSpace(projectName))
        {
            var projectNameLower = projectName.ToLower();
            query = query.Where(f => f.ProjectName != null && f.ProjectName.ToLower().Contains(projectNameLower));
        }

        if (currentFiscalYearOnly)
        {
            var now = DateTime.UtcNow;
            var currentFiscalYear = now.Month >= 4 ? now.Year : now.Year - 1;
            query = query.Where(f => f.FiscalYear == currentFiscalYear);
        }

        // Apply sorting and pagination in one expression to return results directly
        var sortFieldLower = sortField.ToLower();
        IReadOnlyList<EmployeeFeedbackReadModel> results;

        if (sortFieldLower == "feedbackdate")
        {
            results = sortAscending
                ? await query.OrderBy(f => f.FeedbackDate).Skip(skip).Take(pageSize).ToListAsync(cancellationToken)
                : await query.OrderByDescending(f => f.FeedbackDate).Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        }
        else if (sortFieldLower == "recordeddate")
        {
            results = sortAscending
                ? await query.OrderBy(f => f.RecordedDate).Skip(skip).Take(pageSize).ToListAsync(cancellationToken)
                : await query.OrderByDescending(f => f.RecordedDate).Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        }
        else if (sortFieldLower == "employeename")
        {
            results = sortAscending
                ? await query.OrderBy(f => f.EmployeeName).Skip(skip).Take(pageSize).ToListAsync(cancellationToken)
                : await query.OrderByDescending(f => f.EmployeeName).Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        }
        else if (sortFieldLower == "providername")
        {
            results = sortAscending
                ? await query.OrderBy(f => f.ProviderName).Skip(skip).Take(pageSize).ToListAsync(cancellationToken)
                : await query.OrderByDescending(f => f.ProviderName).Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        }
        else if (sortFieldLower == "sourcetype")
        {
            results = sortAscending
                ? await query.OrderBy(f => f.SourceType).Skip(skip).Take(pageSize).ToListAsync(cancellationToken)
                : await query.OrderByDescending(f => f.SourceType).Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        }
        else if (sortFieldLower == "averagerating")
        {
            results = sortAscending
                ? await query.OrderBy(f => f.AverageRating ?? 0).Skip(skip).Take(pageSize).ToListAsync(cancellationToken)
                : await query.OrderByDescending(f => f.AverageRating ?? 0).Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        }
        else
        {
            // Default to FeedbackDate
            results = sortAscending
                ? await query.OrderBy(f => f.FeedbackDate).Skip(skip).Take(pageSize).ToListAsync(cancellationToken)
                : await query.OrderByDescending(f => f.FeedbackDate).Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        }

        var resultList = results.ToList();

        // Populate employee names
        await PopulateEmployeeNamesAsync(session, resultList, cancellationToken);

        // Handle employee name sorting if needed (re-sort in memory since we populated names after DB query)
        if (sortField.ToLower() == "employeename")
        {
            resultList = sortAscending
                ? resultList.OrderBy(f => f.EmployeeName).ToList()
                : resultList.OrderByDescending(f => f.EmployeeName).ToList();
        }

        return resultList;
    }

    public async Task<List<EmployeeFeedbackReadModel>> GetCurrentYearFeedbackAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        using var session = await store.LightweightSerializableSessionAsync();

        var now = DateTime.UtcNow;
        var currentFiscalYear = now.Month >= 4 ? now.Year : now.Year - 1;

        var results = await session.Query<EmployeeFeedbackReadModel>()
            .Where(f => f.EmployeeId == employeeId
                     && !f.IsDeleted
                     && f.FiscalYear == currentFiscalYear)
            .OrderBy(f => f.SourceType)
            .ThenByDescending(f => f.FeedbackDate)
            .ToListAsync(cancellationToken);

        // Populate employee names
        await PopulateEmployeeNamesAsync(session, results, cancellationToken);

        return results.ToList();
    }

    /// <summary>
    /// Helper method to populate employee names for a collection of feedback records
    /// </summary>
    private async Task PopulateEmployeeNamesAsync(
        IDocumentSession session,
        IEnumerable<EmployeeFeedbackReadModel> feedbackRecords,
        CancellationToken cancellationToken)
    {
        var feedbackList = feedbackRecords.ToList();
        if (!feedbackList.Any())
        {
            return;
        }

        // Get unique employee IDs from the results
        var allEmployeeIds = feedbackList
            .SelectMany(f => new Guid?[] { f.EmployeeId, f.RecordedByEmployeeId, f.LastModifiedByEmployeeId }
                .Where(id => id.HasValue).Select(id => id!.Value))
            .Distinct()
            .ToList();

        // Get all employee names in one query
        var employees = await session.Query<EmployeeReadModel>()
            .Where(e => allEmployeeIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        var employeeNameLookup = employees.ToDictionary(
            e => e.Id,
            e => new { FullName = $"{e.FirstName} {e.LastName}", e.FirstName, e.LastName });

        // Populate names in the feedback results
        foreach (var feedback in feedbackList)
        {
            if (employeeNameLookup.TryGetValue(feedback.EmployeeId, out var emp))
            {
                feedback.EmployeeName = emp.FullName;
                feedback.EmployeeFirstName = emp.FirstName;
                feedback.EmployeeLastName = emp.LastName;
            }

            if (employeeNameLookup.TryGetValue(feedback.RecordedByEmployeeId, out var recordedBy))
            {
                feedback.RecordedByEmployeeName = recordedBy.FullName;
            }

            if (feedback.LastModifiedByEmployeeId.HasValue &&
                employeeNameLookup.TryGetValue(feedback.LastModifiedByEmployeeId.Value, out var modifiedBy))
            {
                feedback.LastModifiedByEmployeeName = modifiedBy.FullName;
            }
        }
    }
}
