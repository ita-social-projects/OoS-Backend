using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources.Codes;
using OutOfSchool.WebApi.Config.Images;
using OutOfSchool.WebApi.Extensions;

namespace OutOfSchool.WebApi.Services.Images
{
    /// <summary>
    /// Provides APIs for validating images by their options.
    /// </summary>
    /// <typeparam name="TEntity">This type encapsulates data for which should get validating options.</typeparam>
    public class ImageValidator<TEntity> : IImageValidator
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

                using var image = Image.FromStream(stream); // check disposing, using memory
                if (!FileFormatValid(image.RawFormat.ToString()))
                {
                    return OperationResult.Failed(ImagesOperationErrorCode.InvalidFormatError.GetOperationError());
                }

                if (!ImageResolutionValid(image.Width, image.Height))
                {
                    return OperationResult.Failed(ImagesOperationErrorCode.InvalidResolutionError.GetOperationError());
                }

                return OperationResult.Success;
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
}
