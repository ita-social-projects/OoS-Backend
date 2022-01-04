using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources.Describers;
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

        private readonly ImagesErrorDescriber errorDescriber;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageValidatorService{TEntity}"/> class.
        /// </summary>
        /// <param name="options">Image options.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="errorDescriber">The instance which describes image errors.</param>
        public ImageValidatorService(IOptions<ImageOptions<TEntity>> options, ILogger<ImageValidatorService<TEntity>> logger, ImagesErrorDescriber errorDescriber)
        {
            this.options = options.Value;
            this.logger = logger;
            this.errorDescriber = errorDescriber;
        }

        /// <inheritdoc/>
        public OperationResult Validate(Stream stream)
        {
            if (!ImageSizeValid(stream.Length))
            {
                return OperationResult.Failed(errorDescriber.InvalidSizeError());
            }

            try
            {
                using var image = Image.FromStream(stream); // check disposing, using memory
                if (!ImageFormatValid(image.RawFormat.ToString()))
                {
                    return OperationResult.Failed(errorDescriber.InvalidFormatError());
                }

                if (!ImageResolutionValid(image.Width, image.Height))
                {
                    return OperationResult.Failed(errorDescriber.InvalidResolutionError());
                }

                return OperationResult.Success;
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, $"Unable to validate stream: {ex.Message}");
                return OperationResult.Failed(errorDescriber.InvalidFormatError());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unable to validate stream: {ex.Message}");
                return OperationResult.Failed(errorDescriber.UnexpectedValidationError());
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
