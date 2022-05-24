﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Models.Workshop;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface IChangeableImagesInteractionMediator<in TEntity> : IImageInteractionMediator<TEntity>
        where TEntity : class, IKeyedEntity
    {
        /// <summary>
        /// Removes unnecessary images, comparing and saving given in oldImageIds and adds new images from the ImageFiles.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="oldImageIds">Image ids we shouldn't remove from entities.</param>
        /// <param name="newImages">Images we should add to the entity.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="MultipleImageChangingResult"/> of the operation.</returns>
        public Task<MultipleImageChangingResult> ChangeImagesAsync(TEntity entity, IList<string> oldImageIds, IList<IFormFile> newImages);

        /// <summary>
        /// Updates the image given in oldImageId with a new one.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="oldImageId">Image id to update.</param>
        /// <param name="newImage">Image we should add to the entity.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ImageChangingResult"/> of the operation.</returns>
        public Task<ImageChangingResult> ChangeImageAsync(TEntity entity, string oldImageId, IFormFile newImage);
    }
}
