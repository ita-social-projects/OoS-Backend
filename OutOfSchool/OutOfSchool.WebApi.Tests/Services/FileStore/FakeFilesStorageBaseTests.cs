using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Util.FakeImplementations;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Exceptions;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.WebApi.Tests.Services.FileStore;

[TestFixture]
public class FakeFilesStorageBaseTests
{
    private Mock<IStorageContext<FakeStorageClient>> storageContextMock;
    private Mock<FakeStorageClient> storageClientMock;
    private const string BucketName = "test-bucket";
    private FakeFilesStorageBase<ImageFileModel> storage;

    [SetUp]
    public void Setup()
    {
        storageClientMock = new Mock<FakeStorageClient>();
        storageContextMock = new Mock<IStorageContext<FakeStorageClient>>();
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

        // Act
        var result = await storage.GetByIdAsync(fileId);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(result.ContentType, contentType);
        Assert.NotNull(result.ContentStream);
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
        // 32 - number of characters in Guid without '-'
        // 4 - number of separator characters ('/') in full filename
        // 4 - number of characters in directory names
        Assert.AreEqual(32 + 4 + 4 + main_subfolder.Length, result.Replace("-", "").Length);
    }

    [Test]
    public async Task DeleteAsync_ExceptionInGcp_ThrowsFileStorageException()
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