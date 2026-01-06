namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to initialize a questionnaire assignment.
/// Enables manager-only initialization phase with optional tasks
/// like linking predecessor questionnaires and adding custom questions.
/// </summary>
public class InitializeAssignmentCommand : ICommand<Result>
{
    public Guid AssignmentId { get; init; }
    public Guid InitializedByEmployeeId { get; init; }
    public string? InitializationNotes { get; init; }

    public InitializeAssignmentCommand(
        Guid assignmentId,
        Guid initializedByEmployeeId,
        string? initializationNotes = null)
    {
        AssignmentId = assignmentId;
        InitializedByEmployeeId = initializedByEmployeeId;
        InitializationNotes = initializationNotes;
    }
}
