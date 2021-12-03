using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services.Images
{
    /// <summary>
    /// Provides APIs for operations with images.
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Gets image by id.
        /// </summary>
        /// <param name="imageId">Image id.</param>
        /// <returns>The instance of <see cref="Result{ImageDto}"/>.</returns>
        Task<Result<ImageDto>> GetByIdAsync(string imageId);

        /// <summary>
        /// Uploads images for the chosen workshop and then updates it.
        /// </summary>
        /// <param name="workshopId">It's an Id of Workshop.</param>
        /// <param name="fileCollection">Contains images to upload.</param>
        /// <returns>The instance of <see cref="Dictionary{TKey,TValue}"/> that contains results (type of <see cref="OperationResult"/>) for uploading any image.
        /// This is the pair of keys from fileCollection and appropriate OperationResult.
        /// It returns the pair of (-1, <see cref="OperationResult"/>) if there are no ways to update the current workshop.</returns>
        Task<IDictionary<short, OperationResult>> UploadManyWorkshopImagesWithUpdatingEntityAsync(
            Guid workshopId,
            List<IFormFile> fileCollection);

        /// <summary>
        /// Uploads the given image for the chosen workshop and then updates it.
        /// </summary>
        /// <param name="workshopId">It's an Id of Workshop.</param>
        /// <param name="imageDto">Contains the image.</param>
        /// <returns>The instance of <see cref="OperationResult"/> that describes the result of uploading.</returns>
        Task<OperationResult> UploadWorkshopImageWithUpdatingEntityAsync(Guid workshopId, ImageDto imageDto);
    }
}
