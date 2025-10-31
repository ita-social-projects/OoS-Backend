using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.WorkshopDrafts;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.WebApi.Tests.Services.Images;

[TestFixture]
public class ImageReferenceServiceTests
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private TestOutOfSchoolDbContext context;

    [SetUp]
    public void SetUp()
    {
        var builder = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging();

        options = builder.Options;
        context = new TestOutOfSchoolDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        context?.Dispose();
    }

    #region Workshop Tests

    [Test]
    public async Task CountReferencesAsync_Workshop_WhenImageIdIsNull_ShouldReturnZero()
    {
        // Arrange
        var service = new ImageReferenceService<Workshop>(context);

        // Act
        var result = await service.CountReferencesAsync((string)null);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public async Task CountReferencesAsync_Workshop_WhenImageIdIsEmpty_ShouldReturnZero()
    {
        // Arrange
        var service = new ImageReferenceService<Workshop>(context);

        // Act
        var result = await service.CountReferencesAsync(string.Empty);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public async Task CountReferencesAsync_Workshop_WhenImageIdIsWhitespace_ShouldReturnZero()
    {
        // Arrange
        var service = new ImageReferenceService<Workshop>(context);

        // Act
        var result = await service.CountReferencesAsync("   ");

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public async Task CountReferencesAsync_Workshop_WhenNoReferencesExist_ShouldReturnZero()
    {
        // Arrange
        var service = new ImageReferenceService<Workshop>(context);
        var imageId = "non-existent-image-id";

        // Act
        var result = await service.CountReferencesAsync(imageId);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public async Task CountReferencesAsync_Workshop_WhenOneWorkshopCoverImageExists_ShouldReturnOne()
    {
        // Arrange
        var imageId = "test-image-id-1";
        var workshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = "Test Workshop",
            CoverImageId = imageId,
            ProviderId = Guid.NewGuid()
        };
        context.Workshops.Add(workshop);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<Workshop>(context);

        // Act
        var result = await service.CountReferencesAsync(imageId);

        // Assert
        result.Should().Be(1);
    }

    [Test]
    public async Task CountReferencesAsync_Workshop_WhenMultipleWorkshopsShareSameCoverImage_ShouldReturnCorrectCount()
    {
        // Arrange
        var imageId = "shared-cover-image";
        var workshops = new[]
        {
            new Workshop { Id = Guid.NewGuid(), Title = "Workshop 1", CoverImageId = imageId, ProviderId = Guid.NewGuid() },
            new Workshop { Id = Guid.NewGuid(), Title = "Workshop 2", CoverImageId = imageId, ProviderId = Guid.NewGuid() },
            new Workshop { Id = Guid.NewGuid(), Title = "Workshop 3", CoverImageId = imageId, ProviderId = Guid.NewGuid() }
        };
        context.Workshops.AddRange(workshops);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<Workshop>(context);

        // Act
        var result = await service.CountReferencesAsync(imageId);

        // Assert
        result.Should().Be(3);
    }

    [Test]
    public async Task CountReferencesAsync_Workshop_WhenImageInGallery_ShouldReturnCorrectCount()
    {
        // Arrange
        var imageId = "gallery-image-1";
        var workshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = "Test Workshop",
            ProviderId = Guid.NewGuid()
        };
        context.Workshops.Add(workshop);
        await context.SaveChangesAsync();

        var galleryImage = new Image<Workshop>
        {
            EntityId = workshop.Id,
            ExternalStorageId = imageId
        };
        context.WorkshopImages.Add(galleryImage);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<Workshop>(context);

        // Act
        var result = await service.CountReferencesAsync(imageId);

        // Assert
        result.Should().Be(1);
    }

    [Test]
    public async Task CountReferencesAsync_Workshop_WhenImageUsedAsBothCoverAndGallery_ShouldReturnTwo()
    {
        // Arrange
        var imageId = "dual-use-image";
        var workshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = "Test Workshop",
            CoverImageId = imageId,
            ProviderId = Guid.NewGuid()
        };
        context.Workshops.Add(workshop);
        await context.SaveChangesAsync();

        var galleryImage = new Image<Workshop>
        {
            EntityId = workshop.Id,
            ExternalStorageId = imageId
        };
        context.WorkshopImages.Add(galleryImage);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<Workshop>(context);

        // Act
        var result = await service.CountReferencesAsync(imageId);

        // Assert
        result.Should().Be(2);
    }

    [Test]
    public async Task CountReferencesAsync_WorkshopDraft_WhenImageReferencedInDraft_ShouldReturnCorrectCount()
    {
        // Arrange
        var imageId = "draft-cover-image";
        var draft = new WorkshopDraft
        {
            Id = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            CoverImageId = imageId,
            WorkshopDraftContent = new WorkshopDraftContent()
        };
        context.WorkshopDrafts.Add(draft);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<WorkshopDraft>(context);

        // Act
        var result = await service.CountReferencesAsync(imageId);

        // Assert
        result.Should().Be(1);
    }

    [Test]
    public async Task CountReferencesAsync_Workshop_WhenImageUsedInBothWorkshopAndDraft_ShouldReturnCombinedCount()
    {
        // Arrange
        var imageId = "shared-workshop-draft-image";
        var workshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = "Test Workshop",
            CoverImageId = imageId,
            ProviderId = Guid.NewGuid()
        };
        context.Workshops.Add(workshop);

        var draft = new WorkshopDraft
        {
            Id = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            CoverImageId = imageId,
            WorkshopDraftContent= new WorkshopDraftContent()
        };
        context.WorkshopDrafts.Add(draft);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<Workshop>(context);

        // Act
        var result = await service.CountReferencesAsync(imageId);

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region CompetitiveEvent Tests

    [Test]
    public async Task CountReferencesAsync_CompetitiveEvent_WhenImageIdIsNull_ShouldReturnZero()
    {
        // Arrange
        var service = new ImageReferenceService<CompetitiveEvent>(context);

        // Act
        var result = await service.CountReferencesAsync((string)null);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public async Task CountReferencesAsync_CompetitiveEvent_WhenNoReferencesExist_ShouldReturnZero()
    {
        // Arrange
        var service = new ImageReferenceService<CompetitiveEvent>(context);
        var imageId = "non-existent-competitive-event-image";

        // Act
        var result = await service.CountReferencesAsync(imageId);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public async Task CountReferencesAsync_CompetitiveEvent_WhenCompetitiveEventCoverImageExists_ShouldReturnOne()
    {
        // Arrange
        var imageId = "competitive-event-cover-image";
        var competitiveEvent = new CompetitiveEvent
        {
            Id = Guid.NewGuid(),
            Title = "Test Competitive Event",
            CoverImageId = imageId,
            ShortTitle = "short title"
        };
        context.CompetitiveEvents.Add(competitiveEvent);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<CompetitiveEvent>(context);

        // Act
        var result = await service.CountReferencesAsync(imageId);

        // Assert
        result.Should().Be(1);
    }

    [Test]
    public async Task CountReferencesAsync_CompetitiveEventDraft_WhenImageInDraft_ShouldReturnCorrectCount()
    {
        // Arrange
        var imageId = "competitive-event-draft-image";
        var draft = new CompetitiveEventDraft
        {
            Id = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            CoverImageId = imageId,
            CompetitiveEventDraftContent = new CompetitiveEventDraftContent()
        };
        context.CompetitiveEventDrafts.Add(draft);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<CompetitiveEventDraft>(context);

        // Act
        var result = await service.CountReferencesAsync(imageId);

        // Assert
        result.Should().Be(1);
    }

    [Test]
    public async Task CountReferencesAsync_CompetitiveEvent_WhenImageInGallery_ShouldReturnCorrectCount()
    {
        // Arrange
        var imageId = "competitive-event-gallery-image";
        var competitiveEvent = new CompetitiveEvent
        {
            Id = Guid.NewGuid(),
            Title = "Test Event",
            ShortTitle = "Title"
        };
        context.CompetitiveEvents.Add(competitiveEvent);
        await context.SaveChangesAsync();

        var galleryImage = new Image<CompetitiveEvent>
        {
            EntityId = competitiveEvent.Id,
            ExternalStorageId = imageId
        };
        context.CompetitiveEventsImages.Add(galleryImage);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<CompetitiveEvent>(context);

        // Act
        var result = await service.CountReferencesAsync(imageId);

        // Assert
        result.Should().Be(1);
    }

    #endregion

    #region Batch CountReferencesAsync Tests

    [Test]
    public async Task CountReferencesAsync_Batch_WhenIdsCollectionIsNull_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var service = new ImageReferenceService<Workshop>(context);

        // Act
        var result = await service.CountReferencesAsync((IEnumerable<string>)null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task CountReferencesAsync_Batch_WhenIdsCollectionIsEmpty_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var service = new ImageReferenceService<Workshop>(context);

        // Act
        var result = await service.CountReferencesAsync(new List<string>());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task CountReferencesAsync_Batch_WhenIdsContainNullOrWhitespace_ShouldFilterThemOut()
    {
        // Arrange
        var service = new ImageReferenceService<Workshop>(context);
        var imageIds = new List<string> { "valid-id", null, "", "   " };

        // Act
        var result = await service.CountReferencesAsync(imageIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("valid-id");
        result.Should().HaveCount(1);
    }

    [Test]
    public async Task CountReferencesAsync_Batch_WhenDuplicateIds_ShouldReturnDistinctResults()
    {
        // Arrange
        var imageId = "duplicate-image-id";
        var workshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = "Test Workshop",
            CoverImageId = imageId,
            ProviderId = Guid.NewGuid()
        };
        context.Workshops.Add(workshop);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<Workshop>(context);
        var imageIds = new List<string> { imageId, imageId, imageId };

        // Act
        var result = await service.CountReferencesAsync(imageIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[imageId].Should().Be(1);
    }

    [Test]
    public async Task CountReferencesAsync_Batch_WhenMultipleImagesWithDifferentCounts_ShouldReturnCorrectCounts()
    {
        // Arrange
        var imageId1 = "image-1";
        var imageId2 = "image-2";
        var imageId3 = "image-3";
        var imageId4 = "image-4";

        // image-1: used once
        var workshop1 = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = "Workshop 1",
            CoverImageId = imageId1,
            ProviderId = Guid.NewGuid()
        };

        // image-2: used twice
        var workshop2 = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = "Workshop 2",
            CoverImageId = imageId2,
            ProviderId = Guid.NewGuid()
        };
        var workshop3 = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = "Workshop 3",
            CoverImageId = imageId2,
            ProviderId = Guid.NewGuid()
        };

        // image-3: used in gallery
        var workshop4 = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = "Workshop 4",
            ProviderId = Guid.NewGuid()
        };

        context.Workshops.AddRange(workshop1, workshop2, workshop3, workshop4);
        await context.SaveChangesAsync();

        var galleryImage = new Image<Workshop>
        {
            EntityId = workshop4.Id,
            ExternalStorageId = imageId3
        };
        context.WorkshopImages.Add(galleryImage);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<Workshop>(context);
        var imageIds = new List<string> { imageId1, imageId2, imageId3, imageId4 };

        // Act
        var result = await service.CountReferencesAsync(imageIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4);
        result[imageId1].Should().Be(1);
        result[imageId2].Should().Be(2);
        result[imageId3].Should().Be(1);
        result[imageId4].Should().Be(0);
    }

    [Test]
    public async Task CountReferencesAsync_Batch_WhenImageUsedAcrossMultipleTables_ShouldReturnCombinedCount()
    {
        // Arrange
        var imageId = "multi-table-image";

        // Add to Workshop
        var workshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = "Test Workshop",
            CoverImageId = imageId,
            ProviderId = Guid.NewGuid()
        };
        context.Workshops.Add(workshop);

        // Add to WorkshopDraft
        var draft = new WorkshopDraft
        {
            Id = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            CoverImageId = imageId,
            WorkshopDraftContent = new WorkshopDraftContent()
        };
        context.WorkshopDrafts.Add(draft);

        // Add to Workshop gallery
        await context.SaveChangesAsync();
        var galleryImage = new Image<Workshop>
        {
            EntityId = workshop.Id,
            ExternalStorageId = imageId
        };
        context.WorkshopImages.Add(galleryImage);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<Workshop>(context);

        // Act
        var result = await service.CountReferencesAsync(new List<string> { imageId });

        // Assert
        result.Should().NotBeNull();
        result[imageId].Should().Be(3);
    }

    #endregion

    #region Cancellation Token Tests

    [Test]
    public void CountReferencesAsync_WhenCancellationRequested_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var service = new ImageReferenceService<Workshop>(context);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        Func<Task> act = async () => await service.CountReferencesAsync("test-image", cancellationTokenSource.Token);

        // Assert
        act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public void CountReferencesAsync_Batch_WhenCancellationRequested_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var service = new ImageReferenceService<Workshop>(context);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        Func<Task> act = async () => await service.CountReferencesAsync(new[] { "test-image" }, cancellationTokenSource.Token);

        // Assert
        act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Edge Cases

    [Test]
    public async Task CountReferencesAsync_Workshop_WhenImageIdHasSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var imageId = "image-with-special-chars_@#$%^&*()";
        var workshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = "Test Workshop",
            CoverImageId = imageId,
            ProviderId = Guid.NewGuid()
        };
        context.Workshops.Add(workshop);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<Workshop>(context);

        // Act
        var result = await service.CountReferencesAsync(imageId);

        // Assert
        result.Should().Be(1);
    }

    [Test]
    public async Task CountReferencesAsync_Workshop_WhenVeryLongImageId_ShouldHandleCorrectly()
    {
        // Arrange
        var imageId = new string('a', 500);
        var workshop = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = "Test Workshop",
            CoverImageId = imageId,
            ProviderId = Guid.NewGuid()
        };
        context.Workshops.Add(workshop);
        await context.SaveChangesAsync();

        var service = new ImageReferenceService<Workshop>(context);

        // Act
        var result = await service.CountReferencesAsync(imageId);

        // Assert
        result.Should().Be(1);
    }

    [Test]
    public async Task CountReferencesAsync_Batch_WhenLargeNumberOfIds_ShouldHandleCorrectly()
    {
        // Arrange
        var service = new ImageReferenceService<Workshop>(context);
        var imageIds = Enumerable.Range(1, 1000).Select(i => $"image-{i}").ToList();

        // Act
        var result = await service.CountReferencesAsync(imageIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1000);
        result.Values.Should().AllBeEquivalentTo(0);
    }

    #endregion

    #region Constructor Tests

    [Test]
    public void Constructor_WhenDbContextIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new ImageReferenceService<Workshop>(null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("db");
    }

    #endregion
}