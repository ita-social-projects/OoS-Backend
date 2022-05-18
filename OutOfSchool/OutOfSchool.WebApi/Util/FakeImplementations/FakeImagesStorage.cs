using System;
using System.Threading;
using System.Threading.Tasks;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository.Files;

namespace OutOfSchool.WebApi.Util.FakeImplementations
{
    public class FakeImagesStorage : IImageFilesStorage
    {
        public Task<ImageFileModel> GetByIdAsync(string fileId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ImageFileModel());
        }

        public Task<string> UploadAsync(ImageFileModel file, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public Task DeleteAsync(string fileId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}