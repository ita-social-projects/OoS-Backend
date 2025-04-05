using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models;
using SkiaSharp;

namespace OutOfSchool.BusinessLogic.Services.ThumbnailProcessor;
public class ThumbnailProcessingService : IThumbnailProcessingService
{
    private readonly IImageService imageService;
    private readonly ILogger<ThumbnailProcessingService> logger;
    private readonly ThumbnailGenerationOptions options;
    public ThumbnailProcessingService(
        IImageService service,
        ILogger<ThumbnailProcessingService> logger,
        IOptions<ThumbnailGenerationOptions> options)
    {
        this.imageService = service;
        this.logger = logger;
        this.options = options.Value;
    }
    public async Task<bool> HasThumbnail(string imageId)
    {
        var thumbnailId = GetThumbnailId(imageId);
        var thumbnail = await imageService.GetByIdAsync(thumbnailId);
        return thumbnail.Value != null;
    }

    private string GetThumbnailId(string imageId)
       => Path.GetFileNameWithoutExtension(imageId) + $"-thumbnail.{options.Format}";

    private (int width, int height) GetNewResizedParametres(int basicWidth, int basicHeight)
    {
        double min = Math.Min((double)options.MaxWidth / basicWidth, (double)options.MaxHeight / basicHeight);
        return ((int)(basicWidth * min), (int)(basicHeight * min));
    }

    public async Task<bool> ProcessImage(string imageId)
    {
        var thumbnailId = GetThumbnailId(imageId);
        
        try
        {
            var image = imageService.GetByIdAsync(imageId);
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

            var metadata = new Dictionary<string, string>
            { 
                { "processed", "true" } 
            };

            //TODO: rewrite with metadata + IObjectStorage

            var formFile = new FormFile(thumbnailStream, 0, thumbnailStream.Length, "file", "thumbnail.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            //await imageService.UploadImageAsync(formFile);

            logger.LogInformation("Thumbnail saved: {ThumbnailId}", thumbnailId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Thumbnail generation failed for {ImageId}", imageId);
            return false;
        }
    }
}
