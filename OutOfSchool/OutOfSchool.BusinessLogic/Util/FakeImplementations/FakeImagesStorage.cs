using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.BusinessLogic.Util.FakeImplementations;

/// <summary>
/// Only for development purposes. Used as fake storage whenever no need to interplay with gcp storage.
/// </summary>
public class FakeImagesStorage : IImageStorage
{
    public Task<ImageFileModel> GetByIdAsync(string fileId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ImageFileModel());
    }

    public Task<string> UploadAsync(ImageFileModel file, string cacheControl, IDictionary<string, string> metadata,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GenerateFileId());
    }

    public Task DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public string GenerateFileId()
    {
        return Guid.NewGuid().ToString();
    }
}