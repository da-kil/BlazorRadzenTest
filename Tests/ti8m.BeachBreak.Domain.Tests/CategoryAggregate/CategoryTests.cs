using NUnit.Framework;
using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.CategoryAggregate;
using ti8m.BeachBreak.Domain.CategoryAggregate.Events;
using ti8m.BeachBreak.Domain.Tests.Common;

namespace ti8m.BeachBreak.Domain.Tests.CategoryAggregate;

/// <summary>
/// Example tests for Category aggregate demonstrating the event sourcing testing framework.
/// Shows Given/When/Then pattern with fluent event assertions.
/// </summary>
[TestFixture]
public class CategoryTests : AggregateTestBase<Category>
{
    private static readonly Guid CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
    private static readonly Translation CategoryName = new("Performance Reviews", "Leistungsbeurteilungen");
    private static readonly Translation CategoryDescription = new("Annual performance review categories", "Jährliche Leistungsbeurteilungskategorien");

    [SetUp]
    public void Setup()
    {
        SetUp(); // Call base setup
    }

    #region Category Creation Tests

    [Test]
    public void CreateCategory_WithValidData_RaisesCategoryAddedEvent()
    {
        // Given - no prior events (new category)

        // When - category is created
        When(() => new Category(CategoryId, CategoryName, CategoryDescription, 1));

        // Then - CategoryAdded event is raised
        ThenEventWasRaised<CategoryAdded>()
            .WithAggregateId(CategoryId)
            .WithProperty(e => e.Name, CategoryName)
            .WithProperty(e => e.Description, CategoryDescription)
            .WithProperty(e => e.SortOrder, 1)
            .WithTimestampNear(e => e.CreatedDate);

        // And - no other events are raised
        ThenEventCountIs(1);

        // And - aggregate state is correct
        ThenAggregateState(category =>
        {
            Assert.That(category.Id, Is.EqualTo(CategoryId));
            Assert.That(category.Name, Is.EqualTo(CategoryName));
            Assert.That(category.Description, Is.EqualTo(CategoryDescription));
            Assert.That(category.SortOrder, Is.EqualTo(1));
            Assert.That(category.IsActive, Is.True);
        });
    }

    #endregion

    #region Name Change Tests

    [Test]
    public void ChangeName_WithValidName_RaisesCategoryNameChangedEvent()
    {
        // Given - category exists
        var originalName = new Translation("Original Name", "Ursprünglicher Name");
        Given(new CategoryAdded(CategoryId, originalName, CategoryDescription, DateTime.UtcNow, DateTime.UtcNow, 1));

        var newName = new Translation("Updated Name", "Aktualisierter Name");

        // When - name is changed
        When(() => Aggregate.ChangeName(newName));

        // Then - CategoryNameChanged event is raised
        ThenEventWasRaised<CategoryNameChanged>()
            .WithProperty(e => e.Name, newName)
            .WithTimestampNear(e => e.LastModifiedDate);

        // And - aggregate state is updated
        ThenAggregateState(category =>
        {
            Assert.That(category.Name, Is.EqualTo(newName));
        });
    }

    [Test]
    public void ChangeName_WithSameName_RaisesNoEvents()
    {
        // Given - category exists with specific name
        var currentName = new Translation("Existing Name", "Bestehender Name");
        Given(new CategoryAdded(CategoryId, currentName, CategoryDescription, DateTime.UtcNow, DateTime.UtcNow, 1));

        // When - name is "changed" to same value
        When(() => Aggregate.ChangeName(currentName));

        // Then - no events are raised (optimization)
        ThenNoEventsWereRaised();
    }

    [Test]
    public void ChangeName_WithNullName_RaisesNoEvents()
    {
        // Given - category exists
        Given(new CategoryAdded(CategoryId, CategoryName, CategoryDescription, DateTime.UtcNow, DateTime.UtcNow, 1));

        // When - changing to null name
        When(() => Aggregate.ChangeName(null!));

        // Then - no events are raised (null check in domain logic)
        ThenNoEventsWereRaised();
    }

    #endregion

    #region Description Change Tests

    [Test]
    public void ChangeDescription_WithValidDescription_RaisesCategoryDescriptionChangedEvent()
    {
        // Given - category exists
        var originalDescription = new Translation("Original Description", "Ursprüngliche Beschreibung");
        Given(new CategoryAdded(CategoryId, CategoryName, originalDescription, DateTime.UtcNow, DateTime.UtcNow, 1));

        var newDescription = new Translation("Updated Description", "Aktualisierte Beschreibung");

        // When - description is changed
        When(() => Aggregate.ChangeDescription(newDescription));

        // Then - CategoryDescriptionChanged event is raised
        ThenEventWasRaised<CategoryDescriptionChanged>()
            .WithProperty(e => e.Description, newDescription)
            .WithTimestampNear(e => e.LastModifiedDate);

        // And - aggregate state is updated
        ThenAggregateState(category =>
        {
            Assert.That(category.Description, Is.EqualTo(newDescription));
        });
    }

    #endregion

    #region Sort Order Change Tests

    [Test]
    public void ChangeSortOrder_WithValidOrder_RaisesCategorySortOrderChangedEvent()
    {
        // Given - category exists with sort order 1
        Given(new CategoryAdded(CategoryId, CategoryName, CategoryDescription, DateTime.UtcNow, DateTime.UtcNow, 1));

        // When - sort order is changed
        When(() => Aggregate.ChangeSortOrder(5));

        // Then - CategorySortOrderChanged event is raised
        ThenEventWasRaised<CategorySortOrderChanged>()
            .WithProperty(e => e.SortOrder, 5)
            .WithTimestampNear(e => e.LastModifiedDate);

        // And - aggregate state is updated
        ThenAggregateState(category =>
        {
            Assert.That(category.SortOrder, Is.EqualTo(5));
        });
    }

    [Test]
    public void ChangeSortOrder_WithSameOrder_RaisesNoEvents()
    {
        // Given - category exists with sort order 1
        Given(new CategoryAdded(CategoryId, CategoryName, CategoryDescription, DateTime.UtcNow, DateTime.UtcNow, 1));

        // When - sort order is "changed" to same value
        When(() => Aggregate.ChangeSortOrder(1));

        // Then - no events are raised (optimization)
        ThenNoEventsWereRaised();
    }

    #endregion

    #region Complex Workflow Tests

    [Test]
    public void CategoryLifecycle_CreateChangePropertiesAndOrder_RaisesCorrectEventSequence()
    {
        // Given - no prior events

        var initialName = new Translation("Initial Category", "Erste Kategorie");
        var initialDesc = new Translation("Initial Description", "Erste Beschreibung");
        var updatedName = new Translation("Updated Category", "Aktualisierte Kategorie");

        // When - complex lifecycle: create, rename, change sort order
        When(() =>
        {
            var category = new Category(CategoryId, initialName, initialDesc, 1);
            category.ChangeName(updatedName);
            category.ChangeSortOrder(3);
        });

        // Then - events are raised in correct order
        ThenEventsWereRaisedInOrder(
            typeof(CategoryAdded),
            typeof(CategoryNameChanged),
            typeof(CategorySortOrderChanged)
        );

        // And - final state reflects all changes
        ThenAggregateState(category =>
        {
            Assert.That(category.Name, Is.EqualTo(updatedName));
            Assert.That(category.SortOrder, Is.EqualTo(3));
        });
    }

    [Test]
    public void EventSourcing_ReplayEvents_RestoresCorrectState()
    {
        // Given - historical events representing category lifecycle
        var originalName = new Translation("Historical Category", "Historische Kategorie");
        var renamedTo = new Translation("Renamed Category", "Umbenannte Kategorie");
        var baseDate = DateTime.UtcNow.AddDays(-10);

        Given(
            new CategoryAdded(CategoryId, originalName, CategoryDescription, baseDate, baseDate, 1),
            new CategoryNameChanged(renamedTo, baseDate.AddHours(1)),
            new CategorySortOrderChanged(5, baseDate.AddHours(2))
        );

        // When - no additional commands (just testing event replay)

        // Then - aggregate state reflects all historical events
        ThenAggregateState(category =>
        {
            Assert.That(category.Id, Is.EqualTo(CategoryId));
            Assert.That(category.Name, Is.EqualTo(renamedTo)); // Final name after changes
            Assert.That(category.SortOrder, Is.EqualTo(5)); // Final sort order
            Assert.That(category.IsActive, Is.True);
        });

        // And - version reflects number of events applied
        Assert.That(AggregateVersion, Is.EqualTo(3));
    }

    #endregion

    #region Performance and Edge Case Tests

    [Test]
    public void Performance_MultipleChanges_OnlyRaisesEventsForActualChanges()
    {
        // Given - category exists
        var originalName = new Translation("Original", "Original");
        Given(new CategoryAdded(CategoryId, originalName, CategoryDescription, DateTime.UtcNow, DateTime.UtcNow, 1));

        var changedName = new Translation("Changed", "Geändert");
        var finalName = new Translation("Final", "Final");

        // When - multiple change attempts with same and different values
        When(() =>
        {
            Aggregate.ChangeName(originalName);  // Same name - no event
            Aggregate.ChangeName(changedName);   // Different - event
            Aggregate.ChangeName(changedName);   // Same again - no event
            Aggregate.ChangeName(finalName);     // Different - event
            Aggregate.ChangeSortOrder(1);        // Same sort order - no event
            Aggregate.ChangeSortOrder(2);        // Different sort order - event
        });

        // Then - only actual changes raise events
        ThenEventCountIs(3); // 2 name changes + 1 sort order change

        // And - events contain the actual changes
        var nameChangedEvents = GetRaisedEvents().OfType<CategoryNameChanged>().ToList();
        var sortOrderChangedEvents = GetRaisedEvents().OfType<CategorySortOrderChanged>().ToList();

        Assert.That(nameChangedEvents.Count, Is.EqualTo(2));
        Assert.That(nameChangedEvents[0].Name, Is.EqualTo(changedName));
        Assert.That(nameChangedEvents[1].Name, Is.EqualTo(finalName));
        Assert.That(sortOrderChangedEvents[0].SortOrder, Is.EqualTo(2));
    }

    #endregion
}