using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface IMultipleImageInteractionService<in TKey>
    {
        /// <summary>
        /// Uploads some images to the entity with a specific id without the transaction.
        /// </summary>
        /// <param name="entityId">Entity id.</param>
        /// <param name="images">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="MultipleKeyValueOperationResult"/> of the operation.</returns>
        Task<ImageUploadingResult> UploadManyImagesWithoutTransactionAsync(TKey entityId, IList<IFormFile> images);

        /// <summary>
        /// Uploads some images to the entity with a specific id.
        /// </summary>
        /// <param name="entityId">Entity id.</param>
        /// <param name="images">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="MultipleKeyValueOperationResult"/> of the operation.</returns>
        Task<ImageUploadingResult> UploadManyImagesAsync(TKey entityId, IList<IFormFile> images);

        /// <summary>
        /// Removes some images from the entity with a specific id without the transaction.
        /// </summary>
        /// <param name="entityId">Entity id.</param>
        /// <param name="imageIds">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="MultipleKeyValueOperationResult"/> of the operation.</returns>
        Task<ImageRemovingResult> RemoveManyImagesWithoutTransactionAsync(TKey entityId, IList<string> imageIds);

        /// <summary>
        /// Removes some images from the entity with a specific id.
        /// </summary>
        /// <param name="entityId">Entity id.</param>
        /// <param name="imageIds">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="MultipleKeyValueOperationResult"/> of the operation.</returns>
        Task<ImageRemovingResult> RemoveManyImagesAsync(TKey entityId, IList<string> imageIds);
    }
}
