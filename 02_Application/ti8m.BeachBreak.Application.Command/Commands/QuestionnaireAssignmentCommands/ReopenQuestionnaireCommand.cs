using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;

namespace ti8m.BeachBreak.Application.Command.Commands.QuestionnaireAssignmentCommands;

/// <summary>
/// Command to reopen a questionnaire that has been submitted or confirmed.
/// Requires Admin, HR, or TeamLead role.
///
/// Authorization:
/// - Admin: Can reopen ANY non-finalized state for ALL questionnaires
/// - HR: Can reopen ANY non-finalized state for ALL questionnaires
/// - TeamLead: Can reopen ALL non-finalized states for THEIR TEAM ONLY (data-scoped)
///
/// Note: Finalized state CANNOT be reopened. Create new assignment instead.
/// Note: ReopenReason is REQUIRED (minimum 10 characters).
/// Note: Email notifications will be sent to affected parties.
/// </summary>
public record ReopenQuestionnaireCommand(
    Guid AssignmentId,
    WorkflowState TargetState,
    string ReopenReason,
    Guid ReopenedByEmployeeId,
    string ReopenedByRole) : ICommand<Result>;
