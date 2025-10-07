using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class GetTeamAssignmentsQueryHandler : IQueryHandler<GetTeamAssignmentsQuery, Result<IEnumerable<QuestionnaireAssignment>>>
{
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;
    private readonly IEmployeeRepository employeeRepository;
    private readonly ILogger<GetTeamAssignmentsQueryHandler> logger;

    public GetTeamAssignmentsQueryHandler(
        IQuestionnaireAssignmentRepository assignmentRepository,
        IEmployeeRepository employeeRepository,
        ILogger<GetTeamAssignmentsQueryHandler> logger)
    {
        this.assignmentRepository = assignmentRepository;
        this.employeeRepository = employeeRepository;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<QuestionnaireAssignment>>> HandleAsync(GetTeamAssignmentsQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting team assignments for manager {ManagerId}", query.ManagerId);

        try
        {
            // Get all team members for this manager
            var managerIdStr = query.ManagerId.ToString();
            var teamMembers = await employeeRepository.GetEmployeesByManagerIdAsync(managerIdStr, cancellationToken);
            var teamMemberIds = teamMembers.Where(e => !e.IsDeleted).Select(e => e.Id).ToList();

            if (!teamMemberIds.Any())
            {
                logger.LogInformation("No team members found for manager {ManagerId}", query.ManagerId);
                return Result<IEnumerable<QuestionnaireAssignment>>.Success(Enumerable.Empty<QuestionnaireAssignment>());
            }

            // Get all assignments for team members
            var allAssignments = new List<QuestionnaireAssignment>();
            foreach (var employeeId in teamMemberIds)
            {
                var employeeAssignments = await assignmentRepository.GetAssignmentsByEmployeeIdAsync(employeeId, cancellationToken);
                allAssignments.AddRange(employeeAssignments.Select(a => new QuestionnaireAssignment
                {
                    Id = a.Id,
                    TemplateId = a.TemplateId,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.EmployeeName,
                    EmployeeEmail = a.EmployeeEmail,
                    AssignedDate = a.AssignedDate,
                    DueDate = a.DueDate,
                    StartedDate = a.StartedDate,
                    CompletedDate = a.CompletedDate,
                    IsWithdrawn = a.IsWithdrawn,
                    WithdrawnDate = a.WithdrawnDate,
                    WithdrawnBy = a.WithdrawnBy,
                    WithdrawalReason = a.WithdrawalReason,
                    Status = a.Status,
                    AssignedBy = a.AssignedBy,
                    Notes = a.Notes,
                    TemplateName = string.Empty, // TODO: Fetch from template if needed
                    TemplateCategoryId = null, // TODO: Fetch from template if needed
                    WorkflowState = a.WorkflowState,
                    SectionProgress = a.SectionProgress?.Select(sp => new SectionProgressDto
                    {
                        SectionId = sp.SectionId,
                        IsEmployeeCompleted = sp.IsEmployeeCompleted,
                        IsManagerCompleted = sp.IsManagerCompleted,
                        EmployeeCompletedDate = sp.EmployeeCompletedDate,
                        ManagerCompletedDate = sp.ManagerCompletedDate
                    }).ToList() ?? new List<SectionProgressDto>(),
                    EmployeeConfirmedDate = a.EmployeeConfirmedDate,
                    EmployeeConfirmedBy = a.EmployeeConfirmedBy,
                    ManagerConfirmedDate = a.ManagerConfirmedDate,
                    ManagerConfirmedBy = a.ManagerConfirmedBy,
                    ReviewInitiatedDate = a.ReviewInitiatedDate,
                    ReviewInitiatedBy = a.ReviewInitiatedBy,
                    EmployeeReviewConfirmedDate = a.EmployeeReviewConfirmedDate,
                    EmployeeReviewConfirmedBy = a.EmployeeReviewConfirmedBy,
                    FinalizedDate = a.FinalizedDate,
                    FinalizedBy = a.FinalizedBy,
                    IsLocked = a.IsLocked
                }));
            }

            // Apply status filter if provided
            var filteredAssignments = query.FilterByStatus.HasValue
                ? allAssignments.Where(a => a.Status == query.FilterByStatus.Value)
                : allAssignments;

            logger.LogInformation("Retrieved {Count} team assignments for manager {ManagerId}", filteredAssignments.Count(), query.ManagerId);
            return Result<IEnumerable<QuestionnaireAssignment>>.Success(filteredAssignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve team assignments for manager {ManagerId}", query.ManagerId);
            return Result<IEnumerable<QuestionnaireAssignment>>.Fail($"Failed to retrieve team assignments: {ex.Message}", 500);
        }
    }
}
