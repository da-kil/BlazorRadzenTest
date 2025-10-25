using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;
using ti8m.BeachBreak.Application.Query.Queries;
using ti8m.BeachBreak.Application.Query.Queries.EmployeeQueries;
using ti8m.BeachBreak.Application.Query.Queries.QuestionnaireTemplateQueries;
using ti8m.BeachBreak.CommandApi.Authorization;
using ti8m.BeachBreak.CommandApi.Dto;
using ti8m.BeachBreak.Core.Infrastructure.Authorization;
using ti8m.BeachBreak.Core.Infrastructure.Contexts;
using ti8m.BeachBreak.Domain.EmployeeAggregate;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/assignments")]
[Authorize]
public class AssignmentsController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly IQueryDispatcher queryDispatcher;
    private readonly UserContext userContext;
    private readonly ILogger<AssignmentsController> logger;
    private readonly IManagerAuthorizationService authorizationService;
    private readonly IAuthorizationCacheService authorizationCacheService;

    public AssignmentsController(
        ICommandDispatcher commandDispatcher,
        IQueryDispatcher queryDispatcher,
        UserContext userContext,
        ILogger<AssignmentsController> logger,
        IManagerAuthorizationService authorizationService,
        IAuthorizationCacheService authorizationCacheService)
    {
        this.commandDispatcher = commandDispatcher;
        this.queryDispatcher = queryDispatcher;
        this.userContext = userContext;
        this.logger = logger;
        this.authorizationService = authorizationService;
        this.authorizationCacheService = authorizationCacheService;
    }

    /// <summary>
    /// Creates bulk assignments for any employees. HR/Admin only.
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Roles = "HR,HRLead,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateBulkAssignments([FromBody] CreateBulkAssignmentsDto bulkAssignmentDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (bulkAssignmentDto.EmployeeAssignments == null || !bulkAssignmentDto.EmployeeAssignments.Any())
                return BadRequest("At least one employee assignment is required");

            // Load template to get RequiresManagerReview flag
            var templateResult = await queryDispatcher.QueryAsync(
                new QuestionnaireTemplateQuery(bulkAssignmentDto.TemplateId),
                HttpContext.RequestAborted);

            if (templateResult?.Succeeded != true || templateResult.Payload == null)
            {
                return BadRequest($"Template {bulkAssignmentDto.TemplateId} not found");
            }

            // Get current user's name from UserContext
            var assignedBy = "";
            if (Guid.TryParse(userContext.Id, out var userId))
            {
                var employeeResult = await queryDispatcher.QueryAsync(new EmployeeQuery(userId), HttpContext.RequestAborted);
                if (employeeResult?.Succeeded == true && employeeResult.Payload != null)
                {
                    assignedBy = $"{employeeResult.Payload.FirstName} {employeeResult.Payload.LastName}";
                    logger.LogInformation("Set AssignedBy to {AssignedBy} from user context {UserId}", assignedBy, userId);
                }
            }

            var employeeAssignments = bulkAssignmentDto.EmployeeAssignments
                .Select(e => new EmployeeAssignmentData(
                    e.EmployeeId,
                    e.EmployeeName,
                    e.EmployeeEmail))
                .ToList();

            var command = new CreateBulkAssignmentsCommand(
                bulkAssignmentDto.TemplateId,
                templateResult.Payload.RequiresManagerReview,
                employeeAssignments,
                bulkAssignmentDto.DueDate,
                assignedBy,
                bulkAssignmentDto.Notes);

            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating bulk assignments");
            return StatusCode(500, "An error occurred while creating bulk assignments");
        }
    }

    /// <summary>
    /// Creates bulk assignments for a manager's direct reports only. TeamLead role.
    /// Validates that all employees are direct reports of the authenticated manager.
    /// </summary>
    [HttpPost("manager/bulk")]
    [Authorize(Roles = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateManagerBulkAssignments([FromBody] CreateBulkAssignmentsDto bulkAssignmentDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (bulkAssignmentDto.EmployeeAssignments == null || !bulkAssignmentDto.EmployeeAssignments.Any())
                return BadRequest("At least one employee assignment is required");

            // Load template to get RequiresManagerReview flag
            var templateResult = await queryDispatcher.QueryAsync(
                new QuestionnaireTemplateQuery(bulkAssignmentDto.TemplateId),
                HttpContext.RequestAborted);

            if (templateResult?.Succeeded != true || templateResult.Payload == null)
            {
                return BadRequest($"Template {bulkAssignmentDto.TemplateId} not found");
            }

            // Get authenticated manager ID using authorization service
            Guid managerId;
            try
            {
                managerId = await authorizationService.GetCurrentManagerIdAsync();
                logger.LogInformation("Manager {ManagerId} attempting to create {Count} assignments",
                    managerId, bulkAssignmentDto.EmployeeAssignments.Count);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("CreateManagerBulkAssignments failed: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }

            // Validate all employee IDs are direct reports using authorization service
            var employeeIds = bulkAssignmentDto.EmployeeAssignments.Select(e => e.EmployeeId).ToList();
            var areAllDirectReports = await authorizationService.AreAllDirectReportsAsync(managerId, employeeIds);

            if (!areAllDirectReports)
            {
                logger.LogWarning("Manager {ManagerId} attempted to assign to employees who are not direct reports", managerId);
                return Forbid();
            }

            // Get current user's name from database
            var assignedBy = "";
            var employeeResult = await queryDispatcher.QueryAsync(new EmployeeQuery(managerId), HttpContext.RequestAborted);
            if (employeeResult?.Succeeded == true && employeeResult.Payload != null)
            {
                assignedBy = $"{employeeResult.Payload.FirstName} {employeeResult.Payload.LastName}";
                logger.LogInformation("Set AssignedBy to {AssignedBy} for manager {ManagerId}", assignedBy, managerId);
            }

            var employeeAssignments = bulkAssignmentDto.EmployeeAssignments
                .Select(e => new EmployeeAssignmentData(
                    e.EmployeeId,
                    e.EmployeeName,
                    e.EmployeeEmail))
                .ToList();

            var command = new CreateBulkAssignmentsCommand(
                bulkAssignmentDto.TemplateId,
                templateResult.Payload.RequiresManagerReview,
                employeeAssignments,
                bulkAssignmentDto.DueDate,
                assignedBy,
                bulkAssignmentDto.Notes);

            var result = await commandDispatcher.SendAsync(command);

            if (result.Succeeded)
            {
                logger.LogInformation("Manager {ManagerId} successfully created {Count} assignments",
                    managerId, bulkAssignmentDto.EmployeeAssignments.Count);
            }

            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating manager bulk assignments");
            return StatusCode(500, "An error occurred while creating bulk assignments");
        }
    }

    [HttpPost("{assignmentId}/start")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> StartAssignmentWork(Guid assignmentId)
    {
        try
        {
            var command = new StartAssignmentWorkCommand(assignmentId);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting assignment work");
            return StatusCode(500, "An error occurred while starting assignment work");
        }
    }

    [HttpPost("{assignmentId}/complete")]
    public async Task<IActionResult> CompleteAssignmentWork(Guid assignmentId)
    {
        try
        {
            var command = new CompleteAssignmentWorkCommand(assignmentId);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing assignment work");
            return StatusCode(500, "An error occurred while completing assignment work");
        }
    }

    /// <summary>
    /// Extends the due date of an assignment.
    /// Managers can only extend assignments for their direct reports.
    /// HR/Admin can extend any assignment.
    /// </summary>
    [HttpPost("extend-due-date")]
    [Authorize(Roles = "TeamLead,HR,HRLead,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExtendAssignmentDueDate([FromBody] ExtendAssignmentDueDateDto extendDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check authorization - only apply manager restrictions if user doesn't have elevated HR/Admin roles
            Guid managerId;
            try
            {
                managerId = await authorizationService.GetCurrentManagerIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("Authorization failed: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }

            var hasElevatedRole = await HasElevatedRoleAsync(managerId);
            if (!hasElevatedRole)
            {
                var canAccess = await authorizationService.CanAccessAssignmentAsync(managerId, extendDto.AssignmentId);

                if (!canAccess)
                {
                    logger.LogWarning("Manager {ManagerId} attempted to extend assignment {AssignmentId} for non-direct report",
                        managerId, extendDto.AssignmentId);
                    return Forbid();
                }
            }

            var command = new ExtendAssignmentDueDateCommand(
                extendDto.AssignmentId,
                extendDto.NewDueDate,
                extendDto.ExtensionReason);

            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extending assignment due date");
            return StatusCode(500, "An error occurred while extending due date");
        }
    }

    /// <summary>
    /// Withdraws (cancels) an assignment.
    /// Managers can only withdraw assignments for their direct reports.
    /// HR/Admin can withdraw any assignment.
    /// </summary>
    [HttpPost("withdraw")]
    [Authorize(Roles = "TeamLead,HR,HRLead,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> WithdrawAssignment([FromBody] WithdrawAssignmentDto withdrawDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check authorization - only apply manager restrictions if user doesn't have elevated HR/Admin roles
            Guid managerId;
            try
            {
                managerId = await authorizationService.GetCurrentManagerIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning("Authorization failed: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }

            var hasElevatedRole = await HasElevatedRoleAsync(managerId);
            if (!hasElevatedRole)
            {
                var canAccess = await authorizationService.CanAccessAssignmentAsync(managerId, withdrawDto.AssignmentId);

                if (!canAccess)
                {
                    logger.LogWarning("Manager {ManagerId} attempted to withdraw assignment {AssignmentId} for non-direct report",
                        managerId, withdrawDto.AssignmentId);
                    return Forbid();
                }
            }

            var command = new WithdrawAssignmentCommand(
                withdrawDto.AssignmentId,
                managerId,
                withdrawDto.WithdrawalReason);

            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error withdrawing assignment");
            return StatusCode(500, "An error occurred while withdrawing assignment");
        }
    }

    // Workflow endpoints
    [HttpPost("{assignmentId}/sections/{sectionId}/complete-employee")]
    public async Task<IActionResult> CompleteSectionAsEmployee(Guid assignmentId, Guid sectionId)
    {
        try
        {
            var command = new CompleteSectionAsEmployeeCommand(assignmentId, sectionId);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing section as employee");
            return StatusCode(500, "An error occurred while completing section");
        }
    }

    [HttpPost("{assignmentId}/sections/bulk-complete-employee")]
    public async Task<IActionResult> CompleteBulkSectionsAsEmployee(Guid assignmentId, [FromBody] List<Guid> sectionIds)
    {
        try
        {
            if (sectionIds == null || !sectionIds.Any())
                return BadRequest("Section IDs list cannot be null or empty");

            var command = new CompleteBulkSectionsAsEmployeeCommand(assignmentId, sectionIds);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing sections as employee");
            return StatusCode(500, "An error occurred while completing sections");
        }
    }

    [HttpPost("{assignmentId}/sections/{sectionId}/complete-manager")]
    [Authorize(Roles = "TeamLead")]
    public async Task<IActionResult> CompleteSectionAsManager(Guid assignmentId, Guid sectionId)
    {
        try
        {
            var command = new CompleteSectionAsManagerCommand(assignmentId, sectionId);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing section as manager");
            return StatusCode(500, "An error occurred while completing section");
        }
    }

    [HttpPost("{assignmentId}/sections/bulk-complete-manager")]
    [Authorize(Roles = "TeamLead")]
    public async Task<IActionResult> CompleteBulkSectionsAsManager(Guid assignmentId, [FromBody] List<Guid> sectionIds)
    {
        try
        {
            if (sectionIds == null || !sectionIds.Any())
                return BadRequest("Section IDs list cannot be null or empty");

            var command = new CompleteBulkSectionsAsManagerCommand(assignmentId, sectionIds);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing sections as manager");
            return StatusCode(500, "An error occurred while completing sections");
        }
    }

    [HttpPost("{assignmentId}/submit-employee")]
    public async Task<IActionResult> SubmitEmployeeQuestionnaire(Guid assignmentId, [FromBody] SubmitQuestionnaireDto submitDto)
    {
        try
        {
            // Get employee ID from authenticated user context
            if (!Guid.TryParse(userContext.Id, out var employeeId))
            {
                logger.LogWarning("SubmitEmployeeQuestionnaire failed: Unable to parse user ID from context");
                return Unauthorized("User ID not found in authentication context");
            }

            var command = new SubmitEmployeeQuestionnaireCommand(
                assignmentId,
                employeeId,
                submitDto.ExpectedVersion);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting employee questionnaire");
            return StatusCode(500, "An error occurred while submitting employee questionnaire");
        }
    }

    [HttpPost("{assignmentId}/submit-manager")]
    [Authorize(Roles = "TeamLead")]
    public async Task<IActionResult> SubmitManagerQuestionnaire(Guid assignmentId, [FromBody] SubmitQuestionnaireDto submitDto)
    {
        try
        {
            // Get manager ID from authenticated user context
            if (!Guid.TryParse(userContext.Id, out var managerId))
            {
                logger.LogWarning("SubmitManagerQuestionnaire failed: Unable to parse user ID from context");
                return Unauthorized("User ID not found in authentication context");
            }

            var command = new SubmitManagerQuestionnaireCommand(
                assignmentId,
                managerId,
                submitDto.ExpectedVersion);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting manager questionnaire");
            return StatusCode(500, "An error occurred while submitting manager questionnaire");
        }
    }

    [HttpPost("{assignmentId}/initiate-review")]
    [Authorize(Roles = "TeamLead")]
    public async Task<IActionResult> InitiateReview(Guid assignmentId)
    {
        try
        {
            // Get manager ID from authenticated user context
            if (!Guid.TryParse(userContext.Id, out var managerId))
            {
                logger.LogWarning("InitiateReview failed: Unable to parse user ID from context");
                return Unauthorized("User ID not found in authentication context");
            }

            var command = new InitiateReviewCommand(assignmentId, managerId);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initiating review");
            return StatusCode(500, "An error occurred while initiating review");
        }
    }

    [HttpPost("{assignmentId}/edit-answer")]
    public async Task<IActionResult> EditAnswerDuringReview(Guid assignmentId, [FromBody] EditAnswerDto editDto)
    {
        try
        {
            if (!Enum.TryParse<Domain.QuestionnaireTemplateAggregate.CompletionRole>(editDto.OriginalCompletionRole, out var completionRole))
                return BadRequest("Invalid CompletionRole value");

            // Get user ID from authenticated user context
            if (!Guid.TryParse(userContext.Id, out var userId))
            {
                logger.LogWarning("EditAnswerDuringReview failed: Unable to parse user ID from context");
                return Unauthorized("User ID not found in authentication context");
            }

            var command = new EditAnswerDuringReviewCommand(
                assignmentId,
                editDto.SectionId,
                editDto.QuestionId,
                completionRole,
                editDto.Answer,
                userId);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error editing answer during review");
            return StatusCode(500, "An error occurred while editing answer");
        }
    }

    // Refined review workflow endpoints
    /// <summary>
    /// Manager finishes the review meeting.
    /// Transitions from InReview to ManagerReviewConfirmed state.
    /// </summary>
    [HttpPost("{assignmentId}/review/finish")]
    [Authorize(Roles = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FinishReviewMeeting(Guid assignmentId, [FromBody] FinishReviewMeetingDto finishDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get manager ID from authenticated user context
            if (!Guid.TryParse(userContext.Id, out var managerId))
            {
                logger.LogWarning("FinishReviewMeeting failed: Unable to parse user ID from context");
                return Unauthorized("User ID not found in authentication context");
            }

            var command = new FinishReviewMeetingCommand(
                assignmentId,
                managerId,
                finishDto.ReviewSummary,
                finishDto.ExpectedVersion);

            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finishing review meeting for assignment {AssignmentId}", assignmentId);
            return StatusCode(500, "An error occurred while finishing review meeting");
        }
    }

    /// <summary>
    /// Employee confirms the review outcome.
    /// Employee cannot reject but can add comments.
    /// Transitions from ManagerReviewConfirmed to EmployeeReviewConfirmed state.
    /// </summary>
    [HttpPost("{assignmentId}/review/confirm-employee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmReviewOutcomeAsEmployee(Guid assignmentId, [FromBody] ConfirmReviewOutcomeDto confirmDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get employee ID from authenticated user context
            if (!Guid.TryParse(userContext.Id, out var employeeId))
            {
                logger.LogWarning("ConfirmReviewOutcomeAsEmployee failed: Unable to parse user ID from context");
                return Unauthorized("User ID not found in authentication context");
            }

            var command = new ConfirmReviewOutcomeAsEmployeeCommand(
                assignmentId,
                employeeId,
                confirmDto.EmployeeComments,
                confirmDto.ExpectedVersion);

            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming review outcome for assignment {AssignmentId}", assignmentId);
            return StatusCode(500, "An error occurred while confirming review outcome");
        }
    }

    /// <summary>
    /// Manager finalizes the questionnaire after employee confirmation.
    /// This is the final step in the review process.
    /// Transitions from EmployeeReviewConfirmed to Finalized state.
    /// </summary>
    [HttpPost("{assignmentId}/review/finalize-manager")]
    [Authorize(Roles = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FinalizeQuestionnaireAsManager(Guid assignmentId, [FromBody] FinalizeAsManagerDto finalizeDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get manager ID from authenticated user context
            if (!Guid.TryParse(userContext.Id, out var managerId))
            {
                logger.LogWarning("FinalizeQuestionnaireAsManager failed: Unable to parse user ID from context");
                return Unauthorized("User ID not found in authentication context");
            }

            var command = new FinalizeQuestionnaireAsManagerCommand(
                assignmentId,
                managerId,
                finalizeDto.ManagerFinalNotes,
                finalizeDto.ExpectedVersion);

            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finalizing questionnaire as manager for assignment {AssignmentId}", assignmentId);
            return StatusCode(500, "An error occurred while finalizing questionnaire");
        }
    }

    [HttpPost("reminder")]
    [Authorize(Roles = "TeamLead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendReminder([FromBody] SendReminderDto reminderDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get manager ID from authenticated user context
            if (!Guid.TryParse(userContext.Id, out var managerId))
            {
                logger.LogWarning("SendReminder failed: Unable to parse user ID from context");
                return Unauthorized("User ID not found in authentication context");
            }

            var command = new SendAssignmentReminderCommand(
                reminderDto.AssignmentId,
                reminderDto.Message,
                managerId);

            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending reminder for assignment {AssignmentId}", reminderDto.AssignmentId);
            return StatusCode(500, "An error occurred while sending reminder");
        }
    }

    /// <summary>
    /// Checks if the current user has an elevated role (HR, HRLead, or Admin).
    /// Returns true if elevated, false if user is only TeamLead/Employee.
    /// </summary>
    private async Task<bool> HasElevatedRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var employeeRole = await authorizationCacheService.GetEmployeeRoleCacheAsync<EmployeeRoleResult>(userId, cancellationToken);
        if (employeeRole == null)
        {
            logger.LogWarning("Unable to retrieve employee role for user {UserId}", userId);
            return false;
        }

        return employeeRole.ApplicationRole == ApplicationRole.HR ||
               employeeRole.ApplicationRole == ApplicationRole.HRLead ||
               employeeRole.ApplicationRole == ApplicationRole.Admin;
    }

    /// <summary>
    /// Reopens a questionnaire assignment for corrections.
    /// Requires Admin, HR, or TeamLead role.
    /// TeamLead can only reopen questionnaires for their own team members.
    /// Finalized questionnaires cannot be reopened - create new assignment instead.
    /// Sends email notifications to affected parties.
    /// </summary>
    /// <param name="assignmentId">The assignment to reopen</param>
    /// <param name="reopenDto">Target state and reason for reopening</param>
    [HttpPost("{assignmentId}/reopen")]
    [Authorize(Roles = "TeamLead,HR,HRLead,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReopenQuestionnaire(
        Guid assignmentId,
        [FromBody] ReopenQuestionnaireDto reopenDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get user ID from authenticated user context
            if (!Guid.TryParse(userContext.Id, out var userId))
            {
                logger.LogWarning("ReopenQuestionnaire failed: Unable to parse user ID from context");
                return Unauthorized("User ID not found in authentication context");
            }

            // Get user role from cache
            var employeeRole = await authorizationCacheService.GetEmployeeRoleCacheAsync<EmployeeRoleResult>(
                userId,
                HttpContext.RequestAborted);

            if (employeeRole == null)
            {
                logger.LogWarning("ReopenQuestionnaire failed: Unable to retrieve employee role for user {UserId}", userId);
                return Unauthorized("User role not found");
            }

            // Map ApplicationRole enum to string for command
            var roleString = employeeRole.ApplicationRole.ToString();

            var command = new ReopenQuestionnaireCommand(
                assignmentId,
                reopenDto.TargetState,
                reopenDto.ReopenReason,
                userId,
                roleString);

            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reopening assignment {AssignmentId}", assignmentId);
            return StatusCode(500, "An error occurred while reopening questionnaire");
        }
    }

}