using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Util.FakeImplementations;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.WebApi.Tests.Services.FileStore;

[TestFixture]
public class FakeFilesStorageBaseTests
{
    private Mock<IStorageContext<IFakeStorageClient>> storageContextMock;
    private Mock<IFakeStorageClient> storageClientMock;
    private const string BucketName = "test-bucket";
    private FakeFilesStorageBase<ImageFileModel> storage;

    [SetUp]
    public void Setup()
    {
        storageClientMock = new Mock<IFakeStorageClient>();
        storageContextMock = new Mock<IStorageContext<IFakeStorageClient>>();
        storageContextMock.Setup(x => x.StorageClient).Returns(storageClientMock.Object);
        storageContextMock.Setup(x => x.BucketName).Returns(BucketName);
        storage = new FakeImagesStorage(storageContextMock.Object);
    }

    [Test]
    public async Task GetByIdAsync_WhenFileExists_ReturnsFile()
    {
        // Arrange
        var fileId = "test-file-id";
        var contentType = "Fake_type";
        var fileContent = new MemoryStream([1, 2, 3]);

        storageClientMock.Setup(x => x.GetByIdAsync(fileId))
            .ReturnsAsync(new FileModel()).Verifiable(Times.Once);

        // Act
        var result = await storage.GetByIdAsync(fileId);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(result.ContentType, contentType);
        Assert.NotNull(result.ContentStream);
    }

    [Test]
    public async Task GetByIdAsync_WhenFileDoesNotExist_ReturnsNull()
    {
        // Arrange
        var fileId = "non-existent-file-id";
        storageClientMock.Setup(x => x.GetByIdAsync(fileId))
            .ReturnsAsync((FileModel)null).Verifiable(Times.Once);

        // Act
        var result = await storage.GetByIdAsync(fileId);

        // Assert
        Assert.IsNull(result);
        storageClientMock.VerifyAll();
    }

    [Test]
    public async Task UploadAsync_ValidFile_ReturnsFileId()
    {
        // Arrange
        var main_subfolder = "provider";
        var file = new ImageFileModel()
        {
            ContentType = "image/png",
            ContentStream = new MemoryStream([1, 2, 3])
        };
        var cacheControl = "max-age=3600";
        var metadata = new Dictionary<string, string> { { "key", "value" } };

        // Act
        var result = await storage.UploadAsync(file, main_subfolder, cacheControl, metadata);

        // Assert
        Assert.NotNull(result);
        Assert.IsInstanceOf<string>(result);
        // Verify it's a non-empty string with expected format
        Assert.That(result, Does.Contain(main_subfolder));
        Assert.That(result, Does.Match(@"^[\w\-/]+$")); // Basic format check
    }

    [Test]
    public async Task DeleteAsync_ValidFileId_DeletesSuccessfully()
    {
        // Arrange
        var fileId = "valid-file-id";
        storageClientMock.Setup(x => x.DeleteAsync(fileId)).Returns(Task.CompletedTask);

        // Act
        await storage.DeleteAsync(fileId);

        // Assert
        storageClientMock.Verify(x => x.DeleteAsync(fileId), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_fileIdisNull_ThrowsArgumentNullException()
    {
        // Arrange
        string? fileId = null;

        // Act and Assert
        await storage.Invoking(s => s.DeleteAsync(fileId))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public void GenerateFileId_ReturnsValidGuid()
    {
        // Act
        var fileId = storage.GenerateFileId();

        // Assert
        Assert.True(Guid.TryParse(fileId, out _));
    }
}