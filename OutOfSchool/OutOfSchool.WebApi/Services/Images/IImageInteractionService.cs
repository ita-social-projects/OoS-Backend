using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface IImageInteractionService<in TKey>
    {
        Task<MultipleKeyValueOperationResult> UploadImagesAsync(TKey entityId, List<IFormFile> images);

        Task<OperationResult> RemoveImagesByIdsAsync(TKey entityId, IEnumerable<string> imageIds);
    }
}
