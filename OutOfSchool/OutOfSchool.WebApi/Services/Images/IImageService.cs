using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    /// <summary>
    /// Provides APIs for operations with images.
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Gets an image by its id.
        /// </summary>
        /// <param name="imageId">Image id.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="Result{T}"/> of the <see cref="ImageDto"/>, got from the operation.</returns>
        Task<Result<ImageDto>> GetByIdAsync(string imageId);

        /// <summary>
        /// Uploads images into a storage.
        /// </summary>
        /// <typeparam name="TEntity">The entity you wanna validate image specs.</typeparam>
        /// <param name="images">Contains images to upload.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ImageUploadingResult"/> of the operation.</returns>
        Task<ImageUploadingResult> UploadManyImagesAsync<TEntity>(IList<IFormFile> images);

        /// <summary>
        /// Uploads the given image into a storage.
        /// </summary>
        /// <typeparam name="TEntity">The entity you wanna validate image specs.</typeparam>
        /// <param name="image">Contains the image.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="Result{T}"/> of the <see cref="string"/>, got from the operation.</returns>
        Task<Result<string>> UploadImageAsync<TEntity>(IFormFile image);

        /// <summary>
        /// Removes images from a storage.
        /// </summary>
        /// <param name="imageIds">Image Ids.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ImageRemovingResult"/> of the operation.</returns>
        Task<ImageRemovingResult> RemoveManyImagesAsync(IList<string> imageIds);

        /// <summary>
        /// Uploads the given image into a storage.
        /// </summary>
        /// <param name="imageId">Image Id.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="OperationResult"/> of the operation.</returns>
        Task<OperationResult> RemoveImageAsync(string imageId);
    }
}
