using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.FakeImplementations;

/// <summary>
/// Only for development purposes. Used as a fake storage whenever no need to interplay with storage.
/// </summary>
/// <remarks>
/// This class provides a mock implementation of File Cloud Storage operations.
/// It does not interact with actual cloud storage and is intended for local development,
/// testing, and scenarios where cloud storage dependencies are not available.
/// All operations simulate success without performing actual storage operations.
/// </remarks>
public class FakeFilesStorageBase<TFile>(IStorageContext<IFakeStorageClient> storageContext)
    : FilesStorageBase<TFile, IFakeStorageClient>(storageContext)
where TFile : FileModel, new()
{
    protected sealed override async Task<TFile> GetByIdOperationAsync(string fileId, MemoryStream fileStream,
        CancellationToken cancellationToken = default)
    {
        if (await StorageClient.GetByIdAsync(fileId) is null)
        {
            return null;
        }

        return new TFile { ContentStream = fileStream, ContentType = "Fake_type" };
    }

    protected sealed override Task<string> UploadOperationAsync(TFile? file, string fullFileName, string cacheControl = "",
        IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(fullFileName);
    }

    protected override async Task DeleteOperationAsync(string fileId, CancellationToken cancellationToken = default)
    {
        await StorageClient.DeleteAsync(fileId);
    }

    protected override IAsyncEnumerable<StorageObject> ListObjectsOperationAsync(string? prefix = null, object? options = null)
    {
        return AsyncEnumerable.Empty<StorageObject>();
    }
}