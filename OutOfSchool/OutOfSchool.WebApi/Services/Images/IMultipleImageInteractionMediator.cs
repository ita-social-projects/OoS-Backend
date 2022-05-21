using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface IMultipleImageInteractionMediator<in TEntity>
        where TEntity : class, IKeyedEntity
    {
        /// <summary>
        /// Uploads some images to the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="images">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="MultipleKeyValueOperationResult"/> of the operation.</returns>
        Task<ImageUploadingResult> UploadManyImagesAsync(TEntity entity, IList<IFormFile> images);

        /// <summary>
        /// Removes some images from the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="imageIds">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="MultipleKeyValueOperationResult"/> of the operation.</returns>
        Task<ImageRemovingResult> RemoveManyImagesAsync(TEntity entity, IList<string> imageIds);
    }
}
