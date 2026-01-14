using ti8m.BeachBreak.Domain.QuestionnaireAssignmentAggregate;
using Xunit;

namespace ti8m.BeachBreak.Domain.Tests;

/// <summary>
/// Unit tests for WorkflowStateMachine domain service.
/// Tests both forward and backward transition validation.
/// </summary>
public class WorkflowStateMachineTests
{
    #region Forward Transition Tests

    [Theory]
    // Initialized state transitions (NEW - Phase 4)
    [InlineData(WorkflowState.Assigned, WorkflowState.Initialized, true)] // Manager completes initialization
    [InlineData(WorkflowState.Initialized, WorkflowState.EmployeeInProgress, true)] // Employee starts
    [InlineData(WorkflowState.Initialized, WorkflowState.ManagerInProgress, true)] // Manager starts
    [InlineData(WorkflowState.Initialized, WorkflowState.BothInProgress, true)] // Both start
    // Existing transitions
    [InlineData(WorkflowState.EmployeeInProgress, WorkflowState.BothInProgress, true)]
    [InlineData(WorkflowState.EmployeeInProgress, WorkflowState.EmployeeSubmitted, true)]
    [InlineData(WorkflowState.EmployeeSubmitted, WorkflowState.BothSubmitted, true)]
    [InlineData(WorkflowState.EmployeeSubmitted, WorkflowState.Finalized, true)] // Auto-finalize
    [InlineData(WorkflowState.BothSubmitted, WorkflowState.InReview, true)]
    [InlineData(WorkflowState.InReview, WorkflowState.ManagerReviewConfirmed, true)]
    public void CanTransitionForward_ValidTransitions_ReturnsValid(
        WorkflowState from,
        WorkflowState to,
        bool expected)
    {
        // Act
        var result = WorkflowStateMachine.CanTransitionForward(from, to, out var reason);

        // Assert
        Assert.Equal(WorkflowStateMachine.ValidationResult.Valid, result);
        Assert.Null(reason);
    }

    [Theory]
    // Invalid Initialized state transitions (NEW - Phase 4)
    [InlineData(WorkflowState.Assigned, WorkflowState.EmployeeInProgress)] // Must initialize first
    [InlineData(WorkflowState.Assigned, WorkflowState.ManagerInProgress)] // Must initialize first
    [InlineData(WorkflowState.Assigned, WorkflowState.BothInProgress)] // Must initialize first
    [InlineData(WorkflowState.Initialized, WorkflowState.Assigned)] // Can't go backwards to Assigned
    // Existing invalid transitions
    [InlineData(WorkflowState.Assigned, WorkflowState.Finalized)] // Can't skip to finalized
    [InlineData(WorkflowState.Assigned, WorkflowState.InReview)] // Can't jump to review
    [InlineData(WorkflowState.EmployeeInProgress, WorkflowState.ManagerSubmitted)] // Wrong role
    [InlineData(WorkflowState.EmployeeSubmitted, WorkflowState.EmployeeInProgress)] // Backwards
    [InlineData(WorkflowState.InReview, WorkflowState.BothSubmitted)] // Backwards
    [InlineData(WorkflowState.Finalized, WorkflowState.Assigned)] // Terminal state
    public void CanTransitionForward_InvalidTransitions_ReturnsInvalid(
        WorkflowState from,
        WorkflowState to)
    {
        // Act
        var result = WorkflowStateMachine.CanTransitionForward(from, to, out var reason);

        // Assert
        Assert.Equal(WorkflowStateMachine.ValidationResult.Invalid, result);
        Assert.NotNull(reason);
        Assert.NotEmpty(reason);
    }

    [Fact]
    public void CanTransitionForward_FromFinalizedState_AlwaysReturnsInvalid()
    {
        // Arrange
        var allStates = Enum.GetValues<WorkflowState>();

        foreach (var targetState in allStates)
        {
            // Act
            var result = WorkflowStateMachine.CanTransitionForward(
                WorkflowState.Finalized,
                targetState,
                out var reason);

            // Assert
            Assert.Equal(WorkflowStateMachine.ValidationResult.Invalid, result);
            Assert.Contains("Finalized", reason);
        }
    }

    #endregion

    #region Backward Transition Tests (Reopening)

    [Theory]
    [InlineData(WorkflowState.EmployeeSubmitted, WorkflowState.EmployeeInProgress, "Admin", true)]
    [InlineData(WorkflowState.EmployeeSubmitted, WorkflowState.EmployeeInProgress, "HR", true)]
    [InlineData(WorkflowState.EmployeeSubmitted, WorkflowState.EmployeeInProgress, "TeamLead", true)]
    [InlineData(WorkflowState.ManagerSubmitted, WorkflowState.ManagerInProgress, "Admin", true)]
    [InlineData(WorkflowState.BothSubmitted, WorkflowState.BothInProgress, "HR", true)]
    [InlineData(WorkflowState.ManagerReviewConfirmed, WorkflowState.InReview, "Admin", true)]
    [InlineData(WorkflowState.EmployeeReviewConfirmed, WorkflowState.InReview, "HR", true)]
    public void CanTransitionBackward_ValidReopeningWithAuthorizedRole_ReturnsValid(
        WorkflowState from,
        WorkflowState to,
        string role,
        bool expected)
    {
        // Act
        var result = WorkflowStateMachine.CanTransitionBackward(from, to, role, out var reason);

        // Assert
        Assert.Equal(WorkflowStateMachine.ValidationResult.Valid, result);
        Assert.Null(reason);
    }

    [Theory]
    [InlineData(WorkflowState.EmployeeSubmitted, WorkflowState.EmployeeInProgress, "Employee")]
    [InlineData(WorkflowState.EmployeeSubmitted, WorkflowState.EmployeeInProgress, "Manager")]
    [InlineData(WorkflowState.ManagerReviewConfirmed, WorkflowState.InReview, "TeamLead")] // TeamLead can't reopen review states
    [InlineData(WorkflowState.EmployeeReviewConfirmed, WorkflowState.InReview, "Employee")]
    public void CanTransitionBackward_UnauthorizedRole_ReturnsInvalid(
        WorkflowState from,
        WorkflowState to,
        string role)
    {
        // Act
        var result = WorkflowStateMachine.CanTransitionBackward(from, to, role, out var reason);

        // Assert
        Assert.Equal(WorkflowStateMachine.ValidationResult.Invalid, result);
        Assert.NotNull(reason);
        Assert.Contains(role, reason);
    }

    [Fact]
    public void CanTransitionBackward_FromFinalizedState_AlwaysReturnsInvalid()
    {
        // Arrange
        var allStates = Enum.GetValues<WorkflowState>();

        foreach (var targetState in allStates)
        {
            // Act
            var result = WorkflowStateMachine.CanTransitionBackward(
                WorkflowState.Finalized,
                targetState,
                "Admin",
                out var reason);

            // Assert
            Assert.Equal(WorkflowStateMachine.ValidationResult.Invalid, result);
            Assert.Contains("Finalized", reason);
        }
    }

    [Theory]
    [InlineData(WorkflowState.Assigned)] // Not yet submitted
    [InlineData(WorkflowState.Initialized)] // Manager initialization phase (NEW for Phase 4)
    [InlineData(WorkflowState.EmployeeInProgress)] // In progress
    [InlineData(WorkflowState.ManagerInProgress)] // In progress
    [InlineData(WorkflowState.BothInProgress)] // In progress
    [InlineData(WorkflowState.Finalized)] // Terminal
    public void CanTransitionBackward_NonReopenableStates_ReturnsInvalid(WorkflowState state)
    {
        // Act
        var result = WorkflowStateMachine.CanTransitionBackward(
            state,
            WorkflowState.Assigned,
            "Admin",
            out var reason);

        // Assert
        Assert.Equal(WorkflowStateMachine.ValidationResult.Invalid, result);
        Assert.NotNull(reason);
    }

    #endregion

    #region Helper Method Tests

    [Theory]
    [InlineData(WorkflowState.Assigned, 1)] // Can only go to Initialized (UPDATED for Phase 4)
    [InlineData(WorkflowState.Initialized, 3)] // Can go to 3 in-progress states (NEW for Phase 4)
    [InlineData(WorkflowState.EmployeeSubmitted, 2)] // BothSubmitted or Finalized
    [InlineData(WorkflowState.BothSubmitted, 1)] // Only InReview
    [InlineData(WorkflowState.Finalized, 0)] // Terminal state
    public void GetValidNextStates_ReturnsCorrectCount(WorkflowState state, int expectedCount)
    {
        // Act
        var validStates = WorkflowStateMachine.GetValidNextStates(state);

        // Assert
        Assert.Equal(expectedCount, validStates.Count);
    }

    [Theory]
    [InlineData(WorkflowState.EmployeeSubmitted, true)]
    [InlineData(WorkflowState.ManagerSubmitted, true)]
    [InlineData(WorkflowState.BothSubmitted, true)]
    [InlineData(WorkflowState.ManagerReviewConfirmed, true)]
    [InlineData(WorkflowState.EmployeeReviewConfirmed, true)]
    [InlineData(WorkflowState.Assigned, false)]
    [InlineData(WorkflowState.Finalized, false)]
    public void IsReopenable_ReturnsCorrectValue(WorkflowState state, bool expected)
    {
        // Act
        var result = WorkflowStateMachine.IsReopenable(state);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetRolesWhoCanReopen_SubmissionStates_IncludesTeamLead()
    {
        // Act
        var roles = WorkflowStateMachine.GetRolesWhoCanReopen(WorkflowState.EmployeeSubmitted);

        // Assert
        Assert.Contains("Admin", roles, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("HR", roles, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("TeamLead", roles, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetRolesWhoCanReopen_ReviewStates_ExcludesTeamLead()
    {
        // Act
        var roles = WorkflowStateMachine.GetRolesWhoCanReopen(WorkflowState.ManagerReviewConfirmed);

        // Assert
        Assert.Contains("Admin", roles, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("HR", roles, StringComparer.OrdinalIgnoreCase);
        Assert.DoesNotContain("TeamLead", roles, StringComparer.OrdinalIgnoreCase);
    }

    #endregion

    #region Auto-Transition Logic Tests

    [Theory]
    [InlineData(true, true, WorkflowState.BothInProgress)]
    [InlineData(true, false, WorkflowState.EmployeeInProgress)]
    [InlineData(false, true, WorkflowState.ManagerInProgress)]
    [InlineData(false, false, WorkflowState.Initialized)] // UPDATED: Default state is Initialized (not Assigned)
    public void DetermineProgressState_ReturnsCorrectState(
        bool hasEmployeeProgress,
        bool hasManagerProgress,
        WorkflowState expected)
    {
        // Act - Start from Initialized state (UPDATED for Phase 4)
        var result = WorkflowStateMachine.DetermineProgressState(
            hasEmployeeProgress,
            hasManagerProgress,
            WorkflowState.Initialized);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DetermineProgressState_AfterSubmission_DoesNotChange()
    {
        // Act
        var result = WorkflowStateMachine.DetermineProgressState(
            hasEmployeeProgress: true,
            hasManagerProgress: true,
            currentState: WorkflowState.EmployeeSubmitted);

        // Assert - Should remain in EmployeeSubmitted, not change to BothInProgress
        Assert.Equal(WorkflowState.EmployeeSubmitted, result);
    }

    [Theory]
    [InlineData(true, true, true, WorkflowState.BothSubmitted)]
    [InlineData(true, true, false, WorkflowState.Finalized)] // Auto-finalize
    [InlineData(true, false, true, WorkflowState.EmployeeSubmitted)]
    [InlineData(false, true, true, WorkflowState.ManagerSubmitted)]
    public void DetermineSubmissionState_ReturnsCorrectState(
        bool isEmployeeSubmitted,
        bool isManagerSubmitted,
        bool requiresManagerReview,
        WorkflowState expected)
    {
        // Act
        var result = WorkflowStateMachine.DetermineSubmissionState(
            isEmployeeSubmitted,
            isManagerSubmitted,
            requiresManagerReview);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DetermineSubmissionState_NoSubmissions_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            WorkflowStateMachine.DetermineSubmissionState(
                isEmployeeSubmitted: false,
                isManagerSubmitted: false,
                requiresManagerReview: true));
    }

    #endregion
}
