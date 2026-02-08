# Domain Aggregate Testing Framework

## Overview

This testing framework provides a comprehensive, standardized approach for testing domain aggregates in the ti8m BeachBreak event sourcing system. The framework follows the **Given/When/Then** pattern and provides fluent assertions for domain events, making tests readable and maintainable.

## Key Features

- **Given/When/Then Pattern**: Clear test structure that reads like specifications
- **Event Sourcing Support**: Native support for applying historical events and testing event streams
- **Fluent Assertions**: Rich, readable assertions for domain events and exceptions
- **Type Safety**: Compile-time safety for all event and aggregate testing
- **Performance Focused**: Lightweight test execution suitable for large test suites

## Quick Start

### Basic Test Structure

```csharp
[TestFixture]
public class YourAggregateTests : AggregateTestBase<YourAggregate>
{
    [SetUp]
    public void Setup()
    {
        SetUp(); // Call base setup
    }

    [Test]
    public void YourScenario_WithCondition_ExpectedBehavior()
    {
        // Given - set up initial state
        Given(new SomeEventThatHappened(aggregateId, "initial data"));

        // When - execute the operation
        When(() => Aggregate.DoSomething("new data"));

        // Then - assert expected events were raised
        ThenEventWasRaised<SomethingHappened>()
            .WithProperty(e => e.Data, "new data");
    }
}
```

## Framework Components

### 1. AggregateTestBase&lt;T&gt;

The core base class that provides the Given/When/Then infrastructure.

```csharp
public abstract class AggregateTestBase<TAggregate> where TAggregate : AggregateRoot
```

**Key Methods:**
- `Given(events)` - Apply historical events to set up aggregate state
- `When(action)` - Execute command and capture events/exceptions
- `Then*()` - Various assertion methods for events and state

### 2. EventAssertion&lt;T&gt;

Fluent assertion methods for individual domain events.

```csharp
ThenEventWasRaised<CategoryCreated>()
    .WithProperty(e => e.Name, "Expected Name")
    .WithAggregateId(expectedId)
    .WithTimestampNear(e => e.CreatedDate)
    .WithNonEmptyString(e => e.SomeTextField);
```

### 3. MultiEventAssertion&lt;T&gt;

Assertion methods for multiple events of the same type.

```csharp
ThenEventsWereRaised<ItemAdded>(3)
    .WithAllSatisfying(e => !string.IsNullOrEmpty(e.Name))
    .AtIndex(0).WithProperty(e => e.Order, 1)
    .AtIndex(1).WithProperty(e => e.Order, 2);
```

### 4. ExceptionAssertion&lt;T&gt;

Assertion methods for exceptions thrown during command execution.

```csharp
ThenExceptionWasThrown<ArgumentNullException>()
    .WithMessage("name")
    .With(ex => ex.ParamName == "name");
```

## Testing Patterns

### 1. Simple Command Testing

Test individual aggregate commands in isolation:

```csharp
[Test]
public void ChangeName_WithValidName_RaisesNameChangedEvent()
{
    // Given - aggregate exists
    Given(new CategoryCreated(categoryId, "Original Name", DateTime.UtcNow));

    // When - name is changed
    When(() => Aggregate.ChangeName("New Name"));

    // Then - event is raised
    ThenEventWasRaised<CategoryNameChanged>()
        .WithProperty(e => e.Name, "New Name");
}
```

### 2. Business Rule Validation

Test business rules and validation logic:

```csharp
[Test]
public void ChangeName_OnDeletedCategory_ThrowsInvalidOperationException()
{
    // Given - deleted category
    Given(
        new CategoryCreated(categoryId, "Name", DateTime.UtcNow),
        new CategoryDeleted()
    );

    // When - attempting to change name
    When(() => Aggregate.ChangeName("New Name"));

    // Then - operation is rejected
    ThenExceptionWasThrown<InvalidOperationException>()
        .WithMessage("Cannot modify deleted category");
}
```

### 3. Async Operations with Dependencies

Test operations that depend on external services:

```csharp
[Test]
public async Task ChangeProcessType_WithActiveAssignments_ThrowsException()
{
    // Given - template with assignments
    Given(CreateTemplateCreatedEvent());

    assignmentServiceMock
        .Setup(x => x.HasActiveAssignmentsAsync(templateId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);

    // When - attempting to change process type
    await When(async () =>
        await Aggregate.ChangeProcessTypeAsync(newType, assignmentServiceMock.Object));

    // Then - operation is rejected
    ThenExceptionWasThrown<InvalidOperationException>()
        .WithMessage("Cannot change process type");
}
```

### 4. Event Sourcing and State Reconstruction

Test that events properly reconstruct aggregate state:

```csharp
[Test]
public void EventReplay_WithComplexHistory_RestoresCorrectState()
{
    // Given - complete event history
    Given(
        new TemplateCreated(id, name, categoryId, DateTime.UtcNow),
        new TemplateNameChanged(newName),
        new TemplatePublished(publisherId, DateTime.UtcNow),
        new TemplateArchived()
    );

    // When - no additional commands (testing replay only)

    // Then - state reflects all events
    ThenAggregateState(template =>
    {
        Assert.That(template.Name, Is.EqualTo(newName));
        Assert.That(template.Status, Is.EqualTo(TemplateStatus.Archived));
    });

    Assert.That(AggregateVersion, Is.EqualTo(4));
}
```

### 5. Complex Workflows

Test complete business workflows:

```csharp
[Test]
public void TemplateLifecycle_CreatePublishArchive_RaisesCorrectEventSequence()
{
    // When - complete workflow
    When(() =>
    {
        var template = new QuestionnaireTemplate(id, name, description, categoryId);
        template.Publish(publisherId);
        template.Archive();
    });

    // Then - events in correct order
    ThenEventsWereRaisedInOrder(
        typeof(QuestionnaireTemplateCreated),
        typeof(QuestionnaireTemplatePublished),
        typeof(QuestionnaireTemplateArchived)
    );
}
```

## Advanced Features

### Custom Aggregate Construction

Override `CreateEmptyAggregate()` for aggregates with special construction requirements:

```csharp
protected override YourAggregate CreateEmptyAggregate()
{
    return new YourAggregate(specialDependency);
}
```

### Custom Event Assertions

Access the event collection directly for complex assertions:

```csharp
var raisedEvents = GetRaisedEvents();
var specificEvents = raisedEvents.OfType<YourEvent>().ToList();

Assert.That(specificEvents.Count, Is.EqualTo(expectedCount));
Assert.That(specificEvents.All(e => e.SomeProperty > 0), Is.True);
```

### Performance Testing

Test performance characteristics of your aggregates:

```csharp
[Test]
public void BulkOperations_OptimizesEventGeneration()
{
    // Given - aggregate with items
    Given(CreateAggregateWithManyItems());

    // When - bulk operation
    When(() => Aggregate.BulkUpdate(manyUpdates));

    // Then - events are optimized (not one per item)
    ThenEventCountIs(1); // Single bulk update event, not many individual events
}
```

## Best Practices

### 1. Test Naming

Use descriptive test names that follow the pattern:
```
MethodName_WithCondition_ExpectedBehavior
```

Examples:
- `CreateCategory_WithValidName_RaisesCategoryCreatedEvent`
- `ChangeName_OnDeletedCategory_ThrowsInvalidOperationException`
- `Publish_OnDraftTemplate_RaisesPublishedEvent`

### 2. Given Setup

- Use `Given()` to establish aggregate state through events
- Prefer realistic event sequences over minimal setups
- Create helper methods for common event patterns

```csharp
private QuestionnaireTemplateCreated CreateTemplateCreatedEvent(
    Translation? name = null,
    QuestionnaireProcessType processType = QuestionnaireProcessType.PerformanceReview)
{
    return new QuestionnaireTemplateCreated(/* ... */);
}
```

### 3. When Actions

- Keep `When()` blocks focused on a single operation
- Use async `When()` for operations that return Task
- Test both success and failure scenarios

### 4. Then Assertions

- Start with specific event assertions, then check aggregate state
- Use `ThenAggregateState()` for complex state validation
- Prefer specific assertions over generic ones

### 5. Test Organization

Group related tests using regions or nested classes:

```csharp
#region Creation Tests
// Tests for aggregate creation
#endregion

#region Modification Tests
// Tests for aggregate modifications
#endregion

#region Business Rules
// Tests for complex business logic
#endregion
```

### 6. Mock Usage

- Use mocks sparingly, only for external dependencies
- Prefer testing business logic without mocks when possible
- Verify important service interactions

```csharp
// Verify service was called with correct parameters
assignmentServiceMock.Verify(
    x => x.HasActiveAssignmentsAsync(expectedId, It.IsAny<CancellationToken>()),
    Times.Once);
```

## Testing Categories

### Unit Tests
- Single aggregate behavior
- Individual command testing
- Business rule validation
- Event application logic

### Integration Tests
- Multi-aggregate scenarios
- Service dependency interactions
- Complex workflow testing

### Projection Tests
- Read model consistency
- Event replay accuracy
- Version tracking

## Performance Guidelines

### Test Execution Performance

- Tests should complete in **< 100ms** per aggregate test
- Use `[SetUp]` for common initialization, not `[OneTimeSetUp]`
- Avoid heavy external dependencies in unit tests

### Memory Usage

- Framework creates minimal overhead per test
- Event collections are cleared between tests
- Mock objects are recreated for each test

## Common Patterns

### Testing Idempotent Operations

```csharp
[Test]
public void ChangeName_WithSameName_RaisesNoEvents()
{
    // Given - category with specific name
    Given(new CategoryCreated(id, "Current Name", DateTime.UtcNow));

    // When - "changing" to same name
    When(() => Aggregate.ChangeName("Current Name"));

    // Then - no events (optimization)
    ThenNoEventsWereRaised();
}
```

### Testing Validation Logic

```csharp
[Test]
[TestCase("")]
[TestCase("   ")]
[TestCase(null)]
public void ChangeName_WithInvalidName_ThrowsArgumentException(string invalidName)
{
    // Given - valid category
    Given(CreateCategoryCreatedEvent());

    // When - changing to invalid name
    When(() => Aggregate.ChangeName(invalidName));

    // Then - validation fails
    ThenExceptionWasThrown<ArgumentException>()
        .WithMessage("name");
}
```

### Testing Event Ordering

```csharp
[Test]
public void ComplexOperation_RaisesEventsInCorrectOrder()
{
    // When - complex operation
    When(() => Aggregate.PerformComplexOperation());

    // Then - events in specific order
    ThenEventsWereRaisedInOrder(
        typeof(OperationStarted),
        typeof(DataValidated),
        typeof(ChangeApplied),
        typeof(OperationCompleted)
    );
}
```

## Troubleshooting

### Common Issues

1. **Apply Method Not Found**
   ```
   Aggregate YourAggregate does not have an Apply method for event YourEvent
   ```
   - Ensure all events have corresponding `Apply(YourEvent @event)` methods
   - Check that event types match exactly

2. **Aggregate Construction Failed**
   ```
   Could not create instance of YourAggregate
   ```
   - Add parameterless constructor (can be private)
   - Or override `CreateEmptyAggregate()` method

3. **Event Assertion Failed**
   ```
   Expected event of type YourEvent but no such event was raised
   ```
   - Check that the command actually raises the expected event
   - Verify event is added with `RaiseEvent()` not just applied

### Debugging Tips

- Use `GetRaisedEvents()` to examine all events raised during When
- Use `GetGivenEvents()` to verify historical event setup
- Check `AggregateVersion` to ensure events are being applied
- Add console output for complex debugging scenarios

## Examples

See the following example test files:
- `CategoryAggregate/CategoryTests.cs` - Simple aggregate testing
- `QuestionnaireTemplateAggregate/QuestionnaireTemplateTests.cs` - Complex business logic testing

## Framework Architecture

The testing framework is built on these principles:

1. **Event Sourcing First**: Designed specifically for event-sourced aggregates
2. **Type Safety**: Compile-time safety for all operations
3. **Fluent Interface**: Readable, chainable assertions
4. **Performance**: Lightweight execution suitable for large test suites
5. **Extensibility**: Easy to extend for custom scenarios

## Contributing

When extending the framework:

1. Maintain the Given/When/Then pattern
2. Add fluent assertion methods to EventAssertion classes
3. Include comprehensive documentation and examples
4. Ensure all new features have their own tests
5. Follow the existing naming conventions