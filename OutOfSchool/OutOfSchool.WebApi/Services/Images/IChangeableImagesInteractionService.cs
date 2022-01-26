using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Models.Workshop;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface IChangeableImagesInteractionService<in TKey> : IImageInteractionService<TKey>
    {
        /// <summary>
        /// Removes unnecessary images, comparing given in ImageIds and adds new images from the ImageFiles.
        /// </summary>
        /// <param name="entityId">Entity id.</param>
        /// <param name="oldImageIds">Image ids we shouldn't remove from entities.</param>
        /// <param name="newImages">Images we should add to the entity.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ImageChangingResult"/> of the operation.</returns>
        public Task<ImageChangingResult> ChangeImagesAsync(TKey entityId, IList<string> oldImageIds, IList<IFormFile> newImages);
    }
}
