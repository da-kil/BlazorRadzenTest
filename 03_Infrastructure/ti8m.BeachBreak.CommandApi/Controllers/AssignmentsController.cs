using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ti8m.BeachBreak.Application.Command.Commands;
using ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;
using ti8m.BeachBreak.CommandApi.Dto;

namespace ti8m.BeachBreak.CommandApi.Controllers;

[ApiController]
[Route("c/api/v{version:apiVersion}/assignments")]
[Authorize] // All endpoints require authentication
public class AssignmentsController : BaseController
{
    private readonly ICommandDispatcher commandDispatcher;
    private readonly ILogger<AssignmentsController> logger;

    public AssignmentsController(
        ICommandDispatcher commandDispatcher,
        ILogger<AssignmentsController> logger)
    {
        this.commandDispatcher = commandDispatcher;
        this.logger = logger;
    }

    [HttpPost("bulk")]
    [Authorize(Policy = "HRAccess")] // Only Admin, HRLead, HR can create assignments
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

    [HttpPost("{assignmentId}/start")]
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
    [Authorize(Policy = "ManagerAccess")] // Only managers
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
    [Authorize(Policy = "ManagerAccess")] // Only managers
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
    [Authorize(Policy = "ManagerAccess")] // Only managers can initiate review
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
    [Authorize(Policy = "ManagerAccess")] // Only managers can finalize
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

}