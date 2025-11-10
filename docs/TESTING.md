# Testing Guidelines for ti8m BeachBreak

## Table of Contents

1. [Overview & Philosophy](#overview--philosophy)
2. [Current State](#current-state)
3. [Test Project Setup](#test-project-setup)
4. [Domain Layer Testing](#domain-layer-testing)
5. [Application Layer Testing](#application-layer-testing)
6. [Integration Testing](#integration-testing)
7. [Test Data Builders](#test-data-builders)
8. [Do's and Don'ts](#dos-and-donts)
9. [Conventions & Patterns](#conventions--patterns)
10. [Priority Matrix](#priority-matrix)
11. [Complete Code Examples](#complete-code-examples)
12. [References](#references)
13. [Roadmap](#roadmap)

---

## Overview & Philosophy

### Why Testing Matters for Event Sourcing/CQRS

ti8m BeachBreak uses **CQRS (Command Query Responsibility Segregation)** and **Event Sourcing** architecture. This makes testing both **critical** and **easier** than traditional architectures:

#### Critical Because:
- **Domain events are the source of truth** - bugs in event application break the entire system
- **State transitions are complex** - QuestionnaireAssignment has 11 workflow states with intricate rules
- **No database rollback** - events are append-only; bad events corrupt history
- **Authorization is complex** - role-based permissions vary by workflow state
- **Business rules are intricate** - goal weighting, predecessor validation, submission workflows

#### Easier Because:
- **Deterministic state rebuilding** - aggregates reconstitute from events, making any state testable
- **No database needed for unit tests** - test pure domain logic in memory
- **Clear test boundaries** - CQRS separates commands (write) from queries (read)
- **Event history = documentation** - events describe exactly what happened
- **Command/Query handlers are small** - focused single-purpose handlers are easy to test

### Test Pyramid for ti8m BeachBreak

```
                    ▲
                   /E\      E2E Tests (Few)
                  /___\     - Critical user workflows
                 /     \    - Blazor component integration
                /       \
               /_________\  Integration Tests (Some)
              /           \ - Event sourcing roundtrips
             /             \- API endpoint tests
            /               \- Marten projections
           /                 \
          /___________________\ Unit Tests (Many)
         /                     \ - Aggregate tests (Given/When/Then)
        /                       \- Command handler tests (mocked repos)
       /                         \- Query handler tests (mocked reads)
      /___________________________\- Domain service tests
                                   - Value object tests

     MOST TESTS HERE              FEWEST TESTS HERE
     Fast, Isolated               Slow, Integrated
```

### Testing Goals

1. **Protect domain invariants** - Ensure business rules cannot be violated
2. **Document behavior** - Tests serve as executable specifications
3. **Enable refactoring** - Change implementation with confidence
4. **Catch regressions** - Prevent bugs from reappearing
5. **Support onboarding** - New developers learn from tests

---

## Current State

### What We Have

**One test file**: `Tests/ti8m.BeachBreak.Domain.Tests/WorkflowStateMachineTests.cs`
- 293 lines
- Tests `WorkflowStateMachine` domain service
- Excellent test quality (use as template)
- **Problem**: No .csproj file exists - tests cannot run!

### What We're Missing

- ❌ **NO test projects** properly configured in solution
- ❌ **NO aggregate tests** (0% coverage of Category, Employee, QuestionnaireAssignment, etc.)
- ❌ **NO command handler tests** (30+ handlers untested)
- ❌ **NO query handler tests** (20+ handlers untested)
- ❌ **NO integration tests** (event sourcing persistence untested)
- ❌ **NO API tests** (endpoints untested)
- ❌ **NO frontend tests** (400+ Blazor components untested)

**Test Coverage**: ~0.1% (1 domain service out of 1000+ classes)

### The Good News

The existing `WorkflowStateMachineTests.cs` demonstrates the team **knows how to write excellent tests**:
- Uses xUnit with Theory/InlineData for data-driven tests
- Clear Arrange/Act/Assert structure
- Descriptive test names
- Comprehensive coverage of complex logic
- Tests both happy paths and error cases

**We just need to scale this pattern across the codebase.**

---

## Test Project Setup

### 1. Create Test Project Structure

```
Tests/
├── ti8m.BeachBreak.Domain.Tests/
│   ├── ti8m.BeachBreak.Domain.Tests.csproj
│   ├── AggregateTests/
│   │   ├── CategoryTests.cs
│   │   ├── EmployeeTests.cs
│   │   ├── QuestionnaireAssignmentTests.cs
│   │   ├── QuestionnaireTemplateTests.cs
│   │   └── QuestionnaireResponseTests.cs
│   ├── DomainServices/
│   │   └── WorkflowStateMachineTests.cs (existing)
│   └── ValueObjects/
│       ├── TranslationTests.cs
│       └── GoalTests.cs
│
├── ti8m.BeachBreak.Application.Tests/
│   ├── ti8m.BeachBreak.Application.Tests.csproj
│   ├── CommandHandlers/
│   │   ├── CategoryCommandHandlerTests.cs
│   │   ├── EmployeeCommandHandlerTests.cs
│   │   └── QuestionnaireAssignmentCommandHandlerTests.cs
│   └── QueryHandlers/
│       ├── CategoryQueryHandlerTests.cs
│       └── DashboardQueryHandlerTests.cs
│
├── ti8m.BeachBreak.Integration.Tests/
│   ├── ti8m.BeachBreak.Integration.Tests.csproj
│   ├── EventSourcing/
│   │   ├── CategoryAggregateRepositoryTests.cs
│   │   └── QuestionnaireAssignmentRepositoryTests.cs
│   ├── Api/
│   │   ├── CategoryApiTests.cs
│   │   └── QuestionnaireAssignmentApiTests.cs
│   └── Fixtures/
│       ├── DatabaseFixture.cs
│       └── WebApplicationFixture.cs
│
└── ti8m.BeachBreak.Frontend.Tests/ (future)
    ├── ti8m.BeachBreak.Frontend.Tests.csproj
    └── Components/
        └── (Blazor component tests with bUnit)
```

### 2. Create Test Project Files

#### ti8m.BeachBreak.Domain.Tests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- Test Framework -->
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />

    <!-- Mocking -->
    <PackageReference Include="NSubstitute" Version="5.1.0" />

    <!-- Assertions -->
    <PackageReference Include="AwesomeAssertions" Version="7.0.0" />

    <!-- Coverage -->
    <PackageReference Include="coverlet.collector" Version="6.0.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\01_Domain\ti8m.BeachBreak.Domain\ti8m.BeachBreak.Domain.csproj" />
    <ProjectReference Include="..\..\04_Core\ti8m.BeachBreak.Core.Domain\ti8m.BeachBreak.Core.Domain.csproj" />
  </ItemGroup>

</Project>
```

#### ti8m.BeachBreak.Application.Tests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- Test Framework -->
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />

    <!-- Mocking -->
    <PackageReference Include="NSubstitute" Version="5.1.0" />

    <!-- Assertions -->
    <PackageReference Include="AwesomeAssertions" Version="7.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\02_Application\ti8m.BeachBreak.Application.Command\ti8m.BeachBreak.Application.Command.csproj" />
    <ProjectReference Include="..\..\02_Application\ti8m.BeachBreak.Application.Query\ti8m.BeachBreak.Application.Query.csproj" />
  </ItemGroup>

</Project>
```

#### ti8m.BeachBreak.Integration.Tests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- Test Framework -->
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />

    <!-- Integration Testing -->
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
    <PackageReference Include="Testcontainers.PostgreSql" Version="3.10.0" />

    <!-- Assertions -->
    <PackageReference Include="AwesomeAssertions" Version="7.0.0" />

    <!-- Marten for Event Sourcing Tests -->
    <PackageReference Include="Marten" Version="7.31.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\03_Infrastructure\ti8m.BeachBreak.CommandApi\ti8m.BeachBreak.CommandApi.csproj" />
    <ProjectReference Include="..\..\03_Infrastructure\ti8m.BeachBreak.QueryApi\ti8m.BeachBreak.QueryApi.csproj" />
    <ProjectReference Include="..\..\03_Infrastructure\ti8m.BeachBreak.Infrastructure.Marten\ti8m.BeachBreak.Infrastructure.Marten.csproj" />
  </ItemGroup>

</Project>
```

### 3. Add to Solution

```bash
cd C:\projects\BlazorRadzenTest

# Add test projects to solution
dotnet sln add Tests\ti8m.BeachBreak.Domain.Tests\ti8m.BeachBreak.Domain.Tests.csproj
dotnet sln add Tests\ti8m.BeachBreak.Application.Tests\ti8m.BeachBreak.Application.Tests.csproj
dotnet sln add Tests\ti8m.BeachBreak.Integration.Tests\ti8m.BeachBreak.Integration.Tests.csproj
```

### 4. Verify Setup

```bash
# Build test projects
dotnet build Tests\ti8m.BeachBreak.Domain.Tests
dotnet build Tests\ti8m.BeachBreak.Application.Tests
dotnet build Tests\ti8m.BeachBreak.Integration.Tests

# Run tests (should pass with 0 tests initially)
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## Domain Layer Testing

### Testing Aggregates with Given/When/Then

Event Sourcing makes aggregate testing **deterministic and easy**:
1. **Given**: Set up initial state by applying events
2. **When**: Execute domain behavior
3. **Then**: Assert new events raised and state changed

### Pattern: Testing Aggregate Methods

```csharp
using AwesomeAssertions;
using ti8m.BeachBreak.Domain.CategoryAggregate;
using ti8m.BeachBreak.Domain.CategoryAggregate.Events;
using ti8m.BeachBreak.Domain.SharedKernel;
using Xunit;

namespace ti8m.BeachBreak.Domain.Tests.AggregateTests;

public class CategoryTests
{
    [Fact]
    public void Constructor_ShouldRaiseCategoryCreatedEvent()
    {
        // Given
        var id = Guid.NewGuid();
        var name = new Translation("Test DE", "Test EN");
        var description = new Translation("Desc DE", "Desc EN");
        var sortOrder = 1;

        // When
        var category = new Category(id, name, description, sortOrder);

        // Then
        var events = category.UncommittedEvents.ToList();
        events.ShouldContainSingle();

        var evt = events.First().ShouldBeOfType<CategoryCreated>().Subject;
        evt.AggregateId.ShouldBe(id);
        evt.Name.ShouldBe(name);
        evt.Description.ShouldBe(description);
        evt.SortOrder.ShouldBe(sortOrder);
    }

    [Fact]
    public void ChangeName_ShouldRaiseCategoryNameChangedEvent()
    {
        // Given
        var category = CreateCategory();
        category.ClearUncommittedDomainEvents(); // Clear creation event
        var newName = new Translation("New DE", "New EN");

        // When
        category.ChangeName(newName);

        // Then
        var events = category.UncommittedEvents.ToList();
        events.ShouldContainSingle();

        var evt = events.First().ShouldBeOfType<CategoryNameChanged>().Subject;
        evt.Name.ShouldBe(newName);

        // Also verify state changed
        category.Name.ShouldBe(newName);
    }

    [Fact]
    public void ChangeName_WithNullName_ShouldThrowArgumentNullException()
    {
        // Given
        var category = CreateCategory();

        // When
        Action act = () => category.ChangeName(null!);

        // Then
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Delete_ShouldRaiseCategoryDeletedEvent()
    {
        // Given
        var category = CreateCategory();
        category.ClearUncommittedDomainEvents();

        // When
        category.Delete();

        // Then
        var events = category.UncommittedEvents.ToList();
        events.ShouldContainSingle();
        events.First().ShouldBeOfType<CategoryDeleted>();
    }

    // Helper to create test category
    private static Category CreateCategory()
    {
        return new Category(
            Guid.NewGuid(),
            new Translation("Test DE", "Test EN"),
            new Translation("Desc DE", "Desc EN"),
            sortOrder: 1);
    }
}
```

### Pattern: Testing Event Application (Apply Methods)

**CRITICAL**: Test that events correctly update aggregate state. This is the heart of event sourcing.

```csharp
public class CategoryEventApplicationTests
{
    [Fact]
    public void Apply_CategoryCreated_ShouldSetAllProperties()
    {
        // Given
        var id = Guid.NewGuid();
        var name = new Translation("Test DE", "Test EN");
        var description = new Translation("Desc DE", "Desc EN");
        var sortOrder = 5;
        var evt = new CategoryCreated(id, name, description, sortOrder);

        // When
        var category = new Category(); // Parameterless constructor for event sourcing
        category.Apply(evt);

        // Then
        category.Id.ShouldBe(id);
        category.Name.ShouldBe(name);
        category.Description.ShouldBe(description);
        category.SortOrder.ShouldBe(sortOrder);
        category.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Apply_CategoryNameChanged_ShouldUpdateNameOnly()
    {
        // Given
        var category = CreateCategory();
        var originalDescription = category.Description;
        var originalSortOrder = category.SortOrder;

        var newName = new Translation("Updated DE", "Updated EN");
        var evt = new CategoryNameChanged(newName);

        // When
        category.Apply(evt);

        // Then
        category.Name.ShouldBe(newName);
        category.Description.ShouldBe(originalDescription); // Unchanged
        category.SortOrder.ShouldBe(originalSortOrder); // Unchanged
    }

    [Fact]
    public void Apply_CategoryDeleted_ShouldSetIsDeletedTrue()
    {
        // Given
        var category = CreateCategory();
        var evt = new CategoryDeleted();

        // When
        category.Apply(evt);

        // Then
        category.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Apply_EventsInSequence_ShouldReconstructAggregateState()
    {
        // Given - Event history
        var id = Guid.NewGuid();
        var events = new IDomainEvent[]
        {
            new CategoryCreated(id,
                new Translation("Original DE", "Original EN"),
                new Translation("Desc DE", "Desc EN"),
                sortOrder: 1),
            new CategoryNameChanged(new Translation("Updated DE", "Updated EN")),
            new CategoryDescriptionChanged(new Translation("New Desc DE", "New Desc EN")),
            new CategorySortOrderChanged(10)
        };

        // When - Reconstitute from events
        var category = new Category();
        foreach (var evt in events)
        {
            category.Apply(evt);
        }

        // Then - Final state should match event history
        category.Id.ShouldBe(id);
        category.Name.German.ShouldBe("Updated DE");
        category.Name.English.ShouldBe("Updated EN");
        category.Description.German.ShouldBe("New Desc DE");
        category.SortOrder.ShouldBe(10);
        category.IsDeleted.Should().BeFalse();
    }

    private static Category CreateCategory()
    {
        return new Category(
            Guid.NewGuid(),
            new Translation("Test DE", "Test EN"),
            new Translation("Desc DE", "Desc EN"),
            sortOrder: 1);
    }
}
```

### Pattern: Testing Complex Aggregates (QuestionnaireAssignment)

For complex aggregates with multiple collaborators and business rules:

```csharp
public class QuestionnaireAssignmentTests
{
    #region Goal Management Tests

    [Fact]
    public void AddGoal_WithValidWeighting_ShouldRaiseGoalAddedEvent()
    {
        // Given
        var assignment = CreateAssignment();
        assignment.ClearUncommittedDomainEvents();

        var goal = CreateGoal(weighting: 25);

        // When
        var result = assignment.AddGoal(goal);

        // Then
        result.IsSuccess.Should().BeTrue();

        var events = assignment.UncommittedEvents.ToList();
        events.ShouldContainSingle();

        var evt = events.First().ShouldBeOfType<GoalAdded>().Subject;
        evt.Goal.ShouldBe(goal);
    }

    [Fact]
    public void AddGoal_WhenTotalWeightingExceeds100_ShouldReturnFailure()
    {
        // Given
        var assignment = CreateAssignment();
        assignment.AddGoal(CreateGoal(weighting: 80)); // First goal
        assignment.ClearUncommittedDomainEvents();

        var newGoal = CreateGoal(weighting: 30); // Would total 110%

        // When
        var result = assignment.AddGoal(newGoal);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.ShouldContain("weighting");
        result.ErrorMessage.ShouldContain("100");

        assignment.UncommittedEvents.ShouldBeEmpty(); // No event raised
    }

    [Theory]
    [InlineData(WorkflowState.Finalized)]
    [InlineData(WorkflowState.InReview)]
    public void AddGoal_WhenInLockedState_ShouldReturnFailure(WorkflowState lockedState)
    {
        // Given
        var assignment = CreateAssignmentInState(lockedState);
        assignment.ClearUncommittedDomainEvents();

        var goal = CreateGoal(weighting: 25);

        // When
        var result = assignment.AddGoal(goal);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.ShouldContain("state");
        assignment.UncommittedEvents.ShouldBeEmpty();
    }

    #endregion

    #region Workflow State Transition Tests

    [Theory]
    [InlineData(CompletionRole.Employee)]
    [InlineData(CompletionRole.Manager)]
    public void RecordSectionProgress_WhenCompletingFirstSection_ShouldTransitionToInProgress(
        CompletionRole role)
    {
        // Given
        var assignment = CreateAssignmentInState(WorkflowState.Assigned);
        assignment.ClearUncommittedDomainEvents();

        var sectionId = Guid.NewGuid();
        var progress = new SectionProgress(
            SectionId: sectionId,
            IsComplete: true,
            LastModified: DateTime.UtcNow);

        // When
        assignment.RecordSectionProgress(role, progress);

        // Then
        var events = assignment.UncommittedEvents.ToList();
        events.ShouldContain(e => e is WorkflowStateChanged);

        var stateEvent = events.OfType<WorkflowStateChanged>().First();
        stateEvent.NewState.ShouldBe(
            role == CompletionRole.Employee
                ? WorkflowState.EmployeeInProgress
                : WorkflowState.ManagerInProgress);
    }

    [Fact]
    public void Submit_WhenBothRolesSubmitted_ShouldTransitionToBothSubmitted()
    {
        // Given
        var assignment = CreateAssignmentInState(WorkflowState.BothInProgress);
        CompleteAllSectionsForRole(assignment, CompletionRole.Employee);
        CompleteAllSectionsForRole(assignment, CompletionRole.Manager);
        assignment.ClearUncommittedDomainEvents();

        // When
        var employeeResult = assignment.Submit(CompletionRole.Employee, submittedBy: Guid.NewGuid());
        var managerResult = assignment.Submit(CompletionRole.Manager, submittedBy: Guid.NewGuid());

        // Then
        employeeResult.IsSuccess.Should().BeTrue();
        managerResult.IsSuccess.Should().BeTrue();

        var events = assignment.UncommittedEvents.ToList();
        var finalState = events.OfType<WorkflowStateChanged>().Last();
        finalState.NewState.ShouldBe(WorkflowState.BothSubmitted);
    }

    #endregion

    #region Authorization Tests

    [Theory]
    [InlineData(WorkflowState.Assigned, true)]
    [InlineData(WorkflowState.EmployeeInProgress, true)]
    [InlineData(WorkflowState.ManagerInProgress, false)]
    [InlineData(WorkflowState.BothSubmitted, false)]
    [InlineData(WorkflowState.InReview, false)]
    [InlineData(WorkflowState.Finalized, false)]
    public void CanEmployeeEdit_ShouldReturnCorrectPermission(
        WorkflowState state,
        bool expectedCanEdit)
    {
        // Given
        var assignment = CreateAssignmentInState(state);

        // When
        var canEdit = assignment.CanEmployeeEdit();

        // Then
        canEdit.ShouldBe(expectedCanEdit);
    }

    [Theory]
    [InlineData(WorkflowState.InReview, true)] // Manager can edit during review
    [InlineData(WorkflowState.Finalized, false)] // Locked after finalized
    public void CanManagerEdit_InSpecialStates_ShouldReturnCorrectPermission(
        WorkflowState state,
        bool expectedCanEdit)
    {
        // Given
        var assignment = CreateAssignmentInState(state);

        // When
        var canEdit = assignment.CanManagerEdit();

        // Then
        canEdit.ShouldBe(expectedCanEdit);
    }

    #endregion

    #region Helper Methods

    private static QuestionnaireAssignment CreateAssignment()
    {
        return new QuestionnaireAssignment(
            id: Guid.NewGuid(),
            templateId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            managerId: Guid.NewGuid(),
            assignedDate: DateTime.UtcNow,
            dueDate: DateTime.UtcNow.AddMonths(1));
    }

    private static QuestionnaireAssignment CreateAssignmentInState(WorkflowState state)
    {
        var assignment = CreateAssignment();

        // Apply events to get to desired state
        // (Implementation depends on your state machine logic)

        return assignment;
    }

    private static Goal CreateGoal(int weighting)
    {
        return new Goal(
            Id: Guid.NewGuid(),
            Description: "Test Goal",
            Weighting: weighting);
    }

    private static void CompleteAllSectionsForRole(
        QuestionnaireAssignment assignment,
        CompletionRole role)
    {
        // Helper to mark all sections complete for a role
        // (Implementation depends on your section structure)
    }

    #endregion
}
```

### Pattern: Testing Domain Services

Reference the existing `WorkflowStateMachineTests.cs` as the gold standard:

```csharp
// Location: Tests/ti8m.BeachBreak.Domain.Tests/DomainServices/WorkflowStateMachineTests.cs
// This file demonstrates excellent testing practices:
// - Theory + InlineData for parameterized tests
// - Clear test organization with regions
// - Descriptive test names
// - Testing both success and failure paths
// - Validation of output parameters
```

Key patterns from `WorkflowStateMachineTests.cs`:

```csharp
[Theory]
[InlineData(WorkflowState.Assigned, WorkflowState.EmployeeInProgress, true)]
[InlineData(WorkflowState.Assigned, WorkflowState.ManagerInProgress, true)]
[InlineData(WorkflowState.Assigned, WorkflowState.BothInProgress, true)]
[InlineData(WorkflowState.Finalized, WorkflowState.InReview, false)]
public void CanTransitionForward_WithVariousStates_ReturnsExpected(
    WorkflowState from,
    WorkflowState to,
    bool expected)
{
    // When
    var result = WorkflowStateMachine.CanTransitionForward(from, to, out var reason);

    // Then
    result.ShouldBe(expected);
    if (!expected)
    {
        reason.Should().NotBeNullOrEmpty();
    }
}
```

### Value Object Testing

```csharp
public class TranslationTests
{
    [Fact]
    public void Constructor_WithValidValues_ShouldSetProperties()
    {
        // Given
        var german = "Deutsch";
        var english = "English";

        // When
        var translation = new Translation(german, english);

        // Then
        translation.German.ShouldBe(german);
        translation.English.ShouldBe(english);
    }

    [Theory]
    [InlineData(null, "English")]
    [InlineData("Deutsch", null)]
    [InlineData("", "English")]
    [InlineData("Deutsch", "")]
    public void Constructor_WithInvalidValues_ShouldThrowArgumentException(
        string german,
        string english)
    {
        // When
        Action act = () => new Translation(german, english);

        // Then
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Given
        var translation1 = new Translation("Deutsch", "English");
        var translation2 = new Translation("Deutsch", "English");

        // When
        var equals = translation1.Equals(translation2);

        // Then
        equals.Should().BeTrue();
        (translation1 == translation2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Given
        var translation1 = new Translation("Deutsch", "English");
        var translation2 = new Translation("Other", "Other");

        // When
        var equals = translation1.Equals(translation2);

        // Then
        equals.Should().BeFalse();
        (translation1 != translation2).Should().BeTrue();
    }
}
```

---

## Application Layer Testing

### Command Handler Testing Pattern

Command handlers orchestrate:
1. Load aggregate from repository
2. Execute domain logic
3. Store aggregate
4. Return Result

**Test Strategy**: Mock the repository, verify domain logic execution and storage calls.

```csharp
using AwesomeAssertions;
using NSubstitute;
using ti8m.BeachBreak.Application.Command.Commands.CategoryCommands;
using ti8m.BeachBreak.Core.Application;
using ti8m.BeachBreak.Domain.CategoryAggregate;
using ti8m.BeachBreak.Domain.SharedKernel;
using Xunit;

namespace ti8m.BeachBreak.Application.Tests.CommandHandlers;

public class CategoryCommandHandlerTests
{
    private readonly ICategoryAggregateRepository _mockRepository;
    private readonly CategoryCommandHandler _handler;

    public CategoryCommandHandlerTests()
    {
        _mockRepository = Substitute.For<ICategoryAggregateRepository>();
        _handler = new CategoryCommandHandler(_mockRepository);
    }

    #region CreateCategoryCommand Tests

    [Fact]
    public async Task HandleAsync_CreateCategory_ShouldStoreAggregate()
    {
        // Given
        var command = new CreateCategoryCommand(
            new CategoryDto
            {
                NameDe = "Test DE",
                NameEn = "Test EN",
                DescriptionDe = "Desc DE",
                DescriptionEn = "Desc EN",
                SortOrder = 1
            });

        // When
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeTrue();

        await _mockRepository.Received(1).StoreAsync(
            Arg.Is<Category>(c =>
                c.Name.German == "Test DE" &&
                c.Name.English == "Test EN" &&
                c.SortOrder == 1),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_CreateCategory_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Given
        var command = new CreateCategoryCommand(new CategoryDto { /* ... */ });
        _mockRepository.StoreAsync(Arg.Any<Category>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // When
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.ShouldContain("Database error");
    }

    #endregion

    #region UpdateCategoryCommand Tests

    [Fact]
    public async Task HandleAsync_UpdateCategory_ShouldLoadModifyAndStore()
    {
        // Given
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category(
            categoryId,
            new Translation("Old DE", "Old EN"),
            new Translation("Desc", "Desc"),
            sortOrder: 1);

        _mockRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(existingCategory);

        var command = new UpdateCategoryCommand(
            categoryId,
            new CategoryDto
            {
                NameDe = "New DE",
                NameEn = "New EN",
                DescriptionDe = "New Desc DE",
                DescriptionEn = "New Desc EN",
                SortOrder = 2
            });

        // When
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeTrue();

        // Verify aggregate was loaded
        await _mockRepository.Received(1).GetByIdAsync(
            categoryId,
            Arg.Any<CancellationToken>());

        // Verify aggregate was modified and stored
        await _mockRepository.Received(1).StoreAsync(
            Arg.Is<Category>(c =>
                c.Id == categoryId &&
                c.Name.German == "New DE" &&
                c.SortOrder == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UpdateNonExistentCategory_ShouldReturnNotFoundFailure()
    {
        // Given
        var categoryId = Guid.NewGuid();
        _mockRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        var command = new UpdateCategoryCommand(categoryId, new CategoryDto { /* ... */ });

        // When
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.ShouldBe(404);
        result.ErrorMessage.ShouldContain("not found");

        // Verify store was NOT called
        await _mockRepository.DidNotReceive().StoreAsync(
            Arg.Any<Category>(),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteCategoryCommand Tests

    [Fact]
    public async Task HandleAsync_DeleteCategory_ShouldCallDeleteOnAggregate()
    {
        // Given
        var categoryId = Guid.NewGuid();
        var category = new Category(
            categoryId,
            new Translation("Test", "Test"),
            new Translation("Desc", "Desc"),
            sortOrder: 1);

        _mockRepository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);

        var command = new DeleteCategoryCommand(categoryId);

        // When
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeTrue();

        // Verify aggregate state changed
        category.IsDeleted.Should().BeTrue();

        // Verify aggregate was stored
        await _mockRepository.Received(1).StoreAsync(
            Arg.Is<Category>(c => c.IsDeleted),
            Arg.Any<CancellationToken>());
    }

    #endregion
}
```

### Query Handler Testing Pattern

```csharp
using AwesomeAssertions;
using Marten;
using NSubstitute;
using ti8m.BeachBreak.Application.Query.Queries.CategoryQueries;
using ti8m.BeachBreak.Application.Query.ReadModels;
using Xunit;

namespace ti8m.BeachBreak.Application.Tests.QueryHandlers;

public class CategoryQueryHandlerTests
{
    private readonly IDocumentSession _mockSession;
    private readonly CategoryQueryHandler _handler;

    public CategoryQueryHandlerTests()
    {
        _mockSession = Substitute.For<IDocumentSession>();
        _handler = new CategoryQueryHandler(_mockSession);
    }

    [Fact]
    public async Task HandleAsync_GetAllCategories_ShouldReturnAllNonDeleted()
    {
        // Given
        var categories = new List<CategoryReadModel>
        {
            new() { Id = Guid.NewGuid(), NameDe = "Cat 1", IsDeleted = false },
            new() { Id = Guid.NewGuid(), NameDe = "Cat 2", IsDeleted = false },
            new() { Id = Guid.NewGuid(), NameDe = "Deleted", IsDeleted = true }
        };

        var queryable = categories.AsQueryable();
        _mockSession.Query<CategoryReadModel>().Returns(queryable);

        var query = new GetAllCategoriesQuery();

        // When
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().NotContain(c => c.IsDeleted);
    }

    [Fact]
    public async Task HandleAsync_GetCategoryById_ShouldReturnCategory()
    {
        // Given
        var categoryId = Guid.NewGuid();
        var category = new CategoryReadModel
        {
            Id = categoryId,
            NameDe = "Test Category",
            IsDeleted = false
        };

        _mockSession.LoadAsync<CategoryReadModel>(categoryId, Arg.Any<CancellationToken>())
            .Returns(category);

        var query = new GetCategoryByIdQuery(categoryId);

        // When
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.ShouldBe(categoryId);
    }

    [Fact]
    public async Task HandleAsync_GetNonExistentCategory_ShouldReturnNotFound()
    {
        // Given
        var categoryId = Guid.NewGuid();
        _mockSession.LoadAsync<CategoryReadModel>(categoryId, Arg.Any<CancellationToken>())
            .Returns((CategoryReadModel?)null);

        var query = new GetCategoryByIdQuery(categoryId);

        // When
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.ShouldBe(404);
    }
}
```

---

## Integration Testing

### Event Sourcing Integration Tests

Test the full event sourcing roundtrip: store aggregate → load aggregate → verify state reconstruction.

```csharp
using AwesomeAssertions;
using Marten;
using ti8m.BeachBreak.Domain.CategoryAggregate;
using ti8m.BeachBreak.Domain.SharedKernel;
using Xunit;

namespace ti8m.BeachBreak.Integration.Tests.EventSourcing;

public class CategoryAggregateRepositoryTests : IAsyncLifetime
{
    private IDocumentStore _store = null!;
    private ICategoryAggregateRepository _repository = null!;

    public async Task InitializeAsync()
    {
        // Setup test database
        _store = DocumentStore.For(opts =>
        {
            opts.Connection("Host=localhost;Database=beachbreak_test;Username=postgres;Password=test");
            opts.Events.StreamIdentity = StreamIdentity.AsGuid;

            // Configure event sourcing for Category
            opts.Events.AddEventType<CategoryCreated>();
            opts.Events.AddEventType<CategoryNameChanged>();
            opts.Events.AddEventType<CategoryDescriptionChanged>();
            opts.Events.AddEventType<CategorySortOrderChanged>();
            opts.Events.AddEventType<CategoryDeleted>();
        });

        // Clean database before each test
        await _store.Advanced.Clean.DeleteAllDocumentsAsync();

        _repository = new CategoryAggregateRepository(_store);
    }

    public async Task DisposeAsync()
    {
        await _store.Advanced.Clean.DeleteAllDocumentsAsync();
        _store.Dispose();
    }

    [Fact]
    public async Task StoreAndLoad_Category_ShouldReconstituteFromEvents()
    {
        // Given
        var categoryId = Guid.NewGuid();
        var category = new Category(
            categoryId,
            new Translation("Test DE", "Test EN"),
            new Translation("Desc DE", "Desc EN"),
            sortOrder: 1);

        // Modify aggregate to create more events
        category.ChangeName(new Translation("Updated DE", "Updated EN"));
        category.ChangeSortOrder(5);

        // When - Store
        await _repository.StoreAsync(category, CancellationToken.None);

        // Then - Load and verify
        var loadedCategory = await _repository.GetByIdAsync(categoryId, CancellationToken.None);

        loadedCategory.Should().NotBeNull();
        loadedCategory!.Id.ShouldBe(categoryId);
        loadedCategory.Name.German.ShouldBe("Updated DE");
        loadedCategory.Name.English.ShouldBe("Updated EN");
        loadedCategory.SortOrder.ShouldBe(5);
        loadedCategory.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Load_NonExistentCategory_ShouldReturnNull()
    {
        // Given
        var nonExistentId = Guid.NewGuid();

        // When
        var category = await _repository.GetByIdAsync(nonExistentId, CancellationToken.None);

        // Then
        category.Should().BeNull();
    }

    [Fact]
    public async Task StoreMultipleChanges_ShouldPersistAllEvents()
    {
        // Given
        var categoryId = Guid.NewGuid();
        var category = new Category(
            categoryId,
            new Translation("Original", "Original"),
            new Translation("Desc", "Desc"),
            sortOrder: 1);

        await _repository.StoreAsync(category, CancellationToken.None);

        // When - Make multiple changes
        var loadedCategory = await _repository.GetByIdAsync(categoryId, CancellationToken.None);
        loadedCategory!.ChangeName(new Translation("Update 1", "Update 1"));
        await _repository.StoreAsync(loadedCategory, CancellationToken.None);

        loadedCategory = await _repository.GetByIdAsync(categoryId, CancellationToken.None);
        loadedCategory!.ChangeName(new Translation("Update 2", "Update 2"));
        loadedCategory.ChangeSortOrder(10);
        await _repository.StoreAsync(loadedCategory, CancellationToken.None);

        // Then - Verify final state
        var finalCategory = await _repository.GetByIdAsync(categoryId, CancellationToken.None);
        finalCategory!.Name.German.ShouldBe("Update 2");
        finalCategory.SortOrder.ShouldBe(10);

        // Verify event count in stream
        using var session = _store.LightweightSession();
        var events = await session.Events.FetchStreamAsync(categoryId);
        events.Should().HaveCountGreaterOrEqualTo(5); // Created + 2 name changes + sort order + more
    }

    [Fact]
    public async Task OptimisticConcurrency_WithConflictingUpdates_ShouldThrow()
    {
        // Given
        var categoryId = Guid.NewGuid();
        var category = new Category(
            categoryId,
            new Translation("Test", "Test"),
            new Translation("Desc", "Desc"),
            sortOrder: 1);

        await _repository.StoreAsync(category, CancellationToken.None);

        // When - Two concurrent loads
        var category1 = await _repository.GetByIdAsync(categoryId, CancellationToken.None);
        var category2 = await _repository.GetByIdAsync(categoryId, CancellationToken.None);

        // First update succeeds
        category1!.ChangeName(new Translation("Update 1", "Update 1"));
        await _repository.StoreAsync(category1, CancellationToken.None);

        // Second update should fail (stale version)
        category2!.ChangeName(new Translation("Update 2", "Update 2"));

        // Then
        Func<Task> act = async () => await _repository.StoreAsync(category2, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>(); // Marten throws on version conflict
    }
}
```

### API Integration Tests

```csharp
using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using ti8m.BeachBreak.CommandApi;
using Xunit;

namespace ti8m.BeachBreak.Integration.Tests.Api;

public class CategoryApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CategoryApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_CreateCategory_ShouldReturn200WithId()
    {
        // Given
        var request = new CreateCategoryRequest
        {
            NameDe = "Test Kategorie",
            NameEn = "Test Category",
            DescriptionDe = "Beschreibung",
            DescriptionEn = "Description",
            SortOrder = 1
        };

        // When
        var response = await _client.PostAsJsonAsync("/api/categories", request);

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CreateCategoryResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GET_AllCategories_ShouldReturnList()
    {
        // When
        var response = await _client.GetAsync("/api/categories");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        categories.Should().NotBeNull();
    }

    [Fact]
    public async Task PUT_UpdateCategory_ShouldReturn200()
    {
        // Given - Create category first
        var createRequest = new CreateCategoryRequest { /* ... */ };
        var createResponse = await _client.PostAsJsonAsync("/api/categories", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>();

        var updateRequest = new UpdateCategoryRequest
        {
            Id = created!.Id,
            NameDe = "Updated DE",
            NameEn = "Updated EN",
            DescriptionDe = "Updated Desc",
            DescriptionEn = "Updated Desc",
            SortOrder = 2
        };

        // When
        var response = await _client.PutAsJsonAsync($"/api/categories/{created.Id}", updateRequest);

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DELETE_Category_ShouldReturn200()
    {
        // Given - Create category first
        var createRequest = new CreateCategoryRequest { /* ... */ };
        var createResponse = await _client.PostAsJsonAsync("/api/categories", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>();

        // When
        var response = await _client.DeleteAsync($"/api/categories/{created!.Id}");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GET_NonExistentCategory_ShouldReturn404()
    {
        // Given
        var nonExistentId = Guid.NewGuid();

        // When
        var response = await _client.GetAsync($"/api/categories/{nonExistentId}");

        // Then
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
```

---

## Test Data Builders

Use the Builder pattern to create test data fluently:

```csharp
public class CategoryTestBuilder
{
    private Guid _id = Guid.NewGuid();
    private Translation _name = new("Test DE", "Test EN");
    private Translation _description = new("Desc DE", "Desc EN");
    private int _sortOrder = 1;

    public CategoryTestBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public CategoryTestBuilder WithName(string german, string english)
    {
        _name = new Translation(german, english);
        return this;
    }

    public CategoryTestBuilder WithDescription(string german, string english)
    {
        _description = new Translation(german, english);
        return this;
    }

    public CategoryTestBuilder WithSortOrder(int sortOrder)
    {
        _sortOrder = sortOrder;
        return this;
    }

    public Category Build()
    {
        return new Category(_id, _name, _description, _sortOrder);
    }
}

// Usage in tests:
var category = new CategoryTestBuilder()
    .WithName("Custom DE", "Custom EN")
    .WithSortOrder(5)
    .Build();
```

---

## Do's and Don'ts

### ✅ DO

1. **DO use Given/When/Then structure**
   ```csharp
   [Fact]
   public void TestName()
   {
       // Given - Setup
       var sut = CreateSystemUnderTest();

       // When - Execute
       var result = sut.DoSomething();

       // Then - Assert
       result.Should().BeTrue();
   }
   ```

2. **DO test one thing per test**
   ```csharp
   // GOOD: One assertion focus
   [Fact]
   public void ChangeName_ShouldRaiseEvent() { /* ... */ }

   [Fact]
   public void ChangeName_ShouldUpdateState() { /* ... */ }

   // BAD: Testing multiple concerns
   [Fact]
   public void ChangeName_ShouldDoEverything()
   {
       // Tests event, state, validation, authorization...
   }
   ```

3. **DO use descriptive test names**
   ```csharp
   // GOOD
   [Fact]
   public void AddGoal_WhenTotalWeightingExceeds100_ShouldReturnFailure()

   // BAD
   [Fact]
   public void TestAddGoal()
   ```

4. **DO use Theory + InlineData for parameterized tests**
   ```csharp
   [Theory]
   [InlineData(WorkflowState.Assigned, true)]
   [InlineData(WorkflowState.Finalized, false)]
   public void CanEdit_WithVariousStates_ReturnsExpected(
       WorkflowState state,
       bool expected)
   {
       // Test logic
   }
   ```

5. **DO use AwesomeAssertions for readability**
   ```csharp
   // GOOD
   result.Should().BeOfType<CategoryNameChanged>()
       .Subject.Name.Should().Be(expectedName);

   // BAD
   Assert.IsType<CategoryNameChanged>(result);
   Assert.Equal(expectedName, ((CategoryNameChanged)result).Name);
   ```

6. **DO test event application logic**
   ```csharp
   [Fact]
   public void Apply_CategoryCreated_ShouldSetAllProperties()
   {
       // This is CRITICAL for event sourcing
   }
   ```

7. **DO test business rules and invariants**
   ```csharp
   [Fact]
   public void AddGoal_WhenWeightingExceeds100_ShouldFail()
   {
       // Protects domain invariants
   }
   ```

8. **DO mock at boundaries**
   ```csharp
   // Mock repositories, not domain objects
   var mockRepo = Substitute.For<ICategoryAggregateRepository>();
   ```

9. **DO use async/await correctly**
   ```csharp
   [Fact]
   public async Task HandleAsync_ShouldCompleteSuccessfully()
   {
       var result = await handler.HandleAsync(command);
       result.Should().BeTrue();
   }
   ```

10. **DO clean up in integration tests**
    ```csharp
    public async Task DisposeAsync()
    {
        await _store.Advanced.Clean.DeleteAllDocumentsAsync();
        _store.Dispose();
    }
    ```

### ❌ DON'T

1. **DON'T test framework code**
   ```csharp
   // BAD: Testing AutoMapper, EF Core, etc.
   [Fact]
   public void AutoMapper_ShouldMapCorrectly() { /* ... */ }
   ```

2. **DON'T write brittle tests**
   ```csharp
   // BAD: Testing implementation details
   [Fact]
   public void ShouldCallPrivateMethodInSpecificOrder() { /* ... */ }

   // GOOD: Test public behavior
   [Fact]
   public void ShouldProduceCorrectResult() { /* ... */ }
   ```

3. **DON'T mock everything**
   ```csharp
   // BAD: Over-mocking
   var mockTranslation = Substitute.For<ITranslation>();
   var mockGuid = Substitute.For<IGuid>();

   // GOOD: Use real value objects
   var translation = new Translation("DE", "EN");
   var id = Guid.NewGuid();
   ```

4. **DON'T ignore test failures**
   ```csharp
   // BAD: Commenting out failing tests
   // [Fact]
   // public void BrokenTest() { /* ... */ }

   // GOOD: Fix or document with [Fact(Skip = "Reason")]
   [Fact(Skip = "Known issue #1234 - fix in progress")]
   public void BrokenTest() { /* ... */ }
   ```

5. **DON'T test private methods directly**
   ```csharp
   // BAD: Using reflection to test private methods
   var privateMethod = typeof(Category).GetMethod("ValidateWeighting",
       BindingFlags.NonPublic | BindingFlags.Instance);

   // GOOD: Test through public API
   var result = category.AddGoal(invalidGoal);
   result.IsSuccess.Should().BeFalse();
   ```

6. **DON'T use production data in tests**
   ```csharp
   // BAD: Connecting to production database
   var connectionString = "Host=prod.database.com;...";

   // GOOD: Use test database or in-memory
   var connectionString = "Host=localhost;Database=beachbreak_test;...";
   ```

7. **DON'T write slow tests without reason**
   ```csharp
   // BAD: Unnecessary delays
   [Fact]
   public async Task Test()
   {
       await Task.Delay(5000); // Why?
   }

   // GOOD: Fast, focused tests
   [Fact]
   public void Test()
   {
       // Executes in milliseconds
   }
   ```

8. **DON'T share state between tests**
   ```csharp
   // BAD: Static state
   public class Tests
   {
       private static Category _sharedCategory; // Shared across tests!

       [Fact]
       public void Test1() { _sharedCategory = ...; }

       [Fact]
       public void Test2() { _sharedCategory.ChangeName(...); } // Depends on Test1!
   }

   // GOOD: Fresh state per test
   public class Tests
   {
       [Fact]
       public void Test1() { var category = CreateCategory(); /* ... */ }

       [Fact]
       public void Test2() { var category = CreateCategory(); /* ... */ }
   }
   ```

9. **DON'T test multiple aggregates together (in unit tests)**
   ```csharp
   // BAD: Testing Category + Employee + Assignment together
   [Fact]
   public void ComplexInteraction() { /* ... */ }

   // GOOD: Test each aggregate separately
   // Use integration tests for cross-aggregate workflows
   ```

10. **DON'T assert on implementation details**
    ```csharp
    // BAD: Asserting internal state
    Assert.Equal(3, category.GetPrivateFieldValue("_version"));

    // GOOD: Assert on observable behavior
    category.Version.ShouldBe(3);
    ```

---

## Conventions & Patterns

### Test Naming Convention

**Pattern**: `MethodName_StateUnderTest_ExpectedBehavior`

```csharp
// Good examples:
public void AddGoal_WithValidWeighting_ShouldRaiseGoalAddedEvent()
public void AddGoal_WhenTotalWeightingExceeds100_ShouldReturnFailure()
public void Submit_WhenNotAllSectionsComplete_ShouldReturnFailure()
public void CanEmployeeEdit_WhenInReviewState_ShouldReturnFalse()

// Alternative (BDD style):
public void Given_ValidGoalWeighting_When_AddingGoal_Then_ShouldSucceed()
```

### Test File Organization

```csharp
public class CategoryTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldRaiseCategoryCreatedEvent() { }

    #endregion

    #region ChangeName Tests

    [Fact]
    public void ChangeName_WithValidName_ShouldRaiseEvent() { }

    [Fact]
    public void ChangeName_WithNullName_ShouldThrowArgumentNullException() { }

    #endregion

    #region Helper Methods

    private static Category CreateCategory() { }

    #endregion
}
```

### AwesomeAssertions Package

**Important**: We use the **AwesomeAssertions** NuGet package instead of AwesomeAssertions to avoid licensing restrictions.

**Key Points**:
- **Package to install**: `AwesomeAssertions` version 7.0.0
- **Namespace to use**: `using AwesomeAssertions;` (the package installs AwesomeAssertions DLLs)
- **Syntax**: Standard AwesomeAssertions syntax with `.Should()` methods
- **License**: Free and open-source (no commercial license required)
- **Repository**: https://github.com/AwesomeAssertions/AwesomeAssertions

AwesomeAssertions is a repackaging of AwesomeAssertions that provides the same functionality without licensing restrictions. This means:
- Use `AwesomeAssertions` in your .csproj file
- Use `using AwesomeAssertions;` in your test files
- Use all standard AwesomeAssertions assertion methods

### AwesomeAssertions Patterns

```csharp
// Assertions
result.Should().BeTrue();
result.Should().BeFalse();
value.Should().Be(expected);
value.Should().NotBe(unexpected);

// Nullability
result.Should().NotBeNull();
result.Should().BeNull();

// Collections
list.Should().HaveCount(3);
list.Should().ContainSingle();
list.Should().Contain(item);
list.Should().NotContain(item);
list.Should().BeEmpty();

// Strings
str.Should().Be("expected");
str.Should().Contain("substring");
str.Should().StartWith("prefix");
str.Should().BeNullOrEmpty();

// Exceptions
Action act = () => method();
act.Should().Throw<ArgumentNullException>()
    .WithParameterName("paramName")
    .WithMessage("*expected message*");

// Type assertions
result.Should().BeOfType<CategoryCreated>();
result.Should().BeAssignableTo<IDomainEvent>();

// Chaining
events.Should().ContainSingle()
    .Which.Should().BeOfType<CategoryCreated>()
    .Subject.Name.Should().Be(expectedName);

// Result pattern
result.IsSuccess.Should().BeTrue();
result.IsSuccess.Should().BeFalse();
result.ErrorMessage.ShouldContain("error");
result.StatusCode.ShouldBe(404);
```

### Exception Testing

```csharp
// Test exception is thrown
[Fact]
public void Method_WithInvalidInput_ShouldThrowException()
{
    // Given
    var sut = CreateSut();

    // When
    Action act = () => sut.InvalidOperation();

    // Then
    act.Should().Throw<InvalidOperationException>()
        .WithMessage("*specific message*");
}

// Test exception type and properties
[Fact]
public void Method_ShouldThrowWithCorrectParameters()
{
    // When
    Action act = () => new Category(id: Guid.Empty, null!, null!, 0);

    // Then
    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("name");
}
```

### Async Test Patterns

```csharp
// Async method testing
[Fact]
public async Task HandleAsync_ShouldCompleteSuccessfully()
{
    // When
    var result = await handler.HandleAsync(command);

    // Then
    result.Should().BeTrue();
}

// Async exception testing
[Fact]
public async Task HandleAsync_ShouldThrowException()
{
    // When
    Func<Task> act = async () => await handler.HandleAsync(invalidCommand);

    // Then
    await act.Should().ThrowAsync<InvalidOperationException>();
}
```

---

## Priority Matrix

### Critical Priority (Start Here)

**Goal**: Establish testing infrastructure and protect core domain logic

1. **Setup Test Projects** (2-4 hours)
   - Create .csproj files
   - Add to solution
   - Configure NuGet packages
   - Verify `dotnet test` works

2. **Category Aggregate Tests** (4-6 hours) - **START HERE**
   - Simplest aggregate (112 lines)
   - Learn the patterns
   - Establish conventions
   - Reference: 11 methods, 6 events to test

3. **WorkflowStateMachine Tests** (Already Done ✓)
   - Use as template for other domain services
   - 293 lines of excellent test coverage

4. **QuestionnaireAssignment Aggregate Tests** (20-40 hours)
   - **MOST IMPORTANT** - highest complexity (1362 lines)
   - 25+ methods, 20+ events
   - Complex workflow state transitions
   - Goal management with business rules
   - Authorization logic
   - Break into multiple test files by concern

### High Priority (Next Phase)

5. **Other Aggregate Tests** (30-50 hours)
   - Employee aggregate (13+ methods, 10+ events)
   - QuestionnaireTemplate aggregate (10+ methods, 10+ events)
   - Organization aggregate
   - QuestionnaireResponse aggregate

6. **Command Handler Tests** (40-60 hours)
   - 30+ command handlers to test
   - Start with Category handlers (simplest)
   - Move to QuestionnaireAssignment handlers
   - Mock repositories
   - Test error handling

7. **Query Handler Tests** (30-50 hours)
   - 20+ query handlers
   - Dashboard queries (high value)
   - Employee/Manager queries
   - Mock IDocumentSession

### Medium Priority (After Foundation)

8. **Integration Tests - Event Sourcing** (20-30 hours)
   - Category repository roundtrip
   - QuestionnaireAssignment repository roundtrip
   - Optimistic concurrency tests
   - Event stream verification

9. **Integration Tests - API** (20-30 hours)
   - Category endpoints
   - QuestionnaireAssignment endpoints
   - Authorization tests
   - Error response tests

10. **Value Object Tests** (5-10 hours)
    - Translation
    - Goal, GoalRating
    - SectionProgress

### Low Priority (Future)

11. **Frontend Component Tests** (80-120 hours)
    - Blazor components with bUnit
    - OptimizedQuestionRenderer
    - DynamicQuestionnaire
    - Dashboard components
    - 400+ .razor files

12. **E2E Tests** (40-60 hours)
    - Playwright browser automation
    - Critical user workflows
    - Cross-browser testing

---

## Complete Code Examples

### Example 1: Complete Category Aggregate Test Suite

See [Domain Layer Testing](#domain-layer-testing) section for complete examples.

### Example 2: Complete Command Handler Test Suite

See [Application Layer Testing](#application-layer-testing) section for complete examples.

### Example 3: Complete Integration Test

See [Integration Testing](#integration-testing) section for complete examples.

---

## References

### Internal References

1. **Existing Test Example**: `Tests/ti8m.BeachBreak.Domain.Tests/WorkflowStateMachineTests.cs`
   - Gold standard for test structure
   - Theory + InlineData usage
   - Clear organization

2. **Domain Aggregates to Test**:
   - `01_Domain/ti8m.BeachBreak.Domain/CategoryAggregate/Category.cs` (simplest)
   - `01_Domain/ti8m.BeachBreak.Domain/QuestionnaireAssignmentAggregate/QuestionnaireAssignment.cs` (most complex)

3. **Command Handlers to Test**:
   - `02_Application/ti8m.BeachBreak.Application.Command/Commands/CategoryCommands/CategoryCommandHandler.cs`

4. **CLAUDE.md**: Reference for architecture patterns and conventions

### External References

1. **xUnit Documentation**: https://xunit.net/
   - [Getting Started](https://xunit.net/docs/getting-started/netcore/cmdline)
   - [Theory and InlineData](https://andrewlock.net/creating-parameterised-tests-in-xunit-with-inlinedata-classdata-and-memberdata/)

2. **AwesomeAssertions**: https://AwesomeAssertions.com/
   - [Introduction](https://AwesomeAssertions.com/introduction)
   - [Tips and Tricks](https://AwesomeAssertions.com/tips/)

3. **NSubstitute**: https://nsubstitute.github.io/
   - [Getting Started](https://nsubstitute.github.io/help/getting-started/)
   - [Argument Matchers](https://nsubstitute.github.io/help/argument-matchers/)

4. **Marten Testing**: https://martendb.io/
   - [Testing](https://martendb.io/guide/integration/)
   - [Event Store Testing](https://martendb.io/events/testing/)

5. **Event Sourcing Testing Patterns**:
   - [Testing Event-Sourced Aggregates](https://buildplease.com/pages/fpc-10/)
   - [CQRS Testing Strategies](https://www.eventstore.com/blog/testing-event-sourced-systems)

6. **ASP.NET Core Integration Testing**:
   - [Microsoft Docs](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
   - [WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests#basic-tests-with-the-default-webapplicationfactory)

---

## Roadmap

### Phase 1: Foundation (Week 1)

**Effort**: 6-10 hours

**Tasks**:
1. ✅ Rescue existing WorkflowStateMachineTests.cs
   - Create ti8m.BeachBreak.Domain.Tests.csproj
   - Add xUnit, AwesomeAssertions, NSubstitute packages
   - Add to solution
   - Verify `dotnet test` works

2. ✅ Create remaining test projects
   - ti8m.BeachBreak.Application.Tests
   - ti8m.BeachBreak.Integration.Tests
   - Configure packages
   - Add to solution

3. ✅ Setup CI/CD pipeline
   - Add test step to build pipeline
   - Configure code coverage reporting
   - Set coverage thresholds (start at 20%, increase over time)

**Success Criteria**:
- All test projects build successfully
- `dotnet test` executes (even with 0 tests)
- CI pipeline runs tests automatically

### Phase 2: Domain Tests (Weeks 2-4)

**Effort**: 60-100 hours

**Tasks**:
1. ✅ Category Aggregate Tests (START HERE)
   - Test all 11 public methods
   - Test all 6 event Apply methods
   - Test business rules
   - Establish team patterns
   - **Target**: 100% aggregate coverage

2. ✅ QuestionnaireAssignment Aggregate Tests (HIGHEST VALUE)
   - Break into multiple test files:
     - GoalManagementTests.cs
     - WorkflowStateTransitionTests.cs
     - AuthorizationTests.cs
     - SectionProgressTests.cs
   - Test all 25+ methods
   - Test all 20+ events
   - **Target**: 80%+ coverage (complex aggregate)

3. ✅ Other Aggregate Tests
   - Employee aggregate
   - QuestionnaireTemplate aggregate
   - Organization aggregate
   - QuestionnaireResponse aggregate
   - **Target**: 80%+ coverage each

4. ✅ Value Object Tests
   - Translation, Goal, GoalRating, SectionProgress
   - **Target**: 100% coverage (simple)

**Success Criteria**:
- All domain aggregates have comprehensive tests
- Team follows established patterns
- Domain logic bugs caught before production
- Coverage: 70%+ for domain layer

### Phase 3: Application Tests (Weeks 5-7)

**Effort**: 70-110 hours

**Tasks**:
1. ✅ Command Handler Tests
   - Category handlers (start here)
   - Employee handlers
   - QuestionnaireAssignment handlers (most important)
   - QuestionnaireTemplate handlers
   - Organization handlers
   - **Target**: 80%+ coverage

2. ✅ Query Handler Tests
   - Dashboard queries (high value)
   - Employee/Manager queries
   - Category/Template queries
   - **Target**: 70%+ coverage

**Success Criteria**:
- All command handlers tested with success/failure paths
- All query handlers tested
- Repository interactions verified
- Coverage: 60%+ for application layer

### Phase 4: Integration Tests (Weeks 8-10)

**Effort**: 40-70 hours

**Tasks**:
1. ✅ Event Sourcing Integration Tests
   - Category aggregate persistence
   - QuestionnaireAssignment persistence
   - Event stream verification
   - Optimistic concurrency tests
   - **Target**: Core aggregates covered

2. ✅ API Integration Tests
   - Category endpoints
   - QuestionnaireAssignment endpoints
   - Authorization tests
   - Error response tests
   - **Target**: Critical endpoints covered

**Success Criteria**:
- Event sourcing roundtrips verified
- API endpoints tested end-to-end
- Database fixtures working
- Coverage: 40%+ for infrastructure layer

### Phase 5: Frontend Tests (Future - Weeks 11+)

**Effort**: 100-150 hours

**Tasks**:
1. Setup bUnit for Blazor testing
2. Test critical components:
   - OptimizedQuestionRenderer
   - DynamicQuestionnaire
   - Dashboard components
3. Component interaction tests
4. State management tests

**Success Criteria**:
- Critical user paths tested
- Component rendering verified
- Coverage: 30%+ for frontend

---

## Success Metrics

### Code Coverage Goals

- **Domain Layer**: 70%+ (higher priority)
- **Application Layer**: 60%+
- **Infrastructure Layer**: 40%+
- **Frontend Layer**: 30%+ (future)
- **Overall**: 50%+

### Quality Metrics

- **Test Execution Time**: < 5 minutes for all unit tests
- **Test Reliability**: 0 flaky tests
- **Build Pipeline**: Tests run on every commit
- **Documentation**: All complex tests include comments explaining intent

---

## Getting Started Checklist

Ready to start testing? Follow this checklist:

- [ ] Read this entire document
- [ ] Review existing `WorkflowStateMachineTests.cs`
- [ ] Create test project structure
- [ ] Add NuGet packages
- [ ] Add projects to solution
- [ ] Verify `dotnet test` works
- [ ] Write first test for Category aggregate
- [ ] Get test reviewed by team
- [ ] Establish team conventions
- [ ] Begin systematic testing of remaining aggregates

---

## Questions?

If you have questions about testing:
1. Reference this document
2. Look at `WorkflowStateMachineTests.cs` for examples
3. Check CLAUDE.md for architectural patterns
4. Ask the team lead

**Remember**: Testing is not optional. It's part of writing production code.

---

*Last Updated: 2025-11-10*
*Version: 1.0*
*Authors: Claude Code + ti8m Development Team*
