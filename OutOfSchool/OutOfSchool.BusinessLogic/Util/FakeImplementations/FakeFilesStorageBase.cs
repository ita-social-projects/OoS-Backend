using OutOfSchool.ExternalFileStore;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.BusinessLogic.Util.FakeImplementations;

/// <summary>
/// Only for development purposes. Used as a fake storage whenever no need to interplay with storage.
/// </summary>
public class FakeFilesStorageBase<TFile>(IStorageContext<IFakeStorageClient> storageContext) 
    : FilesStorageBase<TFile, IFakeStorageClient>(storageContext)
where TFile : FileModel, new()
{
    protected sealed override async Task<TFile> GetByIdOperationAsync(string fileId, MemoryStream fileStream,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new TFile { ContentStream = fileStream, ContentType = "Fake_type" });
    }

    protected sealed override Task<string> UploadOperationAsync(TFile? file, string fullFileName, string cacheControl = "",
        IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(fullFileName);
    }

    protected override Task DeleteOperationAsync(string fileId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    protected override IAsyncEnumerable<StorageObject> ListObjectsOperationAsync(string prefix = null, object options = null)
    {
        return AsyncEnumerable.Empty<StorageObject>();
    }
}