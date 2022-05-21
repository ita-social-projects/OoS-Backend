using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface IEntitySetOfImagesInteractionMediator<in TEntity>
        where TEntity : class, IKeyedEntity
    {
        /// <summary>
        /// Adds an image to the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="image">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="OperationResult"/> of the operation.</returns>
        Task<Result<string>> AddImageAsync(TEntity entity, IFormFile image);

        /// <summary>
        /// Removes an image from the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="imageId">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="OperationResult"/> of the operation.</returns>
        Task<OperationResult> RemoveImageAsync(TEntity entity, string imageId);

        /// <summary>
        /// Adds some images to the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="images">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="MultipleKeyValueOperationResult"/> of the operation.</returns>
        Task<MultipleImageUploadingResult> AddManyImagesAsync(TEntity entity, IList<IFormFile> images);

        /// <summary>
        /// Removes some images from the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="imageIds">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="MultipleKeyValueOperationResult"/> of the operation.</returns>
        Task<MultipleImageRemovingResult> RemoveManyImagesAsync(TEntity entity, IList<string> imageIds);

        /// <summary>
        /// Changes the image given in oldImageId with a new one.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="oldImageId">Image id to change.</param>
        /// <param name="newImage">Image we should add to the entity.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ImageChangingResult"/> of the operation.</returns>
        public Task<ImageChangingResult> ChangeImageAsync(TEntity entity, string oldImageId, IFormFile newImage);

        /// <summary>
        /// Removes unnecessary images, comparing and saving given in oldImageIds and adds new images from the ImageFiles.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="oldImageIds">Image ids we shouldn't remove from entities.</param>
        /// <param name="newImages">Images we should add to the entity.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="MultipleImageChangingResult"/> of the operation.</returns>
        public Task<MultipleImageChangingResult> ChangeImagesAsync(TEntity entity, IList<string> oldImageIds, IList<IFormFile> newImages);
    }
}
