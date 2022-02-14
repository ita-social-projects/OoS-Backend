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
    public class ImageValidatorService<TEntity> : IImageValidatorService<TEntity>
    {
        private readonly ImageOptions<TEntity> options;

        private readonly ILogger<ImageValidatorService<TEntity>> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageValidatorService{TEntity}"/> class.
        /// </summary>
        /// <param name="options">Image options.</param>
        /// <param name="logger">Logger.</param>
        public ImageValidatorService(IOptions<ImageOptions<TEntity>> options, ILogger<ImageValidatorService<TEntity>> logger)
        {
            this.options = options.Value;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public OperationResult Validate(Stream stream)
        {
            if (!ImageSizeValid(stream.Length))
            {
                return OperationResult.Failed(ImagesOperationErrorCode.InvalidSizeError.GetOperationError());
            }

            try
            {
                using var image = Image.FromStream(stream); // check disposing, using memory
                if (!ImageFormatValid(image.RawFormat.ToString()))
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
                logger.LogError(ex, $"Unable to validate stream: {ex.Message}");
                return OperationResult.Failed(ImagesOperationErrorCode.InvalidFormatError.GetOperationError());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unable to validate stream: {ex.Message}");
                return OperationResult.Failed(ImagesOperationErrorCode.UnexpectedValidationError.GetOperationError());
            }
        }

        /// <inheritdoc/>
        public bool ImageSizeValid(long size)
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
                   && width / height < options.MaxWidthHeightRatio
                   && height / width < options.MaxWidthHeightRatio;
        }

        /// <inheritdoc/>
        public bool ImageFormatValid(string format)
        {
            return options.SupportedFormats.Contains(format, StringComparer.OrdinalIgnoreCase);
        }
    }
}
