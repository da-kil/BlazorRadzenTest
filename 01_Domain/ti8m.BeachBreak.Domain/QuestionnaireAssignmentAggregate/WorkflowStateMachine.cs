using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;

namespace ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

/// <summary>
/// Domain service that enforces workflow state transition rules.
/// This is the authoritative implementation of the state machine.
/// Validates both forward (normal) and backward (reopening) transitions.
/// </summary>
public class WorkflowStateMachine
{
    /// <summary>
    /// Validates if a forward transition from currentState to targetState is allowed.
    /// Forward transitions follow the normal workflow progression.
    /// </summary>
    public static ValidationResult CanTransitionForward(
        WorkflowState currentState,
        WorkflowState targetState,
        out string? failureReason)
    {
        // Terminal state check
        if (currentState == WorkflowState.Finalized)
        {
            failureReason = "Cannot transition from Finalized state - questionnaire is locked permanently";
            return ValidationResult.Invalid;
        }

        // Check if current state exists in transition matrix
        if (!WorkflowTransitions.ForwardTransitions.TryGetValue(currentState, out var validTransitions))
        {
            failureReason = $"Unknown current state: {currentState}";
            return ValidationResult.Invalid;
        }

        // Check if target state is valid from current state
        if (!validTransitions.Any(t => t.TargetState == targetState))
        {
            var validStates = string.Join(", ", validTransitions.Select(t => t.TargetState));
            failureReason = $"Invalid forward transition from {currentState} to {targetState}. Valid next states: {validStates}";
            return ValidationResult.Invalid;
        }

        failureReason = null;
        return ValidationResult.Valid;
    }

    /// <summary>
    /// Validates if a backward transition (reopening) from currentState to targetState is allowed.
    /// Backward transitions require special authorization.
    ///
    /// Authorization levels:
    /// - Admin: Can reopen ANY non-finalized state for ALL questionnaires
    /// - HR: Can reopen ANY non-finalized state for ALL questionnaires
    /// - TeamLead: Can reopen ALL non-finalized states for THEIR TEAM ONLY (data-scoped)
    ///
    /// Note: Data-level authorization (TeamLead to their team) must be checked by caller.
    /// This method only validates role-level authorization.
    /// </summary>
    public static ValidationResult CanTransitionBackward(
        WorkflowState currentState,
        WorkflowState targetState,
        string userRole,
        out string? failureReason)
    {
        // Finalized state cannot be reopened
        if (currentState == WorkflowState.Finalized)
        {
            failureReason = "Cannot reopen Finalized state - questionnaire is locked permanently. Create a new assignment instead.";
            return ValidationResult.Invalid;
        }

        // Check if current state can be reopened
        if (!WorkflowTransitions.BackwardTransitions.TryGetValue(currentState, out var reopenTransitions))
        {
            failureReason = $"State {currentState} cannot be reopened - no backward transitions defined";
            return ValidationResult.Invalid;
        }

        // Find the specific reopen transition
        var transition = reopenTransitions.FirstOrDefault(t => t.TargetState == targetState);
        if (transition == null)
        {
            var validStates = string.Join(", ", reopenTransitions.Select(t => t.TargetState));
            failureReason = $"Invalid reopen transition from {currentState} to {targetState}. Valid reopen targets: {validStates}";
            return ValidationResult.Invalid;
        }

        // Check role-level authorization
        if (!transition.AllowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
        {
            failureReason = $"Role '{userRole}' is not authorized to reopen from {currentState}. Allowed roles: {string.Join(", ", transition.AllowedRoles)}";
            return ValidationResult.Invalid;
        }

        failureReason = null;
        return ValidationResult.Valid;
    }

    /// <summary>
    /// Gets all valid next states (forward transitions) from the current state.
    /// </summary>
    public static List<WorkflowState> GetValidNextStates(WorkflowState currentState)
    {
        if (!WorkflowTransitions.ForwardTransitions.TryGetValue(currentState, out var transitions))
            return new List<WorkflowState>();

        return transitions.Select(t => t.TargetState).ToList();
    }

    /// <summary>
    /// Gets all valid reopen target states (backward transitions) from the current state.
    /// </summary>
    public static List<WorkflowState> GetValidReopenStates(WorkflowState currentState)
    {
        if (!WorkflowTransitions.BackwardTransitions.TryGetValue(currentState, out var transitions))
            return new List<WorkflowState>();

        return transitions.Select(t => t.TargetState).ToList();
    }

    /// <summary>
    /// Determines if a state can be reopened by any role.
    /// </summary>
    public static bool IsReopenable(WorkflowState state)
    {
        return state != WorkflowState.Finalized &&
               WorkflowTransitions.BackwardTransitions.ContainsKey(state);
    }

    /// <summary>
    /// Gets roles that can reopen a specific state.
    /// Returns empty array if state cannot be reopened.
    /// </summary>
    public static string[] GetRolesWhoCanReopen(WorkflowState state)
    {
        if (!WorkflowTransitions.BackwardTransitions.TryGetValue(state, out var transitions))
            return Array.Empty<string>();

        return transitions
            .SelectMany(t => t.AllowedRoles)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Determines the next workflow state based on section progress.
    /// Used for automatic state transitions when sections are completed.
    /// </summary>
    public static WorkflowState DetermineProgressState(
        bool hasEmployeeProgress,
        bool hasManagerProgress,
        WorkflowState currentState)
    {
        // Don't update state if already in submission or later phases
        if (currentState >= WorkflowState.EmployeeSubmitted)
            return currentState;

        if (hasEmployeeProgress && hasManagerProgress)
            return WorkflowState.BothInProgress;

        if (hasEmployeeProgress)
            return WorkflowState.EmployeeInProgress;

        if (hasManagerProgress)
            return WorkflowState.ManagerInProgress;

        // Preserve Assigned or Initialized state if no progress yet
        // Don't auto-transition from Initialized to Assigned
        if (currentState == WorkflowState.Assigned || currentState == WorkflowState.Initialized)
            return currentState;

        return WorkflowState.Assigned;
    }

    /// <summary>
    /// Determines the next workflow state based on work having started (via StartedDate).
    /// Used when responses are saved but no sections are completed yet.
    /// </summary>
    public static WorkflowState DetermineProgressStateFromStartedWork(
        bool hasStarted,
        bool hasEmployeeProgress,
        bool hasManagerProgress,
        CompletionRole startedBy,
        WorkflowState currentState)
    {
        // Don't update state if already in submission or later phases
        if (currentState >= WorkflowState.EmployeeSubmitted)
            return currentState;

        // If sections are completed, use section-based logic
        if (hasEmployeeProgress && hasManagerProgress)
            return WorkflowState.BothInProgress;

        if (hasEmployeeProgress)
            return WorkflowState.EmployeeInProgress;

        if (hasManagerProgress)
            return WorkflowState.ManagerInProgress;

        // If no sections completed but work has started, transition based on who started
        if (hasStarted)
        {
            return (currentState, startedBy) switch
            {
                // From Assigned or Initialized state
                (WorkflowState.Assigned, CompletionRole.Employee) => WorkflowState.EmployeeInProgress,
                (WorkflowState.Assigned, CompletionRole.Manager) => WorkflowState.ManagerInProgress,
                (WorkflowState.Initialized, CompletionRole.Employee) => WorkflowState.EmployeeInProgress,
                (WorkflowState.Initialized, CompletionRole.Manager) => WorkflowState.ManagerInProgress,

                // From single-role in-progress to both
                (WorkflowState.EmployeeInProgress, CompletionRole.Manager) => WorkflowState.BothInProgress,
                (WorkflowState.ManagerInProgress, CompletionRole.Employee) => WorkflowState.BothInProgress,

                // Already in correct state or Both role (not used in practice)
                _ => currentState
            };
        }

        return currentState;
    }

    /// <summary>
    /// Determines workflow state after a submission event.
    /// Handles both simple (employee-only) and complex (with manager review) workflows.
    /// </summary>
    public static WorkflowState DetermineSubmissionState(
        bool isEmployeeSubmitted,
        bool isManagerSubmitted,
        bool requiresManagerReview)
    {
        // Simple workflow: Auto-finalize immediately if manager review not required
        if (isEmployeeSubmitted && !requiresManagerReview)
            return WorkflowState.Finalized;

        // Complex workflow: Both must submit before review
        if (isEmployeeSubmitted && isManagerSubmitted)
            return WorkflowState.BothSubmitted;

        if (isEmployeeSubmitted)
            return WorkflowState.EmployeeSubmitted;

        if (isManagerSubmitted)
            return WorkflowState.ManagerSubmitted;

        throw new InvalidOperationException("At least one submission must be present to determine submission state");
    }

    public enum ValidationResult
    {
        Valid,
        Invalid
    }
}
