using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources;
using OutOfSchool.WebApi.Common.Resources.Codes;
using OutOfSchool.WebApi.Config.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    /// <summary>
    /// Provides APIs for validating images by their options.
    /// </summary>
    /// <typeparam name="TEntity">This type encapsulates data for which should get validating options.</typeparam>
    public class ImageValidatorService<TEntity> : IImageValidatorService<TEntity>
    {
        private readonly ImageOptions<TEntity> options;

        private readonly ILogger<ImageValidatorService<TEntity>> logger;

        public ImageValidatorService(IOptions<ImageOptions<TEntity>> options, ILogger<ImageValidatorService<TEntity>> logger)
        {
            this.options = options.Value;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public OperationResult Validate(Stream stream)
        {
            logger.LogInformation("Started validation process for stream.");
            if (!ValidateImageSize(stream.Length))
            {
                return OperationResult.Failed(new OperationError {Code = ImageResourceCodes.InvalidImageSizeError, Description = Resources.ImageResource.InvalidImageSizeError });
            }

            try
            {
                using var image = Image.FromStream(stream); // check disposing, using memory
                if (!ValidateImageFormat(image.RawFormat.ToString()))
                {
                    return OperationResult.Failed(new OperationError {Code = ImageResourceCodes.InvalidImageFormatError, Description = Resources.ImageResource.InvalidImageFormatError});
                }

                if (!ValidateImageResolution(image.Width, image.Height))
                {
                    return OperationResult.Failed(new OperationError {Code = ImageResourceCodes.InvalidImageResolutionError, Description = Resources.ImageResource.InvalidImageResolutionError});
                }

                logger.LogInformation("Validation process was successfully finished.");
                return OperationResult.Success;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, $"Unable to validate stream {ex.Message}");
                return OperationResult.Failed(new OperationError { Code = ImageResourceCodes.InvalidImageFormatError, Description = Resources.ImageResource.InvalidImageFormatError });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unable to validate stream {ex.Message}");
                return OperationResult.Failed(new OperationError {Code = ImageResourceCodes.UnexpectedValidationError, Description = Resources.ImageResource.UnexpectedValidationError});
            }
        }

        /// <inheritdoc/>
        public bool ValidateImageSize(long size)
        {
            return size <= options.MaxSizeBytes;
        }

        /// <inheritdoc/>
        public bool ValidateImageResolution(int width, int height)
        {
            return width >= options.MinWidthPixels
                   && width <= options.MaxWidthPixels
                   && height >= options.MinHeightPixels
                   && height <= options.MaxHeightPixels
                   && width / height < options.MaxWidthHeightRatio
                   && height / width < options.MaxWidthHeightRatio;
        }

        /// <inheritdoc/>
        public bool ValidateImageFormat(string format)
        {
            return options.SupportedFormats.Contains(format, StringComparer.OrdinalIgnoreCase);
        }
    }
}
