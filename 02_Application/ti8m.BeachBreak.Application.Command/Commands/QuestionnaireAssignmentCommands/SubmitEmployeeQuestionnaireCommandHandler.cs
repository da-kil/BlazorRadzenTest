using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the submission of a completed employee questionnaire.
/// Marks the employee's portion of the questionnaire as submitted.
/// </summary>
public class SubmitEmployeeQuestionnaireCommandHandler
    : ICommandHandler<SubmitEmployeeQuestionnaireCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<SubmitEmployeeQuestionnaireCommandHandler> logger;

    public SubmitEmployeeQuestionnaireCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<SubmitEmployeeQuestionnaireCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(SubmitEmployeeQuestionnaireCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Employee submitting questionnaire for assignment {AssignmentId}", command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(
                command.AssignmentId,
                command.ExpectedVersion,
                cancellationToken);
            assignment.SubmitEmployeeQuestionnaire(command.SubmittedByEmployeeId);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully submitted employee questionnaire for assignment {AssignmentId}", command.AssignmentId);
            return Result.Success("Employee questionnaire submitted");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting employee questionnaire for assignment {AssignmentId}", command.AssignmentId);
            return Result.Fail("Failed to submit employee questionnaire: " + ex.Message, 500);
        }
    }
}
