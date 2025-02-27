using Google.Cloud.Storage.V1;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.FakeGcs;

/// <summary>
/// Only for development purposes. Used as fake storage whenever no need to interplay with gcp storage.
/// </summary>
public class FakeGcpFilesStorageBase<TFile>(IStorageContext<StorageClient> storageContext)
    : FilesStorageBase<TFile, StorageClient>(storageContext)
    where TFile : FileModel, new()
{
    protected sealed override async Task<TFile> GetByIdOperationAsync(string fileId, MemoryStream fileStream,
        CancellationToken cancellationToken = default)
    {
        var fileObject = await StorageClient.GetObjectAsync(
                BucketName,
                fileId,
                cancellationToken: cancellationToken);

        return new TFile { ContentStream = fileStream, ContentType = fileObject.ContentType };
    }

    protected sealed override Task<string> UploadOperationAsync(TFile? file, string fullFileName, string cacheControl = "",
        IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(fullFileName);
    }

    protected sealed override Task DeleteOperationAsync(string fileId, CancellationToken cancellationToken = default)
    {
        return StorageClient.DeleteObjectAsync(BucketName, fileId, cancellationToken: cancellationToken);
    }

    protected sealed override IAsyncEnumerable<StorageObject> ListObjectsOperationAsync(string? prefix = null, object? options = null)
    {
        return AsyncEnumerable.Empty<StorageObject>();
    }
}