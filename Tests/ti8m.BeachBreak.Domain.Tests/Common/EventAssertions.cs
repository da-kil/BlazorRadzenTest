using ti8m.BeachBreak.Core.Domain.BuildingBlocks;

namespace ti8m.BeachBreak.Domain.Tests.Common;

/// <summary>
/// Fluent assertion methods for domain events.
/// Provides easy-to-read syntax for validating event payloads and metadata.
/// </summary>
/// <typeparam name="TEvent">The event type being asserted</typeparam>
public class EventAssertion<TEvent>
    where TEvent : IDomainEvent
{
    private readonly TEvent eventInstance;

    public EventAssertion(TEvent eventInstance)
    {
        this.eventInstance = eventInstance ?? throw new ArgumentNullException(nameof(eventInstance));
    }

    /// <summary>
    /// Gets the event instance for direct access to properties.
    /// </summary>
    public TEvent Event => eventInstance;

    /// <summary>
    /// Validates event payload using a predicate function.
    /// </summary>
    /// <param name="predicate">Function to validate event properties</param>
    /// <param name="description">Optional description for assertion failure</param>
    public EventAssertion<TEvent> WithPayload(Func<TEvent, bool> predicate, string? description = null)
    {
        if (!predicate(eventInstance))
        {
            var desc = description ?? "Event payload validation failed";
            throw new AssertionException(
                $"{desc}. Event type: {typeof(TEvent).Name}, Event data: {FormatEventData()}");
        }

        return this;
    }

    /// <summary>
    /// Validates multiple event properties with detailed error messages.
    /// </summary>
    /// <param name="validations">Dictionary of property names to validation functions</param>
    public EventAssertion<TEvent> WithPayload(Dictionary<string, Func<TEvent, bool>> validations)
    {
        var failures = new List<string>();

        foreach (var (propertyName, validation) in validations)
        {
            if (!validation(eventInstance))
            {
                failures.Add(propertyName);
            }
        }

        if (failures.Any())
        {
            throw new AssertionException(
                $"Event payload validation failed for properties: {string.Join(", ", failures)}. " +
                $"Event type: {typeof(TEvent).Name}, Event data: {FormatEventData()}");
        }

        return this;
    }

    /// <summary>
    /// Validates that a specific property has an expected value.
    /// </summary>
    /// <typeparam name="TProperty">Type of the property</typeparam>
    /// <param name="propertySelector">Expression to select the property</param>
    /// <param name="expectedValue">Expected property value</param>
    public EventAssertion<TEvent> WithProperty<TProperty>(
        Func<TEvent, TProperty> propertySelector,
        TProperty expectedValue)
    {
        var actualValue = propertySelector(eventInstance);

        if (!EqualityComparer<TProperty>.Default.Equals(actualValue, expectedValue))
        {
            throw new AssertionException(
                $"Event property validation failed. Expected: {expectedValue}, Actual: {actualValue}. " +
                $"Event type: {typeof(TEvent).Name}");
        }

        return this;
    }

    /// <summary>
    /// Validates that the event has specific aggregate ID.
    /// </summary>
    /// <param name="expectedAggregateId">Expected aggregate ID</param>
    public EventAssertion<TEvent> WithAggregateId(Guid expectedAggregateId)
    {
        // Try to get AggregateId property using reflection
        var aggregateIdProperty = typeof(TEvent).GetProperty("AggregateId");
        if (aggregateIdProperty != null)
        {
            var actualAggregateId = (Guid)aggregateIdProperty.GetValue(eventInstance)!;
            if (actualAggregateId != expectedAggregateId)
            {
                throw new AssertionException(
                    $"Event AggregateId validation failed. Expected: {expectedAggregateId}, Actual: {actualAggregateId}. " +
                    $"Event type: {typeof(TEvent).Name}");
            }
        }
        else
        {
            throw new AssertionException(
                $"Event {typeof(TEvent).Name} does not have an AggregateId property");
        }

        return this;
    }

    /// <summary>
    /// Validates event using a custom assertion action.
    /// </summary>
    /// <param name="assertion">Custom assertion logic</param>
    public EventAssertion<TEvent> With(Action<TEvent> assertion)
    {
        try
        {
            assertion(eventInstance);
        }
        catch (Exception ex) when (!(ex is AssertionException))
        {
            throw new AssertionException(
                $"Custom event assertion failed: {ex.Message}. Event type: {typeof(TEvent).Name}", ex);
        }

        return this;
    }

    /// <summary>
    /// Validates that a DateTime property is within an acceptable range of now.
    /// Useful for testing CreatedDate, Timestamp, etc.
    /// </summary>
    /// <param name="dateSelector">Function to extract DateTime from event</param>
    /// <param name="toleranceSeconds">Tolerance in seconds (default: 5)</param>
    public EventAssertion<TEvent> WithTimestampNear(
        Func<TEvent, DateTime> dateSelector,
        int toleranceSeconds = 5)
    {
        var actualDate = dateSelector(eventInstance);
        var now = DateTime.UtcNow;
        var tolerance = TimeSpan.FromSeconds(toleranceSeconds);

        if (Math.Abs((actualDate - now).TotalSeconds) > tolerance.TotalSeconds)
        {
            throw new AssertionException(
                $"Event timestamp validation failed. Expected within {toleranceSeconds}s of {now:yyyy-MM-dd HH:mm:ss}, " +
                $"but was {actualDate:yyyy-MM-dd HH:mm:ss}. Event type: {typeof(TEvent).Name}");
        }

        return this;
    }

    /// <summary>
    /// Validates that a collection property has expected count.
    /// </summary>
    /// <typeparam name="TItem">Type of collection items</typeparam>
    /// <param name="collectionSelector">Function to extract collection from event</param>
    /// <param name="expectedCount">Expected collection count</param>
    public EventAssertion<TEvent> WithCollectionCount<TItem>(
        Func<TEvent, IEnumerable<TItem>> collectionSelector,
        int expectedCount)
    {
        var collection = collectionSelector(eventInstance);
        var actualCount = collection?.Count() ?? 0;

        if (actualCount != expectedCount)
        {
            throw new AssertionException(
                $"Event collection count validation failed. Expected: {expectedCount}, Actual: {actualCount}. " +
                $"Event type: {typeof(TEvent).Name}");
        }

        return this;
    }

    /// <summary>
    /// Validates that a string property is not null or whitespace.
    /// </summary>
    /// <param name="stringSelector">Function to extract string from event</param>
    public EventAssertion<TEvent> WithNonEmptyString(Func<TEvent, string> stringSelector)
    {
        var value = stringSelector(eventInstance);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new AssertionException(
                $"Event string property validation failed. Expected non-empty string but was: '{value}'. " +
                $"Event type: {typeof(TEvent).Name}");
        }

        return this;
    }

    /// <summary>
    /// Formats event data for error messages.
    /// </summary>
    private string FormatEventData()
    {
        try
        {
            var properties = typeof(TEvent).GetProperties()
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .ToDictionary(p => p.Name, p => p.GetValue(eventInstance)?.ToString() ?? "null");

            return $"{{ {string.Join(", ", properties.Select(kv => $"{kv.Key}: {kv.Value}"))} }}";
        }
        catch
        {
            return eventInstance.ToString() ?? "null";
        }
    }
}

/// <summary>
/// Fluent assertion methods for multiple domain events of the same type.
/// </summary>
/// <typeparam name="TEvent">The event type being asserted</typeparam>
public class MultiEventAssertion<TEvent>
    where TEvent : IDomainEvent
{
    private readonly List<TEvent> events;

    public MultiEventAssertion(List<TEvent> events)
    {
        this.events = events ?? throw new ArgumentNullException(nameof(events));
    }

    /// <summary>
    /// Gets all event instances for direct access.
    /// </summary>
    public IReadOnlyList<TEvent> Events => events.AsReadOnly();

    /// <summary>
    /// Validates all events using a predicate function.
    /// </summary>
    /// <param name="predicate">Function to validate each event</param>
    public MultiEventAssertion<TEvent> WithAllSatisfying(Func<TEvent, bool> predicate)
    {
        var failures = new List<int>();

        for (int i = 0; i < events.Count; i++)
        {
            if (!predicate(events[i]))
            {
                failures.Add(i);
            }
        }

        if (failures.Any())
        {
            throw new AssertionException(
                $"Events validation failed at indices: {string.Join(", ", failures)}. " +
                $"Event type: {typeof(TEvent).Name}, Total events: {events.Count}");
        }

        return this;
    }

    /// <summary>
    /// Validates events by their order/position.
    /// </summary>
    /// <param name="validations">Array of validation functions, one per event position</param>
    public MultiEventAssertion<TEvent> WithOrder(params Func<TEvent, bool>[] validations)
    {
        if (validations.Length != events.Count)
        {
            throw new AssertionException(
                $"Order validation count mismatch. Expected {validations.Length} validations for {events.Count} events");
        }

        var failures = new List<int>();

        for (int i = 0; i < validations.Length; i++)
        {
            if (!validations[i](events[i]))
            {
                failures.Add(i);
            }
        }

        if (failures.Any())
        {
            throw new AssertionException(
                $"Ordered events validation failed at positions: {string.Join(", ", failures)}. " +
                $"Event type: {typeof(TEvent).Name}");
        }

        return this;
    }

    /// <summary>
    /// Gets assertion for a specific event by index.
    /// </summary>
    /// <param name="index">Zero-based event index</param>
    public EventAssertion<TEvent> AtIndex(int index)
    {
        if (index < 0 || index >= events.Count)
        {
            throw new AssertionException(
                $"Event index {index} is out of range. Available events: 0 to {events.Count - 1}");
        }

        return new EventAssertion<TEvent>(events[index]);
    }

    /// <summary>
    /// Gets assertion for the first event.
    /// </summary>
    public EventAssertion<TEvent> First()
    {
        if (!events.Any())
        {
            throw new AssertionException("Cannot get first event - no events available");
        }

        return new EventAssertion<TEvent>(events.First());
    }

    /// <summary>
    /// Gets assertion for the last event.
    /// </summary>
    public EventAssertion<TEvent> Last()
    {
        if (!events.Any())
        {
            throw new AssertionException("Cannot get last event - no events available");
        }

        return new EventAssertion<TEvent>(events.Last());
    }
}

/// <summary>
/// Fluent assertion methods for exceptions thrown during command execution.
/// </summary>
/// <typeparam name="TException">The exception type</typeparam>
public class ExceptionAssertion<TException>
    where TException : Exception
{
    private readonly TException exception;

    public ExceptionAssertion(TException exception)
    {
        this.exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    /// <summary>
    /// Gets the exception instance for direct access.
    /// </summary>
    public TException Exception => exception;

    /// <summary>
    /// Validates exception message contains expected text.
    /// </summary>
    /// <param name="expectedText">Expected text in message</param>
    public ExceptionAssertion<TException> WithMessage(string expectedText)
    {
        if (!exception.Message.Contains(expectedText))
        {
            throw new AssertionException(
                $"Exception message validation failed. Expected message to contain: '{expectedText}', " +
                $"but actual message was: '{exception.Message}'");
        }

        return this;
    }

    /// <summary>
    /// Validates exception message matches exactly.
    /// </summary>
    /// <param name="expectedMessage">Expected exact message</param>
    public ExceptionAssertion<TException> WithExactMessage(string expectedMessage)
    {
        if (exception.Message != expectedMessage)
        {
            throw new AssertionException(
                $"Exception message validation failed. Expected: '{expectedMessage}', " +
                $"Actual: '{exception.Message}'");
        }

        return this;
    }

    /// <summary>
    /// Validates exception using custom assertion.
    /// </summary>
    /// <param name="assertion">Custom assertion logic</param>
    public ExceptionAssertion<TException> With(Action<TException> assertion)
    {
        try
        {
            assertion(exception);
        }
        catch (Exception ex) when (!(ex is AssertionException))
        {
            throw new AssertionException(
                $"Custom exception assertion failed: {ex.Message}. Exception type: {typeof(TException).Name}", ex);
        }

        return this;
    }
}