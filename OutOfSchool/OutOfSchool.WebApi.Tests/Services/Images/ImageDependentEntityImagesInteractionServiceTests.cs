using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Config.Images;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.WebApi.Tests.Services.Images;

[TestFixture]
public class ImageDependentEntityImagesInteractionServiceTests
{
    private Mock<IImageService> imageServiceMock;
    private Mock<IImageReferenceService<TestEntity>> imageReferenceServiceMock;
    private Mock<IOptions<ImagesLimits<TestEntity>>> limitsOptionsMock;
    private Mock<ILogger<ImageDependentEntityImagesInteractionService<TestEntity>>> loggerMock;
    private ImageDependentEntityImagesInteractionService<TestEntity> service;
    private ImagesLimits<TestEntity> imagesLimits;

    [SetUp]
    public void SetUp()
    {
        imageServiceMock = new Mock<IImageService>();
        imageReferenceServiceMock = new Mock<IImageReferenceService<TestEntity>>();
        limitsOptionsMock = new Mock<IOptions<ImagesLimits<TestEntity>>>();
        loggerMock = new Mock<ILogger<ImageDependentEntityImagesInteractionService<TestEntity>>>();

        imagesLimits = new ImagesLimits<TestEntity>
        {
            MaxCountOfFiles = 5
        };

        limitsOptionsMock.Setup(x => x.Value).Returns(imagesLimits);

        service = new ImageDependentEntityImagesInteractionService<TestEntity>(
            imageServiceMock.Object,
            imageReferenceServiceMock.Object,
            limitsOptionsMock.Object,
            loggerMock.Object);
    }

    #region RemoveCoverImageAsync Tests

    [Test]
    public async Task RemoveCoverImageAsync_WhenImageHasNoOtherReferences_ShouldDeleteFromExternalStorage()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            CoverImageId = "cover-image-to-delete"
        };

        imageReferenceServiceMock
            .Setup(x => x.CountReferencesAsync(entity.CoverImageId, default))
            .ReturnsAsync(1);

        imageServiceMock
            .Setup(x => x.RemoveImageAsync(entity.CoverImageId))
            .ReturnsAsync(OperationResult.Success);

        // Act
        var result = await service.RemoveCoverImageAsync(entity);

        // Assert
        result.Succeeded.Should().BeTrue();
        entity.CoverImageId.Should().BeNull();
        imageServiceMock.Verify(x => x.RemoveImageAsync("cover-image-to-delete"), Times.Once);
    }

    [Test]
    public async Task RemoveCoverImageAsync_WhenImageHasMultipleReferences_ShouldNotDeleteFromExternalStorage()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            CoverImageId = "shared-cover-image"
        };

        imageReferenceServiceMock
            .Setup(x => x.CountReferencesAsync(entity.CoverImageId, default))
            .ReturnsAsync(3);

        // Act
        var result = await service.RemoveCoverImageAsync(entity);

        // Assert
        result.Succeeded.Should().BeTrue();
        entity.CoverImageId.Should().BeNull();
        imageServiceMock.Verify(x => x.RemoveImageAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task RemoveCoverImageAsync_WhenImageHasExactlyOneReference_ShouldDeleteFromExternalStorage()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            CoverImageId = "single-reference-image"
        };

        imageReferenceServiceMock
            .Setup(x => x.CountReferencesAsync(entity.CoverImageId, default))
            .ReturnsAsync(1);

        imageServiceMock
            .Setup(x => x.RemoveImageAsync(entity.CoverImageId))
            .ReturnsAsync(OperationResult.Success);

        // Act
        var result = await service.RemoveCoverImageAsync(entity);

        // Assert
        result.Succeeded.Should().BeTrue();
        entity.CoverImageId.Should().BeNull();
        imageServiceMock.Verify(x => x.RemoveImageAsync("single-reference-image"), Times.Once);
    }

    [Test]
    public async Task RemoveCoverImageAsync_WhenImageHasZeroReferences_ShouldDeleteFromExternalStorage()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            CoverImageId = "orphaned-image"
        };

        imageReferenceServiceMock
            .Setup(x => x.CountReferencesAsync(entity.CoverImageId, default))
            .ReturnsAsync(0);

        // Act
        var result = await service.RemoveCoverImageAsync(entity);

        // Assert
        result.Succeeded.Should().BeTrue();
        entity.CoverImageId.Should().BeNull();
        imageServiceMock.Verify(x => x.RemoveImageAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void RemoveCoverImageAsync_WhenEntityIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await service.RemoveCoverImageAsync(null);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public void RemoveCoverImageAsync_WhenCoverImageIdIsNull_ShouldThrowArgumentException()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            CoverImageId = null
        };

        // Act
        Func<Task> act = async () => await service.RemoveCoverImageAsync(entity);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public void RemoveCoverImageAsync_WhenCoverImageIdIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            CoverImageId = string.Empty
        };

        // Act
        Func<Task> act = async () => await service.RemoveCoverImageAsync(entity);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region RemoveImageAsync Tests

    [Test]
    public async Task RemoveImageAsync_WhenImageHasNoOtherReferences_ShouldDeleteFromExternalStorage()
    {
        // Arrange
        var imageId = "image-to-delete";
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Images = new List<Image<TestEntity>>
            {
                new Image<TestEntity> { ExternalStorageId = imageId, EntityId = Guid.NewGuid() }
            }
        };

        imageReferenceServiceMock
            .Setup(x => x.CountReferencesAsync(imageId, default))
            .ReturnsAsync(1);

        imageServiceMock
            .Setup(x => x.RemoveImageAsync(imageId))
            .ReturnsAsync(OperationResult.Success);

        // Act
        var result = await service.RemoveImageAsync(entity, imageId);

        // Assert
        result.Succeeded.Should().BeTrue();
        entity.Images.Should().BeEmpty();
        imageServiceMock.Verify(x => x.RemoveImageAsync(imageId), Times.Once);
    }

    [Test]
    public async Task RemoveImageAsync_WhenImageHasMultipleReferences_ShouldNotDeleteFromExternalStorage()
    {
        // Arrange
        var imageId = "shared-image";
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Images = new List<Image<TestEntity>>
            {
                new Image<TestEntity> { ExternalStorageId = imageId, EntityId = Guid.NewGuid() }
            }
        };

        imageReferenceServiceMock
            .Setup(x => x.CountReferencesAsync(imageId, default))
            .ReturnsAsync(5);

        // Act
        var result = await service.RemoveImageAsync(entity, imageId);

        // Assert
        result.Succeeded.Should().BeTrue();
        entity.Images.Should().BeEmpty();
        imageServiceMock.Verify(x => x.RemoveImageAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task RemoveImageAsync_WhenImageNotInEntityCollection_ShouldReturnFailedResult()
    {
        // Arrange
        var imageId = "non-existent-image";
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Images = new List<Image<TestEntity>>
            {
                new Image<TestEntity> { ExternalStorageId = "different-image", EntityId = Guid.NewGuid() }
            }
        };

        // Act
        var result = await service.RemoveImageAsync(entity, imageId);

        // Assert
        result.Succeeded.Should().BeFalse();
        imageReferenceServiceMock.Verify(x => x.CountReferencesAsync(It.IsAny<string>(), default), Times.Never);
        imageServiceMock.Verify(x => x.RemoveImageAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region RemoveManyImagesAsync Tests

    [Test]
    public async Task RemoveManyImagesAsync_WhenSomeImagesHaveMultipleReferences_ShouldOnlyDeleteUnsharedImages()
    {
        // Arrange
        var imageId1 = "image-1-shared";
        var imageId2 = "image-2-unique";
        var imageId3 = "image-3-shared";

        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Images = new List<Image<TestEntity>>
            {
                new Image<TestEntity> { ExternalStorageId = imageId1, EntityId = Guid.NewGuid() },
                new Image<TestEntity> { ExternalStorageId = imageId2, EntityId = Guid.NewGuid() },
                new Image<TestEntity> { ExternalStorageId = imageId3, EntityId = Guid.NewGuid() }
            }
        };

        var imageIds = new List<string> { imageId1, imageId2, imageId3 };

        var referenceCountDict = new Dictionary<string, int>
        {
            { imageId1, 3 },  // Shared - don't delete
            { imageId2, 1 },  // Unique - delete
            { imageId3, 2 }   // Shared - don't delete
        };

        imageReferenceServiceMock
            .Setup(x => x.CountReferencesAsync(imageIds, default))
            .ReturnsAsync(referenceCountDict);

        imageServiceMock
            .Setup(x => x.RemoveManyImagesAsync(It.IsAny<IList<string>>()))
            .ReturnsAsync(new MultipleImageRemovingResult
            {
                RemovedIds = new List<string> { imageId2 },
                MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult()
            });

        // Act
        var result = await service.RemoveManyImagesAsync(entity, imageIds);

        // Assert
        result.MultipleKeyValueOperationResult.Succeeded.Should().BeTrue();
        entity.Images.Should().BeEmpty();
        imageServiceMock.Verify(x => x.RemoveManyImagesAsync(
            It.Is<IList<string>>(list => list.Count == 1 && list.Contains(imageId2))), Times.Once);
    }

    [Test]
    public async Task RemoveManyImagesAsync_WhenAllImagesHaveMultipleReferences_ShouldNotDeleteAny()
    {
        // Arrange
        var imageId1 = "shared-image-1";
        var imageId2 = "shared-image-2";

        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Images = new List<Image<TestEntity>>
            {
                new Image<TestEntity> { ExternalStorageId = imageId1, EntityId = Guid.NewGuid() },
                new Image<TestEntity> { ExternalStorageId = imageId2, EntityId = Guid.NewGuid() }
            }
        };

        var imageIds = new List<string> { imageId1, imageId2 };

        var referenceCountDict = new Dictionary<string, int>
        {
            { imageId1, 5 },
            { imageId2, 3 }
        };

        imageReferenceServiceMock
            .Setup(x => x.CountReferencesAsync(imageIds, default))
            .ReturnsAsync(referenceCountDict);

        // Act
        var result = await service.RemoveManyImagesAsync(entity, imageIds);

        // Assert
        result.MultipleKeyValueOperationResult.Succeeded.Should().BeTrue();
        entity.Images.Should().BeEmpty();
        imageServiceMock.Verify(x => x.RemoveManyImagesAsync(It.IsAny<IList<string>>()), Times.Never);
    }

    [Test]
    public async Task RemoveManyImagesAsync_WhenAllImagesHaveOneReference_ShouldDeleteAll()
    {
        // Arrange
        var imageId1 = "unique-image-1";
        var imageId2 = "unique-image-2";
        var imageId3 = "unique-image-3";

        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Images = new List<Image<TestEntity>>
            {
                new Image<TestEntity> { ExternalStorageId = imageId1, EntityId = Guid.NewGuid() },
                new Image<TestEntity> { ExternalStorageId = imageId2, EntityId = Guid.NewGuid() },
                new Image<TestEntity> { ExternalStorageId = imageId3, EntityId = Guid.NewGuid() }
            }
        };

        var imageIds = new List<string> { imageId1, imageId2, imageId3 };

        var referenceCountDict = new Dictionary<string, int>
        {
            { imageId1, 1 },
            { imageId2, 1 },
            { imageId3, 1 }
        };

        imageReferenceServiceMock
            .Setup(x => x.CountReferencesAsync(imageIds, default))
            .ReturnsAsync(referenceCountDict);

        imageServiceMock
            .Setup(x => x.RemoveManyImagesAsync(It.IsAny<IList<string>>()))
            .ReturnsAsync(new MultipleImageRemovingResult
            {
                RemovedIds = imageIds,
                MultipleKeyValueOperationResult = new MultipleKeyValueOperationResult ()
            });

        // Act
        var result = await service.RemoveManyImagesAsync(entity, imageIds);

        // Assert
        result.MultipleKeyValueOperationResult.Succeeded.Should().BeTrue();
        entity.Images.Should().BeEmpty();
        imageServiceMock.Verify(x => x.RemoveManyImagesAsync(
            It.Is<IList<string>>(list => list.Count == 3)), Times.Once);
    }

    [Test]
    public async Task RemoveManyImagesAsync_WhenEmptyImageIdsList_ShouldReturnFailedResult()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Images = new List<Image<TestEntity>>()
        };

        var imageIds = new List<string>() { "id"};

        // Act
        var result = await service.RemoveManyImagesAsync(entity, imageIds);

        // Assert
        result.MultipleKeyValueOperationResult.Succeeded.Should().BeFalse();
        imageReferenceServiceMock.Verify(x => x.CountReferencesAsync(It.IsAny<IEnumerable<string>>(), default), Times.Never);
    }

    #endregion

    #region Integration Tests with Reference Counting

    [Test]
    public async Task RemoveImageProcess_WhenReferenceCountIsExactlyOne_ShouldDeleteImage()
    {
        // Arrange
        var imageId = "boundary-case-image";
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Images = new List<Image<TestEntity>>
            {
                new Image<TestEntity> { ExternalStorageId = imageId, EntityId = Guid.NewGuid() }
            }
        };

        imageReferenceServiceMock
            .Setup(x => x.CountReferencesAsync(imageId, default))
            .ReturnsAsync(1);

        imageServiceMock
            .Setup(x => x.RemoveImageAsync(imageId))
            .ReturnsAsync(OperationResult.Success);

        // Act
        var result = await service.RemoveImageAsync(entity, imageId);

        // Assert
        result.Succeeded.Should().BeTrue();
        imageServiceMock.Verify(x => x.RemoveImageAsync(imageId), Times.Once);
    }

    [Test]
    public async Task RemoveImageProcess_WhenReferenceCountIsTwo_ShouldNotDeleteImage()
    {
        // Arrange
        var imageId = "boundary-case-image-2";
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Images = new List<Image<TestEntity>>
            {
                new Image<TestEntity> { ExternalStorageId = imageId, EntityId = Guid.NewGuid() }
            }
        };

        imageReferenceServiceMock
            .Setup(x => x.CountReferencesAsync(imageId, default))
            .ReturnsAsync(2);

        // Act
        var result = await service.RemoveImageAsync(entity, imageId);

        // Assert
        result.Succeeded.Should().BeTrue();
        imageServiceMock.Verify(x => x.RemoveImageAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Logging Verification Tests

    [Test]
    public async Task RemoveCoverImageAsync_WhenImageShared_ShouldLogSkipDeletion()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            CoverImageId = "shared-image-log-test"
        };

        imageReferenceServiceMock
            .Setup(x => x.CountReferencesAsync(entity.CoverImageId, default))
            .ReturnsAsync(3);

        // Act
        await service.RemoveCoverImageAsync(entity);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Skip external deletion")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task RemoveImageAsync_WhenImageShared_ShouldLogSkipDeletion()
    {
        // Arrange
        var imageId = "shared-gallery-image";
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Images = new List<Image<TestEntity>>
            {
                new Image<TestEntity> { ExternalStorageId = imageId, EntityId = Guid.NewGuid() }
            }
        };

        imageReferenceServiceMock
            .Setup(x => x.CountReferencesAsync(imageId, default))
            .ReturnsAsync(4);

        // Act
        await service.RemoveImageAsync(entity, imageId);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Skip external deletion")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Test Entity

    public class TestEntity : IKeyedEntity, IImageDependentEntity<TestEntity>
    {
        public Guid Id { get; set; }
        public string CoverImageId { get; set; }
        public List<Image<TestEntity>> Images { get; set; } = new List<Image<TestEntity>>();
    }

    #endregion
}