using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources.Codes;
using SkiaSharp;

namespace OutOfSchool.WebApi.Services.Images;

/// <summary>
/// Provides APIs for validating images by their options.
/// </summary>
/// <typeparam name="TEntity">This type encapsulates data for which should get validating options.</typeparam>
public class ImageValidator<TEntity> : IImageValidator<TEntity>
{
    private readonly ImageOptions<TEntity> options;

    private readonly ILogger<ImageValidator<TEntity>> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageValidator{TEntity}"/> class.
    /// </summary>
    /// <param name="options">Image options.</param>
    /// <param name="logger">Logger.</param>
    public ImageValidator(IOptions<ImageOptions<TEntity>> options, ILogger<ImageValidator<TEntity>> logger)
    {
        this.options = options.Value;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public OperationResult Validate(Stream stream)
    {
        try
        {
            _ = stream ?? throw new ArgumentNullException(nameof(stream));
            if (!FileSizeValid(stream.Length))
            {
                return OperationResult.Failed(ImagesOperationErrorCode.InvalidSizeError.GetOperationError());
            }

            using var skData = SKData.Create(stream) ?? throw new InvalidOperationException("Unable to create SKData from the stream");
            using var skCodec = SKCodec.Create(skData) ?? throw new InvalidOperationException("Error while creating an instance of SKCodec. Possibly invalid format of the data stream");

            if (!FileFormatValid(skCodec.EncodedFormat.ToString()))
            {
                return OperationResult.Failed(ImagesOperationErrorCode.InvalidFormatError.GetOperationError());
            }

            var skCodecInfo = skCodec.Info;
            return !ImageResolutionValid(skCodecInfo.Width, skCodecInfo.Height)
                ? OperationResult.Failed(ImagesOperationErrorCode.InvalidResolutionError.GetOperationError())
                : OperationResult.Success;
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Unable to validate stream: {Message}", ex.Message);
            return OperationResult.Failed(ImagesOperationErrorCode.InvalidFormatError.GetOperationError());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to validate stream: {Message}", ex.Message);
            return OperationResult.Failed(ImagesOperationErrorCode.UnexpectedValidationError.GetOperationError());
        }
    }

    /// <inheritdoc/>
    public bool FileSizeValid(long size)
    {
        return size <= options.MaxSizeBytes;
    }

    /// <inheritdoc/>
    public bool ImageResolutionValid(int width, int height)
    {
        return width >= options.MinWidthPixels
               && width <= options.MaxWidthPixels
               && height >= options.MinHeightPixels
               && height <= options.MaxHeightPixels
               && (float)width / height <= options.MaxWidthHeightRatio
               && (float)width / height >= options.MinWidthHeightRatio;
    }

    /// <inheritdoc/>
    public bool FileFormatValid(string format)
    {
        return options.SupportedFormats.Contains(format, StringComparer.OrdinalIgnoreCase);
    }
}