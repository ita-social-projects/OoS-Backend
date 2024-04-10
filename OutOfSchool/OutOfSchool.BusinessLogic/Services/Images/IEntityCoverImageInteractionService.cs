﻿using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Images;

namespace OutOfSchool.BusinessLogic.Services.Images;

public interface IEntityCoverImageInteractionService<in TEntity>
    where TEntity : class, IKeyedEntity
{
    /// <summary>
    /// Adds a cover image to the entity.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <param name="image">Represents an image file.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="OperationResult"/> of the operation.</returns>
    Task<Result<string>> AddCoverImageAsync(TEntity entity, IFormFile image);

    /// <summary>
    /// Removes a cover image from the entity.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="OperationResult"/> of the operation.</returns>
    Task<OperationResult> RemoveCoverImageAsync(TEntity entity);

    /// <summary>
    /// Changes the cover image given in oldImageId with a new one.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <param name="dtoImageId">Image id to change.</param>
    /// <param name="newImage">Image we should add to the entity.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ImageChangingResult"/> of the operation.</returns>
    public Task<ImageChangingResult> ChangeCoverImageAsync(TEntity entity, string dtoImageId, IFormFile newImage);
}