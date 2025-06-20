using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Models;
using System.Net.Mime;
using SkiaSharp;

namespace OutOfSchool.BusinessLogic.Services.ThumbnailProcessor;
public class ThumbnailProcessingService : IThumbnailProcessingService
{
    private readonly IImageService imageService;
    private readonly IImageStorage imageStorage;
    private readonly ILogger<ThumbnailProcessingService> logger;
    private readonly ThumbnailGenerationOptions options;
    public ThumbnailProcessingService(
        IImageService service,
        IImageStorage imageStorage,
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
}
