using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.EmployeeFeedbackQueries;

/// <summary>
/// Handler for GetEmployeeFeedbackQuery that retrieves employee feedback with filtering and pagination.
/// Returns feedback summaries matching the specified criteria.
/// </summary>
public class GetEmployeeFeedbackQueryHandler : IQueryHandler<GetEmployeeFeedbackQuery, Result<List<EmployeeFeedbackReadModel>>>
{
    private readonly IEmployeeFeedbackRepository repository;

    public GetEmployeeFeedbackQueryHandler(IEmployeeFeedbackRepository repository)
    {
        this.repository = repository;
    }

    public async Task<Result<List<EmployeeFeedbackReadModel>>> HandleAsync(GetEmployeeFeedbackQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate query parameters
            var validationResult = request.ValidateQuery();
            if (!validationResult.Succeeded)
            {
                return Result<List<EmployeeFeedbackReadModel>>.Fail(validationResult.Message ?? "Validation failed", validationResult.StatusCode);
            }

            // Query the read model database with all filters, pagination, and sorting
            var feedbackList = await repository.GetEmployeeFeedbackAsync(
                employeeId: request.EmployeeId,
                sourceType: request.SourceType,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                providerName: request.ProviderName,
                projectName: request.ProjectName,
                includeDeleted: request.IncludeDeleted,
                currentFiscalYearOnly: request.CurrentFiscalYearOnly,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                sortField: request.SortField,
                sortAscending: request.SortAscending,
                cancellationToken: cancellationToken);

            return Result<List<EmployeeFeedbackReadModel>>.Success(feedbackList);
        }
        catch (Exception ex)
        {
            return Result<List<EmployeeFeedbackReadModel>>.Fail($"Failed to get employee feedback: {ex.Message}", 500);
        }
    }
}