namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

/// <summary>
/// Exception thrown when an invalid workflow state transition is attempted.
/// This exception is raised by the WorkflowStateMachine when validation fails.
/// </summary>
public class InvalidWorkflowTransitionException : Exception
{
    public WorkflowState CurrentState { get; }
    public WorkflowState TargetState { get; }
    public bool IsReopenAttempt { get; }

    public InvalidWorkflowTransitionException(
        WorkflowState currentState,
        WorkflowState targetState,
        string reason,
        bool isReopenAttempt = false)
        : base($"Invalid workflow transition from {currentState} to {targetState}: {reason}")
    {
        CurrentState = currentState;
        TargetState = targetState;
        IsReopenAttempt = isReopenAttempt;
    }
}
