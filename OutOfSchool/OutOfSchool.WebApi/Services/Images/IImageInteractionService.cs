using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface IImageInteractionService<in TKey, in TEntity>
    {
        Task<MultipleKeyValueOperationResult> UploadImagesAsync(TKey entityId, List<IFormFile> images);

        Task<ImageChangingResult> ChangeImagesAsync(TEntity dto, List<IFormFile> images);

        Task<MultipleKeyValueOperationResult> RemoveImagesAsync(TKey entityId, List<string> imageIds);
    }
}
