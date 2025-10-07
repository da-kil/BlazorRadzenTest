using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;
using ti8m.BeachBreak.CommandApi.Authorization;
using ti8m.BeachBreak.CommandApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/assignments")]
[Authorize]
public class AssignmentsController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly ILogger<AssignmentsController> logger;
    private readonly IManagerAuthorizationService authorizationService;

    public AssignmentsController(
        ICommandDispatcher commandDispatcher,
        ILogger<AssignmentsController> logger,
        IManagerAuthorizationService authorizationService)
    {
        this.commandDispatcher = commandDispatcher;
        this.logger = logger;
        this.authorizationService = authorizationService;
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

            var employeeAssignments = bulkAssignmentDto.EmployeeAssignments
                .Select(e => new EmployeeAssignmentData(
                    e.EmployeeId,
                    e.EmployeeName,
                    e.EmployeeEmail))
                .ToList();

            var command = new CreateBulkAssignmentsCommand(
                bulkAssignmentDto.TemplateId,
                employeeAssignments,
                bulkAssignmentDto.DueDate,
                bulkAssignmentDto.AssignedBy,
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

            var employeeAssignments = bulkAssignmentDto.EmployeeAssignments
                .Select(e => new EmployeeAssignmentData(
                    e.EmployeeId,
                    e.EmployeeName,
                    e.EmployeeEmail))
                .ToList();

            var command = new CreateBulkAssignmentsCommand(
                bulkAssignmentDto.TemplateId,
                employeeAssignments,
                bulkAssignmentDto.DueDate,
                bulkAssignmentDto.AssignedBy, // Will be set by backend if null
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

    [HttpPost("extend-due-date")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> ExtendAssignmentDueDate([FromBody] ExtendAssignmentDueDateDto extendDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

    [HttpPost("withdraw")]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> WithdrawAssignment([FromBody] WithdrawAssignmentDto withdrawDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var command = new WithdrawAssignmentCommand(
                withdrawDto.AssignmentId,
                withdrawDto.WithdrawnBy,
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

    [HttpPost("{assignmentId}/confirm-employee")]
    public async Task<IActionResult> ConfirmEmployeeCompletion(Guid assignmentId, [FromBody] ConfirmCompletionDto confirmDto)
    {
        try
        {
            var command = new ConfirmEmployeeCompletionCommand(assignmentId, confirmDto.ConfirmedBy);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming employee completion");
            return StatusCode(500, "An error occurred while confirming employee completion");
        }
    }

    [HttpPost("{assignmentId}/confirm-manager")]
    [Authorize(Roles = "TeamLead")]
    public async Task<IActionResult> ConfirmManagerCompletion(Guid assignmentId, [FromBody] ConfirmCompletionDto confirmDto)
    {
        try
        {
            var command = new ConfirmManagerCompletionCommand(assignmentId, confirmDto.ConfirmedBy);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming manager completion");
            return StatusCode(500, "An error occurred while confirming manager completion");
        }
    }

    [HttpPost("{assignmentId}/initiate-review")]
    [Authorize(Roles = "TeamLead")]
    public async Task<IActionResult> InitiateReview(Guid assignmentId, [FromBody] InitiateReviewDto initiateDto)
    {
        try
        {
            var command = new InitiateReviewCommand(assignmentId, initiateDto.InitiatedBy);
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
            var command = new EditAnswerDuringReviewCommand(
                assignmentId,
                editDto.SectionId,
                editDto.QuestionId,
                editDto.Answer,
                editDto.EditedBy);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error editing answer during review");
            return StatusCode(500, "An error occurred while editing answer");
        }
    }

    [HttpPost("{assignmentId}/confirm-employee-review")]
    public async Task<IActionResult> ConfirmEmployeeReview(Guid assignmentId, [FromBody] ConfirmCompletionDto confirmDto)
    {
        try
        {
            var command = new ConfirmEmployeeReviewCommand(assignmentId, confirmDto.ConfirmedBy);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming employee review");
            return StatusCode(500, "An error occurred while confirming employee review");
        }
    }

    [HttpPost("{assignmentId}/finalize")]
    [Authorize(Roles = "TeamLead")]
    public async Task<IActionResult> FinalizeQuestionnaire(Guid assignmentId, [FromBody] FinalizeQuestionnaireDto finalizeDto)
    {
        try
        {
            var command = new FinalizeQuestionnaireCommand(assignmentId, finalizeDto.FinalizedBy);
            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finalizing questionnaire");
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

            var command = new SendAssignmentReminderCommand(
                reminderDto.AssignmentId,
                reminderDto.Message,
                reminderDto.SentBy);

            var result = await commandDispatcher.SendAsync(command);
            return CreateResponse(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending reminder for assignment {AssignmentId}", reminderDto.AssignmentId);
            return StatusCode(500, "An error occurred while sending reminder");
        }
    }

}