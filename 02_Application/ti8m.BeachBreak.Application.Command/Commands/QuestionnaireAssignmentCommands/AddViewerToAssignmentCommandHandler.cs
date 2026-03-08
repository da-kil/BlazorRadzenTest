using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles adding a viewer to a questionnaire assignment.
/// Viewers get read-only access to the assignment for collaboration, mentoring, or oversight.
/// </summary>
public class AddViewerToAssignmentCommandHandler
    : ICommandHandler<AddViewerToAssignmentCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository assignmentRepository;
    private readonly IEmployeeAggregateRepository employeeRepository;
    private readonly ILogger<AddViewerToAssignmentCommandHandler> logger;

    public AddViewerToAssignmentCommandHandler(
        IQuestionnaireAssignmentAggregateRepository assignmentRepository,
        IEmployeeAggregateRepository employeeRepository,
        ILogger<AddViewerToAssignmentCommandHandler> logger)
    {
        this.assignmentRepository = assignmentRepository;
        this.employeeRepository = employeeRepository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(AddViewerToAssignmentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Adding viewer {ViewerEmployeeId} to assignment {AssignmentId} by user {AddedByUserId}",
                command.ViewerEmployeeId,
                command.AssignmentId,
                command.AddedByUserId);

            // Load the assignment aggregate
            var assignment = await assignmentRepository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                cancellationToken: cancellationToken);

            // Load the viewer employee details
            var viewerEmployee = await employeeRepository.LoadRequiredAsync<Domain.EmployeeAggregate.Employee>(
                command.ViewerEmployeeId,
                cancellationToken: cancellationToken);

            // Add the viewer to the assignment
            var viewerFullName = $"{viewerEmployee.FirstName} {viewerEmployee.LastName}".Trim();
            assignment.AddViewer(
                viewerEmployee.Id,
                viewerFullName,
                viewerEmployee.EMail,
                command.AddedByUserId);

            // Store the updated assignment
            await assignmentRepository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation(
                "Successfully added viewer {ViewerEmployeeId} ({ViewerName}) to assignment {AssignmentId}",
                command.ViewerEmployeeId,
                viewerFullName,
                command.AssignmentId);

            return Result.Success("Viewer added to assignment successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error adding viewer {ViewerEmployeeId} to assignment {AssignmentId}",
                command.ViewerEmployeeId,
                command.AssignmentId);

            return Result.Fail("Failed to add viewer: " + ex.Message, 500);
        }
    }
}