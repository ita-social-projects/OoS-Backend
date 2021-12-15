using System.IO;
using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Services.Images
{
    /// <summary>
    /// Provides APIs for validating images by their options.
    /// </summary>
    /// <typeparam name="TEntityConf">This type encapsulates data for which should get validating options.</typeparam>
    public interface IImageValidatorService<TEntityConf>
    {
        /// <summary>
        /// Validate given <see cref="Stream"/> context.
        /// </summary>
        /// <param name="stream">Describe image context.</param>
        /// <returns>The instance of <see cref="OperationResult"/>.</returns>
        OperationResult Validate(Stream stream);

        /// <summary>
        /// Validate given size.
        /// </summary>
        /// <param name="size">Image size.</param>
        /// <returns>True if it's valid, else - false.</returns>
        bool ValidateImageSize(long size);

        /// <summary>
        /// Validate given Resolution.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <returns>True if it's valid, else - false.</returns>
        bool ValidateImageResolution(int width, int height);

        /// <summary>
        /// Validate given Format.
        /// </summary>
        /// <param name="format">Image format.</param>
        /// <returns>True if it's valid, else - false.</returns>
        bool ValidateImageFormat(string format);
    }
}
