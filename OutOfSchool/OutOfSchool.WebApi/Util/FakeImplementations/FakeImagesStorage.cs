using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository.Files;

namespace OutOfSchool.WebApi.Util.FakeImplementations;

/// <summary>
/// Only for development purposes. Used as fake storage whenever no need to interplay with gcp storage.
/// </summary>
public class FakeImagesStorage : IImageFilesStorage
{
    public IAsyncEnumerable<Objects> GetBulkListsOfObjectsAsync(string prefix = null, ListObjectsOptions options = null)
    {
        throw new NotSupportedException();
    }

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