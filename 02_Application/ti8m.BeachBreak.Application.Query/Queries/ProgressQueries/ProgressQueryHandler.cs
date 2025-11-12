using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Projections;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.ProgressQueries;

public class ProgressQueryHandler :
    IQueryHandler<EmployeeProgressQuery, Result<IEnumerable<AssignmentProgress>>>
{
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;
    private readonly IQuestionnaireTemplateRepository templateRepository;
    private readonly IQuestionnaireResponseRepository responseRepository;
    private readonly ILogger<ProgressQueryHandler> logger;

    public ProgressQueryHandler(
        IQuestionnaireAssignmentRepository assignmentRepository,
        IQuestionnaireTemplateRepository templateRepository,
        IQuestionnaireResponseRepository responseRepository,
        ILogger<ProgressQueryHandler> logger)
    {
        this.assignmentRepository = assignmentRepository;
        this.templateRepository = templateRepository;
        this.responseRepository = responseRepository;
        this.logger = logger;
    }

    public async Task<Result<IEnumerable<AssignmentProgress>>> HandleAsync(EmployeeProgressQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving progress for employee {EmployeeId}", query.EmployeeId);

            // Get all assignments for this employee
            var assignmentReadModels = await assignmentRepository.GetAssignmentsByEmployeeIdAsync(query.EmployeeId, cancellationToken);
            var assignmentsList = assignmentReadModels.ToList();

            if (!assignmentsList.Any())
            {
                logger.LogInformation("No assignments found for employee {EmployeeId}", query.EmployeeId);
                return Result<IEnumerable<AssignmentProgress>>.Success(Enumerable.Empty<AssignmentProgress>());
            }

            // Get all templates referenced by assignments
            var templateIds = assignmentsList.Select(a => a.TemplateId).Distinct().ToList();
            var templates = await templateRepository.GetAllAsync(cancellationToken);
            var templateLookup = templates
                .Where(t => templateIds.Contains(t.Id))
                .ToDictionary(t => t.Id, t => t.Sections.Sum(s => s.Questions.Count));

            // Get all responses for these assignments
            var assignmentIds = assignmentsList.Select(a => a.Id).ToList();
            var responses = await responseRepository.GetByAssignmentIdsAsync(assignmentIds, cancellationToken);

            var responseLookup = responses.ToDictionary(r => r.AssignmentId);

            // Build progress list
            var progressList = new List<AssignmentProgress>();
            foreach (var assignment in assignmentsList)
            {
                var totalQuestions = templateLookup.TryGetValue(assignment.TemplateId, out var count) ? count : 0;

                AssignmentProgress progress;
                if (responseLookup.TryGetValue(assignment.Id, out var response))
                {
                    // Calculate answered questions by counting all role-based responses
                    var answeredQuestions = 0;
                    foreach (var sectionKvp in response.SectionResponses)
                    {
                        foreach (var roleKvp in sectionKvp.Value)
                        {
                            answeredQuestions += roleKvp.Value.Count;
                        }
                    }

                    var progressPercentage = totalQuestions > 0
                        ? (int)Math.Round((double)answeredQuestions / totalQuestions * 100)
                        : 0;

                    progress = new AssignmentProgress
                    {
                        AssignmentId = assignment.Id,
                        TemplateId = assignment.TemplateId,
                        ProgressPercentage = progressPercentage,
                        AnsweredQuestions = answeredQuestions,
                        TotalQuestions = totalQuestions,
                        LastModified = response.LastModified,
                        IsCompleted = assignment.CompletedDate.HasValue,
                        TimeSpent = null // TODO: Implement time tracking
                    };
                }
                else
                {
                    // No response yet - 0% progress
                    progress = new AssignmentProgress
                    {
                        AssignmentId = assignment.Id,
                        TemplateId = assignment.TemplateId,
                        ProgressPercentage = 0,
                        AnsweredQuestions = 0,
                        TotalQuestions = totalQuestions,
                        LastModified = assignment.AssignedDate,
                        IsCompleted = false,
                        TimeSpent = null
                    };
                }

                progressList.Add(progress);
            }

            logger.LogInformation("Retrieved progress for {AssignmentCount} assignments for employee {EmployeeId}",
                progressList.Count, query.EmployeeId);

            return Result<IEnumerable<AssignmentProgress>>.Success(progressList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving progress for employee {EmployeeId}", query.EmployeeId);
            return Result<IEnumerable<AssignmentProgress>>.Fail(
                $"Failed to retrieve employee progress: {ex.Message}",
                500);
        }
    }
}
