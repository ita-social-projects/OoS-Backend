using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface ISingleImageInteractionMediator<in TEntity>
        where TEntity : class, IKeyedEntity
    {
        /// <summary>
        /// Uploads an image to the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="image">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="OperationResult"/> of the operation.</returns>
        Task<Result<string>> UploadImageAsync(TEntity entity, IFormFile image);

        /// <summary>
        /// Removes an image from the entity.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="imageId">Represents an image file.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="OperationResult"/> of the operation.</returns>
        Task<OperationResult> RemoveImageAsync(TEntity entity, string imageId);
    }
}
