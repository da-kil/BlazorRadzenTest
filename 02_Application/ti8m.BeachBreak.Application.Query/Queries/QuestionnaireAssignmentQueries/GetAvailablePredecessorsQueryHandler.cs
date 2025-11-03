using ti8m.BeachBreak.Application.Query.Repositories;
using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;

/// <summary>
/// Query handler to retrieve available predecessor questionnaires for goal rating.
/// Returns finalized questionnaires for the same employee and category that have goals.
/// </summary>
public class GetAvailablePredecessorsQueryHandler
    : IQueryHandler<GetAvailablePredecessorsQuery, Result<IEnumerable<AvailablePredecessorDto>>>
{
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;
    private readonly IQuestionnaireTemplateRepository templateRepository;

    public GetAvailablePredecessorsQueryHandler(
        IQuestionnaireAssignmentRepository assignmentRepository,
        IQuestionnaireTemplateRepository templateRepository)
    {
        this.assignmentRepository = assignmentRepository;
        this.templateRepository = templateRepository;
    }

    public async Task<Result<IEnumerable<AvailablePredecessorDto>>> HandleAsync(
        GetAvailablePredecessorsQuery query,
        CancellationToken cancellationToken = default)
    {
        // Get current assignment to determine employee and category
        var currentAssignment = await assignmentRepository.GetAssignmentByIdAsync(
            query.AssignmentId, cancellationToken);

        if (currentAssignment == null)
        {
            return Result<IEnumerable<AvailablePredecessorDto>>.Fail(
                $"Assignment {query.AssignmentId} not found", 404);
        }

        // SECURITY: Validate that the assignment belongs to the requesting user
        if (currentAssignment.EmployeeId != query.RequestingUserId)
        {
            return Result<IEnumerable<AvailablePredecessorDto>>.Fail(
                "You do not have permission to access this assignment", 403);
        }

        // Get current template to determine category
        var currentTemplate = await templateRepository.GetByIdAsync(
            currentAssignment.TemplateId, cancellationToken);

        if (currentTemplate == null)
        {
            return Result<IEnumerable<AvailablePredecessorDto>>.Fail(
                $"Template {currentAssignment.TemplateId} not found", 404);
        }

        // Get all assignments for the requesting user (already validated as owner)
        var employeeAssignments = await assignmentRepository.GetAssignmentsByEmployeeIdAsync(
            query.RequestingUserId, cancellationToken);

        // Check if current assignment already has a predecessor linked for this question
        var isAlreadyLinked = currentAssignment.PredecessorLinksByQuestion.ContainsKey(query.QuestionId);
        if (isAlreadyLinked)
        {
            // Return empty list - no need to show options when already linked
            return Result<IEnumerable<AvailablePredecessorDto>>.Success(new List<AvailablePredecessorDto>());
        }

        // Filter for available predecessors
        var availablePredecessors = new List<AvailablePredecessorDto>();

        foreach (var assignment in employeeAssignments)
        {
            // Skip the current assignment
            if (assignment.Id == query.AssignmentId)
                continue;

            // Business rule: Must be finalized
            if (assignment.WorkflowState != WorkflowState.Finalized)
                continue;

            // Get template to check category
            var template = await templateRepository.GetByIdAsync(
                assignment.TemplateId, cancellationToken);

            if (template == null)
                continue;

            // Business rule: Must be same category
            if (template.CategoryId != currentTemplate.CategoryId)
                continue;

            // Business rule: Must have goals for the specified question
            // Check if this assignment has any goals for the question
            if (!assignment.GoalsByQuestion.TryGetValue(query.QuestionId, out var goals) ||
                goals == null ||
                !goals.Any())
            {
                continue;
            }

            // Add to available predecessors
            availablePredecessors.Add(new AvailablePredecessorDto
            {
                AssignmentId = assignment.Id,
                TemplateName = template.Name,
                AssignedDate = assignment.AssignedDate,
                CompletedDate = assignment.CompletedDate,
                GoalCount = goals.Count
            });
        }

        // Order by completion date descending (most recent first)
        var orderedPredecessors = availablePredecessors
            .OrderByDescending(p => p.CompletedDate)
            .ToList();

        return Result<IEnumerable<AvailablePredecessorDto>>.Success(orderedPredecessors);
    }
}
