using System.IO;
using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Services
{
    public interface IFileValidator
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
        bool FileSizeValid(long size);

        /// <summary>
        /// Determines if the given format is valid.
        /// </summary>
        /// <param name="format">Image format.</param>
        /// <returns>The <see cref="bool"/> value which shows the validation state.</returns>
        bool FileFormatValid(string format);
    }
}