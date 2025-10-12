using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireAssignmentQueries;
using ti8m.BeachBreak.Application.Query.Repositories;

namespace ti8m.BeachBreak.Application.Query.Queries.ManagerQueries;

public class GetTeamAssignmentsQueryHandler : IQueryHandler<GetTeamAssignmentsQuery, Result<IEnumerable<QuestionnaireAssignment>>>
{
    private readonly IQuestionnaireAssignmentRepository assignmentRepository;
    private readonly IEmployeeRepository employeeRepository;
    private readonly IQuestionnaireTemplateRepository templateRepository;
    private readonly ILogger<GetTeamAssignmentsQueryHandler> logger;

    public GetTeamAssignmentsQueryHandler(
        IQuestionnaireAssignmentRepository assignmentRepository,
        IEmployeeRepository employeeRepository,
        IQuestionnaireTemplateRepository templateRepository,
        ILogger<GetTeamAssignmentsQueryHandler> logger)
    {
        this.assignmentRepository = assignmentRepository;
        this.employeeRepository = employeeRepository;
        this.templateRepository = templateRepository;
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
            var allReadModels = new List<Projections.QuestionnaireAssignmentReadModel>();
            foreach (var employeeId in teamMemberIds)
            {
                var employeeAssignments = await assignmentRepository.GetAssignmentsByEmployeeIdAsync(employeeId, cancellationToken);
                allReadModels.AddRange(employeeAssignments);
            }

            // Enrich with template metadata
            var enrichedAssignments = await EnrichWithTemplateMetadataAsync(allReadModels, cancellationToken);

            // Apply workflow state filter if provided
            var filteredAssignments = query.FilterByWorkflowState.HasValue
                ? enrichedAssignments.Where(a => a.WorkflowState == query.FilterByWorkflowState.Value)
                : enrichedAssignments;

            logger.LogInformation("Retrieved {Count} team assignments for manager {ManagerId}", filteredAssignments.Count(), query.ManagerId);
            return Result<IEnumerable<QuestionnaireAssignment>>.Success(filteredAssignments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve team assignments for manager {ManagerId}", query.ManagerId);
            return Result<IEnumerable<QuestionnaireAssignment>>.Fail($"Failed to retrieve team assignments: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Enriches assignments with template metadata (name and category).
    /// Only fetches templates referenced by the assignments to minimize data transfer.
    /// </summary>
    private async Task<IEnumerable<QuestionnaireAssignment>> EnrichWithTemplateMetadataAsync(
        IEnumerable<Projections.QuestionnaireAssignmentReadModel> readModels,
        CancellationToken cancellationToken)
    {
        var readModelsList = readModels.ToList();
        if (!readModelsList.Any())
        {
            return Enumerable.Empty<QuestionnaireAssignment>();
        }

        // Get unique template IDs from assignments
        var templateIds = readModelsList.Select(a => a.TemplateId).Distinct().ToList();

        // Fetch only the templates we need
        var templates = await templateRepository.GetAllAsync(cancellationToken);
        var templateLookup = templates
            .Where(t => templateIds.Contains(t.Id))
            .ToDictionary(t => t.Id, t => (t.Name, t.CategoryId));

        // Map and enrich
        return readModelsList.Select(readModel =>
        {
            var assignment = MapToQuestionnaireAssignment(readModel);

            // Denormalize template metadata
            if (templateLookup.TryGetValue(readModel.TemplateId, out var templateMetadata))
            {
                assignment.TemplateName = templateMetadata.Name;
                assignment.TemplateCategoryId = templateMetadata.CategoryId;
            }
            else
            {
                // Template not found - log warning but don't fail
                logger.LogWarning("Template {TemplateId} not found for assignment {AssignmentId}",
                    readModel.TemplateId, readModel.Id);
                assignment.TemplateName = "Unknown Template";
                assignment.TemplateCategoryId = null;
            }

            return assignment;
        });
    }

    private static QuestionnaireAssignment MapToQuestionnaireAssignment(Projections.QuestionnaireAssignmentReadModel readModel)
    {
        return new QuestionnaireAssignment
        {
            Id = readModel.Id,
            TemplateId = readModel.TemplateId,
            EmployeeId = readModel.EmployeeId,
            EmployeeName = readModel.EmployeeName,
            EmployeeEmail = readModel.EmployeeEmail,
            AssignedDate = readModel.AssignedDate,
            DueDate = readModel.DueDate,
            StartedDate = readModel.StartedDate,
            CompletedDate = readModel.CompletedDate,
            IsWithdrawn = readModel.IsWithdrawn,
            WithdrawnDate = readModel.WithdrawnDate,
            WithdrawnBy = readModel.WithdrawnBy,
            WithdrawalReason = readModel.WithdrawalReason,
            AssignedBy = readModel.AssignedBy,
            Notes = readModel.Notes,
            WorkflowState = readModel.WorkflowState,
            SectionProgress = readModel.SectionProgress?.Select(sp => new SectionProgressDto
            {
                SectionId = sp.SectionId,
                IsEmployeeCompleted = sp.IsEmployeeCompleted,
                IsManagerCompleted = sp.IsManagerCompleted,
                EmployeeCompletedDate = sp.EmployeeCompletedDate,
                ManagerCompletedDate = sp.ManagerCompletedDate
            }).ToList() ?? new List<SectionProgressDto>(),
            EmployeeSubmittedDate = readModel.EmployeeSubmittedDate,
            EmployeeSubmittedBy = readModel.EmployeeSubmittedBy,
            ManagerSubmittedDate = readModel.ManagerSubmittedDate,
            ManagerSubmittedBy = readModel.ManagerSubmittedBy,
            ManagerReviewConfirmedDate = readModel.ManagerReviewConfirmedDate,
            ManagerReviewConfirmedBy = readModel.ManagerReviewConfirmedBy,
            ReviewInitiatedDate = readModel.ReviewInitiatedDate,
            ReviewInitiatedBy = readModel.ReviewInitiatedBy,
            EmployeeReviewConfirmedDate = readModel.EmployeeReviewConfirmedDate,
            EmployeeReviewConfirmedBy = readModel.EmployeeReviewConfirmedBy,
            FinalizedDate = readModel.FinalizedDate,
            FinalizedBy = readModel.FinalizedBy,
            IsLocked = readModel.IsLocked
        };
    }
}
