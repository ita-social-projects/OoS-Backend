using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources.Describers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    /// <summary>
    /// Provides APIs for operations with images.
    /// </summary>
    public interface IImageService
    {
        ImagesErrorDescriber ErrorDescriber { get; }

        /// <summary>
        /// Gets image by id.
        /// </summary>
        /// <param name="imageId">Image id.</param>
        /// <returns>The instance of <see cref="Result{ImageDto}"/>.</returns>
        Task<Result<ImageDto>> GetByIdAsync(string imageId);

        /// <summary>
        /// Uploads images into a storage.
        /// </summary>
        /// <typeparam name="TEntity">The entity you wanna validate image specs.</typeparam>
        /// <param name="fileCollection">Contains images to upload.</param>
        /// <returns>The instance of <see cref="MultipleKeyValueOperationResult"/>.</returns>
        Task<ImageUploadingResult> UploadManyImagesAsync<TEntity>(List<IFormFile> fileCollection);

        /// <summary>
        /// Uploads the given image into a storage.
        /// </summary>
        /// <typeparam name="TEntity">The entity you wanna validate image specs.</typeparam>
        /// <param name="image">Contains the image.</param>
        /// <returns>The instance of <see cref="Result{T}"/> that describes the result of uploading.</returns>
        Task<Result<string>> UploadImageAsync<TEntity>(IFormFile image);

        Task<ImageRemovingResult> RemoveManyImagesAsync(List<string> imageIds);

        Task<OperationResult> RemoveImageAsync(string imageId);
    }
}
