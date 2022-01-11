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
        /// Determines if the given <see cref="Stream"/> context is valid.
        /// </summary>
        /// <param name="stream">Describe image context.</param>
        /// <returns>The instance of <see cref="OperationResult"/> which shows the validation state..</returns>
        OperationResult Validate(Stream stream);

        /// <summary>
        /// Determines if the given size is valid.
        /// </summary>
        /// <param name="size">Image size.</param>
        /// <returns>The <see cref="bool"/> value which shows the validation state.</returns>
        bool ImageSizeValid(long size);

        /// <summary>
        /// Determines if the given resolution is valid.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <returns>The <see cref="bool"/> value which shows the validation state.</returns>
        bool ImageResolutionValid(int width, int height);

        /// <summary>
        /// Determines if the given format is valid.
        /// </summary>
        /// <param name="format">Image format.</param>
        /// <returns>The <see cref="bool"/> value which shows the validation state.</returns>
        bool ImageFormatValid(string format);
    }
}
