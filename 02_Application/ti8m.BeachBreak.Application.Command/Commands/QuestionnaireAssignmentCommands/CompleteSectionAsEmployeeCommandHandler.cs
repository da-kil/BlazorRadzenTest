using Microsoft.Extensions.Logging;
using ti8m.BeachBreak.Application.Command.Repositories;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Handles the completion of a questionnaire section by an employee.
/// Marks a specific section as completed in the employee's workflow.
/// </summary>
public class CompleteSectionAsEmployeeCommandHandler
    : ICommandHandler<CompleteSectionAsEmployeeCommand, Result>
{
    private readonly IQuestionnaireAssignmentAggregateRepository repository;
    private readonly ILogger<CompleteSectionAsEmployeeCommandHandler> logger;

    public CompleteSectionAsEmployeeCommandHandler(
        IQuestionnaireAssignmentAggregateRepository repository,
        ILogger<CompleteSectionAsEmployeeCommandHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<Result> HandleAsync(CompleteSectionAsEmployeeCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Employee completing section {SectionId} for assignment {AssignmentId}",
                command.SectionId, command.AssignmentId);

            var assignment = await repository.LoadRequiredAsync<Domain.QuestionnaireAssignmentAggregate.QuestionnaireAssignment>(command.AssignmentId, cancellationToken: cancellationToken);
            assignment.CompleteSectionAsEmployee(command.SectionId);
            await repository.StoreAsync(assignment, cancellationToken);

            logger.LogInformation("Successfully completed section {SectionId} as employee for assignment {AssignmentId}",
                command.SectionId, command.AssignmentId);
            return Result.Success("Section completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing section {SectionId} as employee for assignment {AssignmentId}",
                command.SectionId, command.AssignmentId);
            return Result.Fail("Failed to complete section: " + ex.Message, 500);
        }
    }
}
