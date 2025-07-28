using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Models;
using System.Net.Mime;
using SkiaSharp;
using Minio.DataModel.Args;


namespace OutOfSchool.BusinessLogic.Services.ThumbnailProcessor;
public class ThumbnailProcessingService : IThumbnailProcessingService
{
    private readonly IImageService imageService;
    private readonly IObjectImageStorage imageStorage;
    private readonly ILogger<ThumbnailProcessingService> logger;
    private readonly ThumbnailGenerationOptions options;
    public ThumbnailProcessingService(
        IImageService service,
        IObjectImageStorage imageStorage,
        ILogger<ThumbnailProcessingService> logger,
        IOptions<ThumbnailGenerationOptions> options)
    {
        this.imageService = service;
        this.imageStorage = imageStorage;
        this.logger = logger;
        this.options = options.Value;
    }
    public async Task<bool> HasThumbnail(string imageId)
        => await imageStorage.ExistsAsync(GetThumbnailId(imageId));

    public async Task<bool> ProcessImage(string imageId)
    {
        var thumbnailId = GetThumbnailId(imageId);

        try
        {
            var image = imageService.GetByIdAsync(Uri.UnescapeDataString(imageId));
            using var streamMinio = image.Result.Value.ContentStream;

            using var skStream = new SKManagedStream(streamMinio);
            using var bitmap = SKBitmap.Decode(skStream);

            if (bitmap == null)
            {
                logger.LogWarning("Failed to decode image {ImageId}", imageId);
                return false;
            }

            var (newWidth, newHeight) = GetNewResizedParametres(bitmap.Width, bitmap.Height);
            using var thumbnail = bitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.Medium);

            if (thumbnail == null)
            {
                logger.LogWarning("Failed to create thumbnail form image: {ImageId}", imageId);
                return false;
            }

            using var newImage = SKImage.FromBitmap(thumbnail);

            var format = options.Format switch
            {
                "jpeg" or "jpg" => SKEncodedImageFormat.Jpeg,
                "png" => SKEncodedImageFormat.Png,
                _ => SKEncodedImageFormat.Jpeg
            };

            using var encoded = newImage.Encode(format, options.Quality);

            using var thumbnailStream = encoded.AsStream();

            var metadataThumbnail = new Dictionary<string, string>
            {
                { Constants.ExternalImages.CustomFileName , thumbnailId }
            };

            var metadataImage = new Dictionary<string, string>
            {
                { Constants.ExternalImages.IsProcessed , "true" }
            };

            var uploadedThumbnailId = await imageStorage.UploadAsync(
                new ImageFileModel
                {
                    ContentStream = thumbnailStream,
                    ContentType = MediaTypeNames.Image.Jpeg
                },
                cacheControl: Constants.PublicImageCacheControl,
                metadata: metadataThumbnail);

            if (imageStorage is IMetadataStorage metadataStorage)
            {
                await metadataStorage.UpdateMetadataAsync(Uri.UnescapeDataString(imageId), metadataImage);
            }

            logger.LogInformation("Thumbnail saved: {ThumbnailId}", thumbnailId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Thumbnail generation failed for {ImageId}", imageId);
            return false;
        }
    }

    private string GetThumbnailId(string imageId)
       => Uri.UnescapeDataString(imageId) + $"-thumbnail";

    private (int width, int height) GetNewResizedParametres(int basicWidth, int basicHeight)
    {
        double min = Math.Min((double)options.MaxWidth / basicWidth, (double)options.MaxHeight / basicHeight);
        return ((int)(basicWidth * min), (int)(basicHeight * min));
    }

    public async Task ProcessAllUnprocessedThumbnailsAsync(CancellationToken cancellationToken = default)
    {
        var allImages = imageStorage.ListObjectsAsync(options: new ListObjectsArgs().WithRecursive(true));

        //additionally check whether the miniature for a evry existing photo with the mark is_processed == true is valid and if its not correct reset it to false
        await ProcessAllImagesOnThumbnailExistenceAsync(allImages);

        logger.LogInformation("Thumbnail generation for unprocessed images started");

        var unprocessedImageNames = new List<string>();

        await foreach (var obj in imageStorage.ListObjectsAsync(options: new ListObjectsArgs().WithRecursive(true)))
        {
            if (obj.Name.Contains("thumbnail", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var metadata = await (imageStorage as IMetadataStorage).GetCurrentMetadataAsync(obj.Name, cancellationToken);

            if (metadata["is-processed"] == "false")
            {
                unprocessedImageNames.Add(obj.Name);
            }
        }

        foreach (var batch in unprocessedImageNames.Chunk(backgroundJobOptions.BatchSize))
        {
            foreach (var imageName in batch)
            {
                try
                {
                    var isSuccess = await this.ProcessImage(imageName);
                    logger.LogInformation($"Image {imageName} processed = {isSuccess}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to process image {imageName}");
                }
            }
        }

        logger.LogInformation("Thumbnail generation process finisheed");
    }

    public async Task ProcessAllImagesOnThumbnailExistenceAsync(IAsyncEnumerable<StorageObject> objects, CancellationToken cancellationToken = default)
    {
        await foreach (var obj in objects.WithCancellation(cancellationToken))
        {
            if (obj.Name.Contains("thumbnail", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var metadata = await (imageStorage as IMetadataStorage).GetCurrentMetadataAsync(obj.Name, cancellationToken);

            if (metadata.TryGetValue("is-processed", out var isProcessed) && isProcessed == "true")
            {
                var thumbnailName = $"{obj.Name}-thumbnail";
                var exists = await imageStorage.ExistsAsync(thumbnailName, cancellationToken);

                if (!exists)
                {
                    logger.LogWarning("Image {ImageName} is marked as processed, but thumbnail is missing", obj.Name);
                    var updatedMetadata = new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
                    updatedMetadata["is-processed"] = "false";

                    if (imageStorage is IMetadataStorage metadataStorage)
                    {
                        await metadataStorage.UpdateMetadataAsync(obj.Name, updatedMetadata, cancellationToken);
                    }
                }
            }
        }
    }
}
