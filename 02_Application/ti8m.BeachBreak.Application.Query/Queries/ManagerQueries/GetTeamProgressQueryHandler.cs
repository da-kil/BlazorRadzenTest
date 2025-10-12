using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Queries.ProgressQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class GetTeamProgressQueryHandler : IQueryHandler<GetTeamProgressQuery, Result<IEnumerable<AssignmentProgress>>>
{
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;
    private readonly IEmployeeRepository employeeRepository;
    private readonly ILogger<GetTeamProgressQueryHandler> logger;

    public GetTeamProgressQueryHandler(
        IQuestionnaireAssignmentRepository assignmentRepository,
        IEmployeeRepository employeeRepository,
        ILogger<GetTeamProgressQueryHandler> logger)
    {
        this.assignmentRepository = assignmentRepository;
        this.employeeRepository = employeeRepository;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<AssignmentProgress>>> HandleAsync(GetTeamProgressQuery query, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting team progress for manager {ManagerId}", query.ManagerId);

        try
        {
            // Get all team members for this manager
            var managerIdStr = query.ManagerId.ToString();
            var teamMembers = await employeeRepository.GetEmployeesByManagerIdAsync(managerIdStr, cancellationToken);
            var teamMemberIds = teamMembers.Where(e => !e.IsDeleted).Select(e => e.Id).ToList();

            if (!teamMemberIds.Any())
            {
                logger.LogInformation("No team members found for manager {ManagerId}", query.ManagerId);
                return Result<IEnumerable<AssignmentProgress>>.Success(Enumerable.Empty<AssignmentProgress>());
            }

            // Get all assignments for team members
            var allProgress = new List<AssignmentProgress>();
            foreach (var employeeId in teamMemberIds)
            {
                var employeeAssignments = await assignmentRepository.GetAssignmentsByEmployeeIdAsync(employeeId, cancellationToken);

                foreach (var assignment in employeeAssignments.Where(a => !a.IsWithdrawn))
                {
                    // TODO: Calculate actual progress from responses when IQuestionnaireResponseRepository is available
                    // For now, use section progress from assignment read model
                    var totalSections = assignment.SectionProgress?.Count ?? 0;
                    var completedSections = assignment.SectionProgress?.Count(sp => sp.IsEmployeeCompleted) ?? 0;

                    var progressPercentage = totalSections > 0
                        ? (int)((double)completedSections / totalSections * 100)
                        : 0;

                    var isCompleted = assignment.WorkflowState == WorkflowState.Finalized;

                    var timeSpent = assignment.StartedDate.HasValue && assignment.CompletedDate.HasValue
                        ? assignment.CompletedDate.Value - assignment.StartedDate.Value
                        : assignment.StartedDate.HasValue
                            ? DateTime.Now - assignment.StartedDate.Value
                            : (TimeSpan?)null;

                    allProgress.Add(new AssignmentProgress
                    {
                        AssignmentId = assignment.Id,
                        TemplateId = assignment.TemplateId,
                        ProgressPercentage = progressPercentage,
                        TotalQuestions = totalSections, // Using sections as proxy for questions
                        AnsweredQuestions = completedSections,
                        LastModified = assignment.StartedDate ?? assignment.AssignedDate,
                        IsCompleted = isCompleted,
                        TimeSpent = timeSpent
                    });
                }
            }

            logger.LogInformation("Retrieved progress for {Count} assignments for manager {ManagerId}", allProgress.Count, query.ManagerId);
            return Result<IEnumerable<AssignmentProgress>>.Success(allProgress);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve team progress for manager {ManagerId}", query.ManagerId);
            return Result<IEnumerable<AssignmentProgress>>.Fail($"Failed to retrieve team progress: {ex.Message}", 500);
        }
    }
}
