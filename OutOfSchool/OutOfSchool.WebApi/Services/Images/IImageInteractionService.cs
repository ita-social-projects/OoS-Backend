using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface IImageInteractionService<in TKey>
    {
        /// <summary>
        /// Uploads an image to the entity with a specific id.
        /// </summary>
        /// <param name="entityId">Entity id.</param>
        /// <param name="image">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="OperationResult"/> of the operation.</returns>
        Task<OperationResult> UploadImageAsync(TKey entityId, IFormFile image);

        /// <summary>
        /// Removes an image from the entity with a specific id.
        /// </summary>
        /// <param name="entityId">Entity id.</param>
        /// <param name="imageId">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="OperationResult"/> of the operation.</returns>
        Task<OperationResult> RemoveImageAsync(TKey entityId, string imageId);

        /// <summary>
        /// Uploads some images to the entity with a specific id.
        /// </summary>
        /// <param name="entityId">Entity id.</param>
        /// <param name="images">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="MultipleKeyValueOperationResult"/> of the operation.</returns>
        Task<MultipleKeyValueOperationResult> UploadManyImagesAsync(TKey entityId, List<IFormFile> images);

        /// <summary>
        /// Removes some images from the entity with a specific id.
        /// </summary>
        /// <param name="entityId">Entity id.</param>
        /// <param name="imageIds">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="MultipleKeyValueOperationResult"/> of the operation.</returns>
        Task<MultipleKeyValueOperationResult> RemoveManyImagesAsync(TKey entityId, List<string> imageIds);
    }
}
