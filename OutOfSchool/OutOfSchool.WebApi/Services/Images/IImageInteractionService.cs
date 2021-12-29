using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface IImageInteractionService<in TKey>
    {
        Task<OperationResult> UploadImageAsync(TKey entityId, IFormFile image);

        Task<OperationResult> RemoveImageAsync(TKey entityId, string imageId);

        Task<MultipleKeyValueOperationResult> UploadManyImagesAsync(TKey entityId, List<IFormFile> images);

        Task<MultipleKeyValueOperationResult> RemoveManyImagesAsync(TKey entityId, List<string> imageIds);
    }
}
