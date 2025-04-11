using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services.ThumbnailProcessor;
using OutOfSchool.ExternalFileStore.Models;
using OutOfSchool.ExternalFileStore;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models.Images;

namespace OutOfSchool.WebApi.Tests.Services;
[TestFixture]
public class ThumbnailProcessingServiceTests
{
    private Mock<IImageService> imageServiceMock;
    private Mock<IImageStorage> imageStorageMock;
    private Mock<IMetadataStorage> metadataStorageMock;
    private Mock<ILogger<ThumbnailProcessingService>> loggerMock;
    private ThumbnailGenerationOptions options;
    private ThumbnailProcessingService service;

    [SetUp]
    public void Setup()
    {
        imageServiceMock = new Mock<IImageService>();
        imageStorageMock = new Mock<IImageStorage>();
        metadataStorageMock = imageStorageMock.As<IMetadataStorage>();
        loggerMock = new Mock<ILogger<ThumbnailProcessingService>>();

        options = new ThumbnailGenerationOptions
        {
            Format = "jpeg",
            MaxHeight = 100,
            MaxWidth = 200,
            Quality = 75
        };

        service = new ThumbnailProcessingService(
            imageServiceMock.Object,
            imageStorageMock.Object,
            loggerMock.Object,
            Options.Create(options));
    }

    [Test]
    public async Task HasThumbnail_ReturnsTrue_WhenThumbnailExists()
    {
        var imageId = "image-id-test53748857";
        imageServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<ImageDto>.Success(new ImageDto { ContentStream = new MemoryStream(), ContentType = "image/jpeg" }));

        var result = await service.HasThumbnail(imageId);

        Assert.IsTrue(result);
    }

    [Test]
    public async Task HasThumbnail_ReturnsFalse_WhenImageNotFound()
    {
        var imageId = "image-id-test279348234";
        var error = new OperationError {Description = "Image not found", Code = "500" };

        imageServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<ImageDto>.Failed(error));

        var result = await service.HasThumbnail(imageId);

        Assert.IsFalse(result);
    }

    [Test]
    public async Task ProcessImage_ReturnsFalse_WhenImageDecodeFails()
    {
        var imageId = "broken-image-id-test3285440";
        var stream = new MemoryStream();

        var dto = new ImageDto { ContentStream = stream, ContentType = "image/jpeg" };
        imageServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<ImageDto>.Success(dto));

        var result = await service.ProcessImage(imageId);

        Assert.IsFalse(result);
    }

    [Test]
    public async Task ProcessImage_ReturnsTrue_WhenThumbnailCreated()
    {
        var imageId = "valid-image-id-test12439";

        using var bitmap = new SKBitmap(300, 300);
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, 80);
        var imageBytes = encoded.ToArray();
        var stream = new MemoryStream(imageBytes);

        imageServiceMock.Setup(s => s.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<ImageDto>.Success(new ImageDto
            {
                ContentStream = stream,
                ContentType = "image/jpeg"
            }));

        imageStorageMock.Setup(x => x.UploadAsync(
                It.IsAny<ImageFileModel>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>>(),
                default))
            .ReturnsAsync("thumbnail-id");

        metadataStorageMock.Setup(x => x.UpdateMetadataAsync(
            It.IsAny<string>(),
            It.IsAny<IDictionary<string, string>>(),
            default))
            .Returns(Task.CompletedTask);

        var result = await service.ProcessImage(imageId);

        Assert.IsTrue(result);
    }
}