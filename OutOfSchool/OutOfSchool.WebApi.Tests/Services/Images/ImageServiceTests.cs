using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Images;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Exceptions;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.WebApi.Tests.Services.Images;

[TestFixture]
internal class ImageServiceTests
{
    #region TestData

    private static readonly IReadOnlyList<Func<ImageFileModel>> ExternalImageModelsWithMockedStreamTestDataSource;
    private static readonly IReadOnlyList<Func<string>> ImageIdsTestDataSource;

    #endregion

    private Mock<IObjectImageStorage> externalStorageMock;
    private Mock<IServiceProvider> serviceProviderMock;
    private Mock<ILogger<ImageService>> loggerMock;
    private IImageService imageService;

    static ImageServiceTests()
    {
        ExternalImageModelsWithMockedStreamTestDataSource = InitializeExternalImageModelsWithMockedStreamsTestDataSource();
        ImageIdsTestDataSource = InitializeImageIdsTestDataSource();
    }

    [SetUp]
    public void SetUp()
    {
        externalStorageMock = new Mock<IObjectImageStorage>();
        serviceProviderMock = new Mock<IServiceProvider>();
        loggerMock = new Mock<ILogger<ImageService>>();
        imageService = new ImageService(
            externalStorageMock.Object,
            serviceProviderMock.Object,
            loggerMock.Object);
    }

    #region Getting

    [Test]
    public async Task GetById_WhenImageWithThisIdExists_ShouldReturnSuccessfulResultOfImageDto()
    {
        // Arrange
        var imageId = TakeFirstFromTestData(ImageIdsTestDataSource);
        var externalImageModel = TakeFirstFromTestData(ExternalImageModelsWithMockedStreamTestDataSource);
        externalStorageMock.Setup(x => x.GetByIdAsync(imageId, CancellationToken.None)).
            ReturnsAsync(externalImageModel);

        // Act
        var result = await imageService.GetByIdAsync(imageId);

        // Assert
        GenericResultShouldBeSuccess(result);
        Assert.That(result.Value.ContentStream, Is.EqualTo(externalImageModel.ContentStream));
        result.Value.ContentType.Should().BeEquivalentTo(externalImageModel.ContentType);
    }

    [Test]
    public async Task GetById_WhenImageWithThisIdNotExists_ShouldReturnFailedResultOfImageDto()
    {
        // Arrange
        var imageId = TakeFirstFromTestData(ImageIdsTestDataSource);
        externalStorageMock.Setup(x => x.GetByIdAsync(It.IsAny<string>(), CancellationToken.None)).ThrowsAsync(new FileStorageException());

        // Act
        var result = await imageService.GetByIdAsync(imageId);

        // Assert
        GenericResultShouldBeFailed(result);
    }

    [Test]
    public async Task GetById_WhenImageIdIsNull_ShouldReturnFailedResultOfImageDto()
    {
        // Arrange

        // Act
        var result = await imageService.GetByIdAsync(null);

        // Assert
        GenericResultShouldBeFailed(result);
    }

    #endregion

    #region Uploading

    [TestCaseSource(nameof(ImageIdsTestData))]
    public async Task
        UploadManyImages_WhenImageListContainsSomeValidImages_ShouldReturnSuccessfulImageUploadingResultWithAllSavedImages(IList<string> imageIds)
    {
        // Arrange
        var images = CreateMockedFormFiles(imageIds.Count);
        SetUpValidatorWithOperationResult(true);
        var queue = new Queue<string>(imageIds);
        externalStorageMock
            .Setup(x => x.UploadAsync(It.IsAny<ImageFileModel>(), It.IsAny<string>(), It.IsAny<IDictionary<string,string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queue.Dequeue);

        // Act
        var result = await imageService.UploadManyImagesAsync<It.IsAnyType>(images);

        // Assert
        result.MultipleKeyValueOperationResult.Succeeded.Should().BeTrue();
        result.SavedIds.Should().BeEquivalentTo(imageIds);
    }

    [Test]
    public void
        UploadManyImages_WhenImageListIsNull_ShouldReturnFailedImageUploadingResultWithNullSavedIds()
    {
        // Arrange & Act
        Func<Task<MultipleImageUploadingResult>> act = () => imageService.UploadManyImagesAsync<It.IsAnyType>(null);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public void
        UploadManyImages_WhenImageListIsEmpty_ShouldReturnFailedImageUploadingResultWithNullSavedIds()
    {
        // Arrange
        var list = new List<IFormFile>();

        // Act
        Func<Task<MultipleImageUploadingResult>> act = () => imageService.UploadManyImagesAsync<It.IsAnyType>(list);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public void
        UploadManyImages_WhenTEntityCantOperateWithImages_ShouldThrowNullReferenceException()
    {
        // Arrange
        var images = CreateMockedFormFiles(2);
        serviceProviderMock.Setup(x => x.GetService(typeof(IImageValidator<It.IsAnyType>))).Returns((IImageValidator<It.IsAnyType>)null);

        // Act & Assert
        imageService.Invoking(x => x.UploadManyImagesAsync<It.IsAnyType>(images)).Should()
            .ThrowAsync<NullReferenceException>();
    }

    [Test]
    public async Task
        UploadManyImages_WhenImageListContainsSomeInvalidImages_ShouldReturnImageUploadingResultWithSomeSavedImagesAndErrorInfoAboutOthers()
    {
        // Arrange
        const byte countOfImages = 4, countOfValidImages = 2;
        var images = CreateMockedFormFiles(countOfImages);
        var imageIds = TakeFromTestData(ImageIdsTestDataSource, countOfValidImages);
        var validator = new Mock<IImageValidator<It.IsAnyType>>();
        validator.SetupSequence(x => x.Validate(It.IsAny<Stream>()))
            .Returns(OperationResult.Success)
            .Returns(OperationResult.Failed())
            .Returns(OperationResult.Failed())
            .Returns(OperationResult.Success);
        serviceProviderMock.Setup(x => x.GetService(typeof(IImageValidator<It.IsAnyType>))).Returns(validator.Object);
        var queue = new Queue<string>(imageIds);
        externalStorageMock
            .Setup(x => x.UploadAsync(It.IsAny<ImageFileModel>(), It.IsAny<string>(), It.IsAny<IDictionary<string,string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queue.Dequeue);

        // Act
        var result = await imageService.UploadManyImagesAsync<It.IsAnyType>(images);

        // Assert
        result.MultipleKeyValueOperationResult.Succeeded.Should().BeFalse();
        result.SavedIds.Should().BeEquivalentTo(imageIds);
        result.MultipleKeyValueOperationResult.Results.Values.Count(x => !x.Succeeded).Should().Be(countOfImages - countOfValidImages);
    }

    [Test]
    public async Task
        UploadManyImages_WhenSomeImagesWereNotUploadedIntoExternalStorage_ShouldReturnImageUploadingResultWithSomeSavedImagesAndErrorInfoAboutOthers()
    {
        // Arrange
        const byte countOfImages = 4, countOfUploadedImages = 2;
        var images = CreateMockedFormFiles(countOfImages);
        var imageIds = TakeFromTestData(ImageIdsTestDataSource, countOfUploadedImages);
        SetUpValidatorWithOperationResult(true);
        externalStorageMock
            .SetupSequence(x => x.UploadAsync(It.IsAny<ImageFileModel>(), It.IsAny<string>(), It.IsAny<IDictionary<string,string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageIds[0])
            .ThrowsAsync(new FileStorageException())
            .ThrowsAsync(new FileStorageException())
            .ReturnsAsync(imageIds[1]);

        // Act
        var result = await imageService.UploadManyImagesAsync<It.IsAnyType>(images);

        // Assert
        result.MultipleKeyValueOperationResult.Succeeded.Should().BeFalse();
        result.SavedIds.Should().BeEquivalentTo(imageIds);
        result.MultipleKeyValueOperationResult.Results.Should().NotBeNull()
            .And.Subject.Values.Count(x => !x.Succeeded).Should().Be(countOfImages - countOfUploadedImages);
    }

    [Test]
    public async Task UploadImage_WhenImageIsValid_ShouldReturnSuccessfulResultWithSavedImageId()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.ContentType).Returns("image/jpeg");
        var file = fileMock.Object;
        
        var imageId = TakeFirstFromTestData(ImageIdsTestDataSource);
        SetUpValidatorWithOperationResult(true);
        externalStorageMock
            .Setup(x => x.UploadAsync(It.IsAny<ImageFileModel>(), It.IsAny<string>(), It.IsAny<IDictionary<string,string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageId);

        // Act
        var result = await imageService.UploadImageAsync<It.IsAnyType>(file);

        // Assert
        GenericResultShouldBeSuccess(result);
        result.Value.Should().BeEquivalentTo(imageId);
    }

    [Test]
    public void
        UploadImage_WhenImageIsNull_ShouldReturnFailedResult()
    {
        // Arrange & Act
        Func<Task<Result<string>>> act = () => imageService.UploadImageAsync<It.IsAnyType>(null);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task
        UploadImage_WhenImageIsInvalid_ShouldReturnFailedResult()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.ContentType).Returns("image/jpeg");
        var file = fileMock.Object;
        SetUpValidatorWithOperationResult(false);

        // Act
        var result = await imageService.UploadImageAsync<It.IsAnyType>(file);

        // Assert
        GenericResultShouldBeFailed(result);
    }

    [Test]
    public async Task
        UploadImage_WhenWasNotUploadedIntoExternalStorage_ShouldReturnFailedResult()
    {
        // Arrange
        var file = new Mock<IFormFile>().Object;
        var imageId = TakeFirstFromTestData(ImageIdsTestDataSource);
        SetUpValidatorWithOperationResult(true);
        externalStorageMock
            .Setup(x => x.UploadAsync(It.IsAny<ImageFileModel>(), It.IsAny<string>(), It.IsAny<IDictionary<string,string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileStorageException());

        // Act
        var result = await imageService.UploadImageAsync<It.IsAnyType>(file);

        // Assert
        GenericResultShouldBeFailed(result);
    }

    [Test]
    public void
        UploadImage_WhenTEntityCantOperateWithImages_ShouldThrowNullReferenceException()
    {
        // Arrange
        var image = new Mock<IFormFile>().Object;
        serviceProviderMock.Setup(x => x.GetService(typeof(IImageValidator<It.IsAnyType>))).Returns((IImageValidator<It.IsAnyType>)null);

        // Act & Assert
        imageService.Invoking(x => x.UploadImageAsync<It.IsAnyType>(image)).Should()
            .ThrowAsync<NullReferenceException>();
    }

    #endregion

    #region Removing

    [TestCaseSource(nameof(ImageIdsTestData))]
    public async Task
        RemoveManyImages_WhenImageIdsListContainsRightValues_ShouldReturnSuccessfulImageRemovingResultWithAllRemovedImages(IList<string> imageIds)
    {
        // Arrange
        externalStorageMock
            .Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await imageService.RemoveManyImagesAsync(imageIds);

        // Assert
        result.MultipleKeyValueOperationResult.Succeeded.Should().BeTrue();
        result.RemovedIds.Should().BeEquivalentTo(imageIds);
    }

    [Test]
    public void
        RemoveManyImages_WhenImageIdsListIsNull_ShouldReturnFailedImageRemovingResultWithNullRemovedIds()
    {
        // Arrange & Act
        Func<Task<MultipleImageRemovingResult>> act = () => imageService.RemoveManyImagesAsync(null);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public void
        RemoveManyImages_WhenImageIdsListIsEmpty_ShouldReturnFailedImageRemovingResultWithNullRemovedIds()
    {
        // Arrange
        var list = new List<string>();

        // Act
        Func<Task<MultipleImageRemovingResult>> act = () => imageService.RemoveManyImagesAsync(list);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task
        RemoveManyImages_WhenSomeImageIdsWereNotRemoved_BecauseTheyAreIncorrectOrNotExist_ShouldReturnFailedImageRemovingResultWithNullRemovedIds()
    {
        // Arrange
        const byte countOfImageIds = 3, countOfDeleted = 1;
        var imageIds = TakeFromTestData(ImageIdsTestDataSource, countOfImageIds);
        externalStorageMock
            .SetupSequence(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Throws<FileStorageException>()
            .Throws<FileStorageException>();

        // Act
        var result = await imageService.RemoveManyImagesAsync(imageIds);

        // Assert
        result.MultipleKeyValueOperationResult.Succeeded.Should().BeFalse();
        result.MultipleKeyValueOperationResult.Results.Count(x => !x.Value.Succeeded).Should()
            .Be(countOfImageIds - countOfDeleted);
        result.RemovedIds.Should().BeEquivalentTo(imageIds.First());
    }

    [Test]
    public async Task RemoveImage_WhenImageIdIsRight_ShouldReturnSuccessfulOperationResult()
    {
        // Arrange
        var imageId = TakeFirstFromTestData(ImageIdsTestDataSource);
        externalStorageMock
            .Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await imageService.RemoveImageAsync(imageId);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Test]
    public void RemoveImage_WhenImageIdIsNull_ShouldReturnFailedOperationResult()
    {
        // Arrange & Act
        Func<Task<OperationResult>> act = () => imageService.RemoveImageAsync(null);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public void RemoveImage_WhenImageIdIsEmpty_ShouldReturnFailedOperationResult()
    {
        // Arrange
        var imageId = string.Empty;

        // Act
        Func<Task<OperationResult>> act = () => imageService.RemoveImageAsync(imageId);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task RemoveImage_WhenImageIdWasNotRemoved_BecauseItIsIncorrectOrNotExist_ShouldReturnFailedOperationResult()
    {
        // Arrange
        var imageId = TakeFirstFromTestData(ImageIdsTestDataSource);
        externalStorageMock
            .Setup(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Throws<FileStorageException>();

        // Act
        var result = await imageService.RemoveImageAsync(imageId);

        // Assert
        result.Succeeded.Should().BeFalse();
    }

    #endregion
    private static void GenericResultShouldBeSuccess<T>(Result<T> result)
    {
        result.Should().NotBeNull();
        result.OperationResult.Succeeded.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    private static void GenericResultShouldBeFailed<T>(Result<T> result)
    {
        result.Should().NotBeNull();
        result.OperationResult.Succeeded.Should().BeFalse();
    }

    #region TestDataSets

    private static IEnumerable<IList<string>> ImageIdsTestData()
    {
        yield return TakeFromTestData(ImageIdsTestDataSource, ImageIdsTestDataSource.Count);
    }

    #endregion

    private static IList<IFormFile> CreateMockedFormFiles(int count)
    {
        var fileList = new List<IFormFile>();
        for (var i = 0; i < count; i++)
        {
            var file = new Mock<IFormFile>();
            file.Setup(x => x.ContentType).Returns("image/jpeg");
            file.Setup(x => x.OpenReadStream()).Returns(new Mock<Stream>().Object);
            fileList.Add(file.Object);
        }

        return fileList;
    }

    private static IList<T> TakeFromTestData<T>(IReadOnlyList<Func<T>> testDataSource, int count = 1)
    {
        if (testDataSource == null || testDataSource.Count == 0)
        {
            throw new ArgumentException($"{nameof(testDataSource)} is null or empty.");
        }

        if (count < 1 || count > testDataSource.Count)
        {
            throw new InvalidOperationException($"Unreal to take {count} elements from {nameof(testDataSource)}. Count must be more than 0 and less than {testDataSource.Count}.");
        }

        var list = new List<T>();
        for (var i = 0; i < count; i++)
        {
            list.Add(testDataSource[i].Invoke());
        }

        return list;
    }

    private static T TakeFirstFromTestData<T>(IReadOnlyList<Func<T>> testDataSource)
    {
        if (testDataSource == null || testDataSource.Count == 0)
        {
            throw new ArgumentException($"{nameof(testDataSource)} is null or empty.");
        }

        return testDataSource.First().Invoke();
    }

    #region Initializators

    // Shouldn't make these collections smaller
    private static IReadOnlyList<Func<ImageFileModel>> InitializeExternalImageModelsWithMockedStreamsTestDataSource()
    {
        return new List<Func<ImageFileModel>>
        {
            () => new ImageFileModel
            {
                ContentStream = new Mock<Stream>().Object,
                ContentType = "image/jpeg",
            },
            () => new ImageFileModel
            {
                ContentStream = new Mock<Stream>().Object,
                ContentType = "image/png",
            },
        }.AsReadOnly();
    }

    private static IReadOnlyList<Func<string>> InitializeImageIdsTestDataSource()
    {
        return new List<Func<string>>
        {
            () => "61d55214893839e7a516996b",
            () => "61d574bffe6f597b3dea757b",
            () => "61d574bffe6f597b3de08080",
        }.AsReadOnly();
    }

    #endregion

    private void SetUpValidatorWithOperationResult(bool succeeded)
    {
        Func<OperationResult> result;
        if (succeeded)
        {
            result = () => OperationResult.Success;
        }
        else
        {
            result = () => OperationResult.Failed();
        }

        var validator = new Mock<IImageValidator<It.IsAnyType>>();
        validator.Setup(x => x.Validate(It.IsAny<Stream>())).Returns(result);
        serviceProviderMock.Setup(x => x.GetService(typeof(IImageValidator<It.IsAnyType>))).Returns(validator.Object);
    }
}