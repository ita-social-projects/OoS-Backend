using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Download;
using Google.Cloud.Storage.V1;
using Moq;
using NUnit.Framework;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.FakeGcs;
using OutOfSchool.ExternalFileStore.Models;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace OutOfSchool.WebApi.Tests.Services.FileStore;


[TestFixture]
public class FakeGcpFilesStorageBaseTests
{
    private Mock<IStorageContext<StorageClient>> storageContextMock;
    private Mock<StorageClient> storageClientMock;
    private const string BucketName = "test-bucket";
    private FakeGcpFilesStorageBase<ImageFileModel> storage;

    [SetUp]
    public void Setup()
    {
        storageClientMock = new Mock<StorageClient>();
        storageContextMock = new Mock<IStorageContext<StorageClient>>();
        storageContextMock.Setup(x => x.StorageClient).Returns(storageClientMock.Object);
        storageContextMock.Setup(x => x.BucketName).Returns(BucketName);
        storage = new FakeGcpImagesStorage(storageContextMock.Object);
    }

    [Test]
    public async Task GetByIdAsync_WhenFileExists_ReturnsFile()
    {
        // Arrange
        var fileId = "test-file-id";
        var contentType = "image/png";
        var fileContent = new MemoryStream([1, 2, 3]);

        var fileObject = new Object
        {
            Name = fileId,
            ContentType = contentType
        };

        storageClientMock
            .Setup(x => x.GetObjectAsync(BucketName, It.IsAny<string>(), null, CancellationToken.None))
            .ReturnsAsync(fileObject);

        storageClientMock
            .Setup(x => x.DownloadObjectAsync(fileObject, It.IsAny<MemoryStream>(), null, CancellationToken.None, null))
            .Callback<Object, Stream, DownloadObjectOptions, CancellationToken, IProgress<IDownloadProgress>>((_, stream, _, _, _) =>
            {
                fileContent.CopyTo(stream);
            });

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
    public async Task DeleteAsync_ExistingFile_DeletesSuccessfully()
    {
        // Arrange
        var fileId = "test-file-id";

        storageClientMock
            .Setup(x => x.DeleteObjectAsync(BucketName, It.IsAny<string>(), null, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        await storage.DeleteAsync(fileId);

        // Assert
        storageClientMock.Verify(x => x.DeleteObjectAsync(BucketName, It.IsAny<string>(), null, CancellationToken.None), Times.Once);
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
