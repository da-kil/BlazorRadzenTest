using System.Reflection;
using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.Tests.Common;

/// <summary>
/// Base class for testing domain aggregates using Given/When/Then pattern.
/// Provides event sourcing test infrastructure with fluent assertions.
///
/// Usage:
/// - Given: Set up initial aggregate state by applying historical events
/// - When: Execute a business operation (command method)
/// - Then: Assert that expected events were raised with correct payload
///
/// Example:
/// public class CategoryTests : AggregateTestBase&lt;Category&gt;
/// {
///     [Test]
///     public void ChangeName_WithValidName_RaisesNameChangedEvent()
///     {
///         // Given - category exists
///         Given(new CategoryCreated(CategoryId, "Original Name", DateTime.UtcNow));
///
///         // When - name is changed
///         When(() => Aggregate.ChangeName("New Name"));
///
///         // Then - event is raised with new name
///         ThenEventWasRaised&lt;CategoryNameChanged&gt;()
///             .WithPayload(e => e.Name == "New Name");
///     }
/// }
/// </summary>
/// <typeparam name="TAggregate">The aggregate type to test</typeparam>
public abstract class AggregateTestBase<TAggregate>
    where TAggregate : AggregateRoot
{
    protected TAggregate Aggregate { get; private set; } = default!;
    private List<IDomainEvent> givenEvents = new();
    private List<IDomainEvent> whenEvents = new();
    private Exception? whenException;

    /// <summary>
    /// Sets up the test by creating a new aggregate instance.
    /// Call this in [SetUp] or test constructor.
    /// </summary>
    protected virtual void SetUp()
    {
        Aggregate = CreateEmptyAggregate();
        givenEvents.Clear();
        whenEvents.Clear();
        whenException = null;
    }

    #region Given - Historical Event Setup

    /// <summary>
    /// Applies a historical event to set up initial aggregate state.
    /// Use this to arrange the aggregate in a specific state before testing.
    /// </summary>
    protected void Given(IDomainEvent @event)
    {
        ApplyEventToAggregate(@event);
        givenEvents.Add(@event);

        // Clear uncommitted events after applying given events
        Aggregate.ClearUncommittedDomainEvents();
    }

    /// <summary>
    /// Applies multiple historical events to set up initial aggregate state.
    /// Events are applied in the order provided.
    /// </summary>
    protected void Given(params IDomainEvent[] events)
    {
        foreach (var @event in events)
        {
            Given(@event);
        }
    }

    /// <summary>
    /// Applies multiple historical events from an enumerable.
    /// </summary>
    protected void Given(IEnumerable<IDomainEvent> events)
    {
        foreach (var @event in events)
        {
            Given(@event);
        }
    }

    #endregion

    #region When - Command Execution

    /// <summary>
    /// Executes a command/operation on the aggregate and captures resulting events.
    /// Use this to perform the action you want to test.
    /// </summary>
    protected void When(Action action)
    {
        try
        {
            whenException = null;

            // Execute the command
            action();

            // Capture events that were raised
            whenEvents.AddRange(Aggregate.UncommittedEvents);
        }
        catch (Exception ex)
        {
            whenException = ex;
            // Still capture any events that might have been raised before the exception
            whenEvents.AddRange(Aggregate.UncommittedEvents);
        }
    }

    /// <summary>
    /// Executes an async command/operation on the aggregate and captures resulting events.
    /// </summary>
    protected async Task When(Func<Task> asyncAction)
    {
        try
        {
            whenException = null;

            // Execute the async command
            await asyncAction();

            // Capture events that were raised
            whenEvents.AddRange(Aggregate.UncommittedEvents);
        }
        catch (Exception ex)
        {
            whenException = ex;
            // Still capture any events that might have been raised before the exception
            whenEvents.AddRange(Aggregate.UncommittedEvents);
        }
    }

    #endregion

    #region Then - Event Assertions

    /// <summary>
    /// Asserts that a specific event type was raised during the When phase.
    /// Returns EventAssertion for further payload validation.
    /// </summary>
    protected EventAssertion<TEvent> ThenEventWasRaised<TEvent>()
        where TEvent : IDomainEvent
    {
        var eventsOfType = whenEvents.OfType<TEvent>().ToList();

        if (!eventsOfType.Any())
        {
            var actualEventTypes = string.Join(", ", whenEvents.Select(e => e.GetType().Name));
            throw new AssertionException(
                $"Expected event of type {typeof(TEvent).Name} but no such event was raised. " +
                $"Actual events: [{actualEventTypes}]");
        }

        if (eventsOfType.Count > 1)
        {
            throw new AssertionException(
                $"Expected exactly one event of type {typeof(TEvent).Name} but {eventsOfType.Count} were raised.");
        }

        return new EventAssertion<TEvent>(eventsOfType.First());
    }

    /// <summary>
    /// Asserts that multiple events of a specific type were raised.
    /// Returns MultiEventAssertion for further validation.
    /// </summary>
    protected MultiEventAssertion<TEvent> ThenEventsWereRaised<TEvent>(int expectedCount)
        where TEvent : IDomainEvent
    {
        var eventsOfType = whenEvents.OfType<TEvent>().ToList();

        if (eventsOfType.Count != expectedCount)
        {
            throw new AssertionException(
                $"Expected {expectedCount} events of type {typeof(TEvent).Name} but {eventsOfType.Count} were raised.");
        }

        return new MultiEventAssertion<TEvent>(eventsOfType);
    }

    /// <summary>
    /// Asserts that no events were raised during the When phase.
    /// </summary>
    protected void ThenNoEventsWereRaised()
    {
        if (whenEvents.Any())
        {
            var eventTypes = string.Join(", ", whenEvents.Select(e => e.GetType().Name));
            throw new AssertionException(
                $"Expected no events to be raised, but the following events were raised: [{eventTypes}]");
        }
    }

    /// <summary>
    /// Asserts that exactly the specified number of events were raised.
    /// </summary>
    protected void ThenEventCountIs(int expectedCount)
    {
        if (whenEvents.Count != expectedCount)
        {
            var eventTypes = string.Join(", ", whenEvents.Select(e => e.GetType().Name));
            throw new AssertionException(
                $"Expected {expectedCount} events but {whenEvents.Count} were raised: [{eventTypes}]");
        }
    }

    /// <summary>
    /// Asserts that events were raised in a specific order.
    /// </summary>
    protected void ThenEventsWereRaisedInOrder(params Type[] eventTypes)
    {
        if (whenEvents.Count != eventTypes.Length)
        {
            throw new AssertionException(
                $"Expected {eventTypes.Length} events but {whenEvents.Count} were raised.");
        }

        for (int i = 0; i < eventTypes.Length; i++)
        {
            if (whenEvents[i].GetType() != eventTypes[i])
            {
                throw new AssertionException(
                    $"Expected event at position {i} to be {eventTypes[i].Name} but was {whenEvents[i].GetType().Name}");
            }
        }
    }

    /// <summary>
    /// Asserts that an exception of the specified type was thrown during the When phase.
    /// </summary>
    protected ExceptionAssertion<TException> ThenExceptionWasThrown<TException>()
        where TException : Exception
    {
        if (whenException == null)
        {
            throw new AssertionException(
                $"Expected exception of type {typeof(TException).Name} but no exception was thrown.");
        }

        if (!(whenException is TException))
        {
            throw new AssertionException(
                $"Expected exception of type {typeof(TException).Name} but exception of type {whenException.GetType().Name} was thrown. " +
                $"Exception message: {whenException.Message}");
        }

        return new ExceptionAssertion<TException>((TException)whenException);
    }

    /// <summary>
    /// Asserts that no exception was thrown during the When phase.
    /// </summary>
    protected void ThenNoExceptionWasThrown()
    {
        if (whenException != null)
        {
            throw new AssertionException(
                $"Expected no exception but {whenException.GetType().Name} was thrown: {whenException.Message}");
        }
    }

    #endregion

    #region Aggregate State Assertions

    /// <summary>
    /// Asserts aggregate state after events have been applied.
    /// Use this to verify the final state of the aggregate.
    /// </summary>
    protected void ThenAggregateState(Action<TAggregate> assertion)
    {
        assertion(Aggregate);
    }

    /// <summary>
    /// Gets the current aggregate version (number of events applied).
    /// </summary>
    protected long AggregateVersion => Aggregate.Version;

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gets all events raised during the When phase for custom assertions.
    /// </summary>
    protected IReadOnlyList<IDomainEvent> GetRaisedEvents() => whenEvents.AsReadOnly();

    /// <summary>
    /// Gets all historical events used in Given phase.
    /// </summary>
    protected IReadOnlyList<IDomainEvent> GetGivenEvents() => givenEvents.AsReadOnly();

    #endregion

    #region Private Implementation

    /// <summary>
    /// Creates an empty aggregate instance using reflection.
    /// Override this method if your aggregate requires special construction.
    /// </summary>
    protected virtual TAggregate CreateEmptyAggregate()
    {
        // Look for parameterless constructor (including private ones)
        var constructors = typeof(TAggregate).GetConstructors(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        var parameterlessConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);

        if (parameterlessConstructor != null)
        {
            return (TAggregate)parameterlessConstructor.Invoke(null);
        }

        // If no parameterless constructor, try to create uninitialized instance
        try
        {
            return (TAggregate)Activator.CreateInstance(typeof(TAggregate), true)!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Could not create instance of {typeof(TAggregate).Name}. " +
                "Ensure the aggregate has a parameterless constructor (can be private) or override CreateEmptyAggregate().", ex);
        }
    }

    /// <summary>
    /// Applies an event to the aggregate using reflection to call the Apply method.
    /// This mimics how Marten applies events during rehydration.
    /// </summary>
    private void ApplyEventToAggregate(IDomainEvent @event)
    {
        var eventType = @event.GetType();
        var applyMethod = typeof(TAggregate).GetMethod("Apply", new[] { eventType });

        if (applyMethod == null)
        {
            throw new InvalidOperationException(
                $"Aggregate {typeof(TAggregate).Name} does not have an Apply method for event {eventType.Name}");
        }

        try
        {
            applyMethod.Invoke(Aggregate, new object[] { @event });
        }
        catch (TargetInvocationException ex)
        {
            // Unwrap the real exception
            throw ex.InnerException ?? ex;
        }
    }

    #endregion
}

/// <summary>
/// Custom exception for test assertions.
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
    public AssertionException(string message, Exception innerException) : base(message, innerException) { }
}