using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.DataModel.Response;
using Moq;
using NUnit.Framework;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.ExternalFileStore.S3;

namespace OutOfSchool.WebApi.Tests.Services.FileStore;

[TestFixture]
public class S3FilesStorageBaseTests
{
    private Mock<IStorageContext<IMinioClient>> storageContextMock;
    private Mock<IMinioClient> minioClientMock;
    private const string BucketName = "test-bucket";
    private S3FilesStorageBase<ImageFileModel> storage;

    [SetUp]
    public void Setup()
    {
        minioClientMock = new Mock<IMinioClient>();
        storageContextMock = new Mock<IStorageContext<IMinioClient>>();
        storageContextMock.Setup(x => x.StorageClient).Returns(minioClientMock.Object);
        storageContextMock.Setup(x => x.BucketName).Returns(BucketName);
        storage = new S3ImagesStorage(storageContextMock.Object);
    }

    [Test]
    public async Task GetByIdAsync_WhenFileExists_ReturnsFile()
    {
        // Arrange
        var fileId = "test-file-id";
        var contentType = "image/png";

        minioClientMock
            .Setup(x => x.GetObjectAsync(
                It.IsAny<GetObjectArgs>(),
                CancellationToken.None))
            .ReturnsAsync(ObjectStat.FromResponseHeaders(fileId, new Dictionary<string, string>
            {
                {"Content-Type", contentType}
            }));

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
        var file = new ImageFileModel()
        {
            ContentType = "image/png",
            ContentStream = new MemoryStream([1, 2, 3])
        };
        var cacheControl = "max-age=3600";
        var metadata = new Dictionary<string, string> {{"key", "value"}};

        minioClientMock
            .Setup(x => x.PutObjectAsync(
                It.IsAny<PutObjectArgs>(),
                CancellationToken.None))
            .ReturnsAsync(new PutObjectResponse(HttpStatusCode.OK, string.Empty, new Dictionary<string, string>(), 1,
                "new-file-id"));

        // Act
        var result = await storage.UploadAsync(file, cacheControl, metadata);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual("new-file-id", result);
    }

    [Test]
    public async Task DeleteAsync_ExistingFile_DeletesSuccessfully()
    {
        // Arrange
        var fileId = "test-file-id";

        minioClientMock
            .Setup(x => x.RemoveObjectAsync(
                It.IsAny<RemoveObjectArgs>(),
                CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        await storage.DeleteAsync(fileId);

        // Assert
        minioClientMock.Verify(x => x.RemoveObjectAsync(
                It.IsAny<RemoveObjectArgs>(),
                CancellationToken.None),
            Times.Once);
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