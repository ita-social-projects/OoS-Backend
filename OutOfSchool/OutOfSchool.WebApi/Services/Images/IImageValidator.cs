using System.IO;
using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Services.Images;

/// <summary>
/// Provides APIs for validating images by their options.
/// </summary>
/// <typeparam name="TEntity">The entity type for validating images.</typeparam>
public interface IImageValidator<TEntity> : IFileValidator
{
    /// <summary>
    /// Determines if the given resolution is valid.
    /// </summary>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <returns>The <see cref="bool"/> value which shows the validation state.</returns>
    bool ImageResolutionValid(int width, int height);
}