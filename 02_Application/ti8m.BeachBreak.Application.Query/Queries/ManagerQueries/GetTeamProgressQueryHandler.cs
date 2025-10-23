using Marten;
using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Queries.ProgressQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Application.Query.Services;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class GetTeamProgressQueryHandler : IQueryHandler<GetTeamProgressQuery, Result<IEnumerable<AssignmentProgress>>>
{
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;
    private readonly IEmployeeRepository employeeRepository;
    private readonly IQueryDispatcher queryDispatcher;
    private readonly IProgressCalculationService progressCalculationService;
    private readonly IDocumentStore documentStore;
    private readonly ILogger<GetTeamProgressQueryHandler> logger;

    public GetTeamProgressQueryHandler(
        IQuestionnaireAssignmentRepository assignmentRepository,
        IEmployeeRepository employeeRepository,
        IQueryDispatcher queryDispatcher,
        IProgressCalculationService progressCalculationService,
        IDocumentStore documentStore,
        ILogger<GetTeamProgressQueryHandler> logger)
    {
        this.assignmentRepository = assignmentRepository;
        this.employeeRepository = employeeRepository;
        this.queryDispatcher = queryDispatcher;
        this.progressCalculationService = progressCalculationService;
        this.documentStore = documentStore;
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
                    // Calculate actual progress from responses using ReadModel
                    var progressPercentage = 0;
                    var totalQuestions = 0;
                    var answeredQuestions = 0;

                    try
                    {
                        // Load ReadModel to get typed SectionResponses for progress calculation
                        using var session = documentStore.LightweightSession();
                        var readModel = await session.Query<QuestionnaireResponseReadModel>()
                            .Where(r => r.AssignmentId == assignment.Id)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (readModel != null)
                        {
                            // Get template for progress calculation
                            var templateQuery = new QuestionnaireTemplateQuery(assignment.TemplateId);
                            var templateResult = await queryDispatcher.QueryAsync(templateQuery, cancellationToken);
                            var template = templateResult?.Payload;

                            if (template != null)
                            {
                                var progress = progressCalculationService.Calculate(template, readModel.SectionResponses);

                                // Use overall progress for manager view (includes both employee and manager sections)
                                progressPercentage = (int)Math.Round(progress.OverallProgress);
                                totalQuestions = progress.EmployeeTotalQuestions + progress.ManagerTotalQuestions;
                                answeredQuestions = progress.EmployeeAnsweredQuestions + progress.ManagerAnsweredQuestions;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to calculate progress for assignment {AssignmentId}, defaulting to 0", assignment.Id);
                    }

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
                        TotalQuestions = totalQuestions,
                        AnsweredQuestions = answeredQuestions,
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
