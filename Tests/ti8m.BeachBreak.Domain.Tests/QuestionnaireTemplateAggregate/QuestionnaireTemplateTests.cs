using NUnit.Framework;
using ti8m.BeachBreak.Core.Domain;
using ti8m.BeachBreak.Core.Domain.BuildingBlocks;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Events;
using ti8m.BeachBreak.Domain.QuestionnaireTemplateAggregate.Services;
using ti8m.BeachBreak.Domain.Tests.Common;
using Moq;

namespace ti8m.BeachBreak.Domain.Tests.QuestionnaireTemplateAggregate;

/// <summary>
/// Advanced example tests for QuestionnaireTemplate aggregate demonstrating complex business logic testing.
/// Shows testing with dependencies, async operations, and complex state transitions.
/// </summary>
[TestFixture]
public class QuestionnaireTemplateTests : AggregateTestBase<QuestionnaireTemplate>
{
    private static readonly Guid TemplateId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001");
    private static readonly Guid CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002");
    private static readonly Guid PublisherId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003");

    private Mock<IQuestionnaireAssignmentService> assignmentServiceMock = default!;

    [SetUp]
    public void Setup()
    {
        SetUp(); // Call base setup
        assignmentServiceMock = new Mock<IQuestionnaireAssignmentService>();
    }

    #region Template Creation Tests

    [Test]
    public void CreateTemplate_WithValidData_RaisesTemplateCreatedEvent()
    {
        // Given - valid template data
        var name = new Translation("Performance Review", "Leistungsbeurteilung");
        var description = new Translation("Annual performance review", "Jährliche Leistungsbeurteilung");

        // When - template is created
        When(() => new QuestionnaireTemplate(
            TemplateId,
            name,
            description,
            CategoryId,
            QuestionnaireProcessType.PerformanceReview,
            isCustomizable: true,
            autoInitialize: false));

        // Then - QuestionnaireTemplateCreated event is raised
        ThenEventWasRaised<QuestionnaireTemplateCreated>()
            .WithAggregateId(TemplateId)
            .WithProperty(e => e.Name, name)
            .WithProperty(e => e.Description, description)
            .WithProperty(e => e.CategoryId, CategoryId)
            .WithProperty(e => e.ProcessType, QuestionnaireProcessType.PerformanceReview)
            .WithProperty(e => e.IsCustomizable, true)
            .WithProperty(e => e.AutoInitialize, false)
            .WithTimestampNear(e => e.CreatedDate);

        // And - aggregate state is correct
        ThenAggregateState(template =>
        {
            Assert.That(template.Id, Is.EqualTo(TemplateId));
            Assert.That(template.Name, Is.EqualTo(name));
            Assert.That(template.Status, Is.EqualTo(TemplateStatus.Draft));
            Assert.That(template.CanBeEdited(), Is.True);
            Assert.That(template.CanBeAssignedToEmployee(), Is.False);
        });
    }

    [Test]
    public void CreateTemplate_WithInvalidName_ThrowsArgumentException()
    {
        // Given - invalid template data (empty name)
        var emptyName = new Translation("", "");

        // When - template is created with empty name
        When(() => new QuestionnaireTemplate(
            TemplateId,
            emptyName,
            new Translation("Description", "Beschreibung"),
            CategoryId));

        // Then - ArgumentException is thrown
        ThenExceptionWasThrown<ArgumentException>()
            .WithMessage("Name is required in at least one language");
    }

    [Test]
    public void CreateTemplate_WithEmptyCategoryId_ThrowsArgumentException()
    {
        // Given - invalid category ID
        var name = new Translation("Valid Name", "Gültiger Name");

        // When - template is created with empty category ID
        When(() => new QuestionnaireTemplate(
            TemplateId,
            name,
            new Translation("Description", "Beschreibung"),
            Guid.Empty));

        // Then - ArgumentException is thrown
        ThenExceptionWasThrown<ArgumentException>()
            .WithMessage("Category is required");
    }

    #endregion

    #region Template Modification Tests

    [Test]
    public void ChangeName_OnDraftTemplate_RaisesNameChangedEvent()
    {
        // Given - draft template exists
        var originalName = new Translation("Original", "Original");
        var newName = new Translation("Updated", "Aktualisiert");

        Given(CreateTemplateCreatedEvent(originalName));

        // When - name is changed
        When(() => Aggregate.ChangeName(newName));

        // Then - name changed event is raised
        ThenEventWasRaised<QuestionnaireTemplateNameChanged>()
            .WithProperty(e => e.Name, newName);
    }

    [Test]
    public void ChangeName_OnPublishedTemplate_ThrowsInvalidOperationException()
    {
        // Given - published template exists
        var originalName = new Translation("Published Template", "Veröffentlichte Vorlage");

        Given(
            CreateTemplateCreatedEvent(originalName),
            new QuestionnaireTemplatePublished(PublisherId, DateTime.UtcNow, DateTime.UtcNow)
        );

        // When - attempting to change name on published template
        When(() => Aggregate.ChangeName(new Translation("New Name", "Neuer Name")));

        // Then - InvalidOperationException is thrown
        ThenExceptionWasThrown<InvalidOperationException>()
            .WithMessage("Template cannot be edited in current status");
    }

    #endregion

    #region Template Publishing Tests

    [Test]
    public void PublishTemplate_OnDraftTemplate_RaisesPublishedEvent()
    {
        // Given - draft template exists
        Given(CreateTemplateCreatedEvent());

        // When - template is published
        When(() => Aggregate.Publish(PublisherId));

        // Then - template published event is raised
        ThenEventWasRaised<QuestionnaireTemplatePublished>()
            .WithProperty(e => e.PublishedByEmployeeId, PublisherId)
            .WithTimestampNear(e => e.PublishedDate)
            .WithTimestampNear(e => e.LastPublishedDate);

        // And - aggregate state is updated
        ThenAggregateState(template =>
        {
            Assert.That(template.Status, Is.EqualTo(TemplateStatus.Published));
            Assert.That(template.CanBeEdited(), Is.False);
            Assert.That(template.CanBeAssignedToEmployee(), Is.True);
        });
    }

    [Test]
    public void PublishTemplate_AlreadyPublished_ThrowsInvalidOperationException()
    {
        // Given - template is already published
        Given(
            CreateTemplateCreatedEvent(),
            new QuestionnaireTemplatePublished(PublisherId, DateTime.UtcNow, DateTime.UtcNow)
        );

        // When - attempting to publish again
        When(() => Aggregate.Publish(PublisherId));

        // Then - InvalidOperationException is thrown
        ThenExceptionWasThrown<InvalidOperationException>()
            .WithMessage("Template is already published");
    }

    #endregion

    #region Async Operations with Dependencies

    [Test]
    public async Task ChangeProcessType_WithoutActiveAssignments_RaisesProcessTypeChangedEvent()
    {
        // Given - draft template exists and no active assignments
        Given(CreateTemplateCreatedEvent(processType: QuestionnaireProcessType.PerformanceReview));

        assignmentServiceMock
            .Setup(x => x.HasActiveAssignmentsAsync(TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // When - process type is changed
        await When(async () =>
            await Aggregate.ChangeProcessTypeAsync(
                QuestionnaireProcessType.Survey,
                assignmentServiceMock.Object));

        // Then - process type changed event is raised
        ThenEventWasRaised<QuestionnaireTemplateProcessTypeChanged>()
            .WithProperty(e => e.ProcessType, QuestionnaireProcessType.Survey);

        // And - service was called to check assignments
        assignmentServiceMock.Verify(
            x => x.HasActiveAssignmentsAsync(TemplateId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task ChangeProcessType_WithActiveAssignments_ThrowsInvalidOperationException()
    {
        // Given - draft template with active assignments
        Given(CreateTemplateCreatedEvent(processType: QuestionnaireProcessType.PerformanceReview));

        assignmentServiceMock
            .Setup(x => x.HasActiveAssignmentsAsync(TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        assignmentServiceMock
            .Setup(x => x.GetActiveAssignmentCountAsync(TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // When - attempting to change process type with active assignments
        await When(async () =>
            await Aggregate.ChangeProcessTypeAsync(
                QuestionnaireProcessType.Survey,
                assignmentServiceMock.Object));

        // Then - InvalidOperationException is thrown with assignment count
        ThenExceptionWasThrown<InvalidOperationException>()
            .WithMessage("Cannot change process type: 3 active assignment(s) exist");

        // And - no events are raised
        ThenNoEventsWereRaised();
    }

    [Test]
    public async Task UnpublishToDraft_WithActiveAssignments_ThrowsInvalidOperationException()
    {
        // Given - published template with active assignments
        Given(
            CreateTemplateCreatedEvent(),
            new QuestionnaireTemplatePublished(PublisherId, DateTime.UtcNow, DateTime.UtcNow)
        );

        assignmentServiceMock
            .Setup(x => x.HasActiveAssignmentsAsync(TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        assignmentServiceMock
            .Setup(x => x.GetActiveAssignmentCountAsync(TemplateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // When - attempting to unpublish with active assignments
        await When(async () =>
            await Aggregate.UnpublishToDraftAsync(assignmentServiceMock.Object));

        // Then - InvalidOperationException is thrown
        ThenExceptionWasThrown<InvalidOperationException>()
            .WithMessage("Cannot unpublish questionnaire template to draft: 5 active assignment(s) exist");
    }

    #endregion

    #region Complex Business Rules

    [Test]
    public void Archive_OnPublishedTemplate_RaisesArchivedEvent()
    {
        // Given - published template
        Given(
            CreateTemplateCreatedEvent(),
            new QuestionnaireTemplatePublished(PublisherId, DateTime.UtcNow, DateTime.UtcNow)
        );

        // When - template is archived
        When(() => Aggregate.Archive());

        // Then - archived event is raised
        ThenEventWasRaised<QuestionnaireTemplateArchived>();

        // And - template state is updated
        ThenAggregateState(template =>
        {
            Assert.That(template.Status, Is.EqualTo(TemplateStatus.Archived));
            Assert.That(template.CanBeEdited(), Is.False);
            Assert.That(template.CanBeAssignedToEmployee(), Is.False);
        });
    }

    [Test]
    public void RestoreFromArchive_OnArchivedTemplate_RaisesRestoredEvent()
    {
        // Given - archived template
        Given(
            CreateTemplateCreatedEvent(),
            new QuestionnaireTemplatePublished(PublisherId, DateTime.UtcNow, DateTime.UtcNow),
            new QuestionnaireTemplateArchived()
        );

        // When - template is restored
        When(() => Aggregate.RestoreFromArchive());

        // Then - restored event is raised
        ThenEventWasRaised<QuestionnaireTemplateRestoredFromArchive>();

        // And - template is back to draft status
        ThenAggregateState(template =>
        {
            Assert.That(template.Status, Is.EqualTo(TemplateStatus.Draft));
            Assert.That(template.CanBeEdited(), Is.True);
        });
    }

    #endregion

    #region Event Sourcing Integration Tests

    [Test]
    public void TemplateLifecycle_CompleteWorkflow_RaisesCorrectEventSequence()
    {
        // Given - no prior events (new template)
        var name = new Translation("Lifecycle Template", "Lebenszyklus Vorlage");

        // When - complete lifecycle: create → publish → archive → restore → delete
        When(() =>
        {
            var template = new QuestionnaireTemplate(TemplateId, name, new Translation("", ""), CategoryId);
            template.Publish(PublisherId);
            template.Archive();
            template.RestoreFromArchive();
        });

        // Then - events are raised in correct sequence
        ThenEventsWereRaisedInOrder(
            typeof(QuestionnaireTemplateCreated),
            typeof(QuestionnaireTemplatePublished),
            typeof(QuestionnaireTemplateArchived),
            typeof(QuestionnaireTemplateRestoredFromArchive)
        );

        // And - final state is correct
        ThenAggregateState(template =>
        {
            Assert.That(template.Status, Is.EqualTo(TemplateStatus.Draft));
            Assert.That(template.PublishedDate, Is.Not.Null); // Publication date preserved
        });
    }

    [Test]
    public void EventReplay_WithComplexHistory_RestoresCorrectState()
    {
        // Given - complex event history
        var originalName = new Translation("Original", "Original");
        var updatedName = new Translation("Updated", "Aktualisiert");
        var baseDate = DateTime.UtcNow.AddMonths(-3);

        Given(
            new QuestionnaireTemplateCreated(
                TemplateId, originalName, new Translation("", ""), CategoryId,
                QuestionnaireProcessType.PerformanceReview, false, false,
                new List<QuestionSectionData>(), baseDate),
            new QuestionnaireTemplateNameChanged(updatedName),
            new QuestionnaireTemplateCustomizabilityChanged(true),
            new QuestionnaireTemplatePublished(PublisherId, baseDate.AddDays(10), baseDate.AddDays(10)),
            new QuestionnaireTemplateArchived()
        );

        // When - no additional commands (testing event replay only)

        // Then - final state reflects complete history
        ThenAggregateState(template =>
        {
            Assert.That(template.Id, Is.EqualTo(TemplateId));
            Assert.That(template.Name, Is.EqualTo(updatedName)); // Last name change
            Assert.That(template.IsCustomizable, Is.True); // Customizability changed
            Assert.That(template.Status, Is.EqualTo(TemplateStatus.Archived)); // Final status
            Assert.That(template.PublishedDate, Is.EqualTo(baseDate.AddDays(10))); // Publication preserved
            Assert.That(template.PublishedByEmployeeId, Is.EqualTo(PublisherId));
        });

        // And - version reflects all events
        Assert.That(AggregateVersion, Is.EqualTo(5));
    }

    #endregion

    #region Utility Methods

    private QuestionnaireTemplateCreated CreateTemplateCreatedEvent(
        Translation? name = null,
        QuestionnaireProcessType processType = QuestionnaireProcessType.PerformanceReview)
    {
        return new QuestionnaireTemplateCreated(
            TemplateId,
            name ?? new Translation("Test Template", "Test Vorlage"),
            new Translation("Test Description", "Test Beschreibung"),
            CategoryId,
            processType,
            false, // IsCustomizable
            false, // AutoInitialize
            new List<QuestionSectionData>(),
            DateTime.UtcNow);
    }

    #endregion
}