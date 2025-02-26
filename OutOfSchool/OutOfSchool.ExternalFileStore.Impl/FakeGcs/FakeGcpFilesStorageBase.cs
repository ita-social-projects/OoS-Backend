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
    protected sealed override async Task<TFile> GetByIdOperationAsync(string fullFileName, MemoryStream fileStream, 
        CancellationToken cancellationToken = default)
    {
        var fileObject = await StorageClient.GetObjectAsync(
                BucketName,
                fullFileName,
                cancellationToken: cancellationToken);

        return new TFile { ContentStream = fileStream, ContentType = fileObject.ContentType };
    }

    protected sealed override Task UploadOperationAsync(TFile? file, string fullFileName, string cacheControl = "",
        IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    protected sealed override Task DeleteOperationAsync(string fullFileName, CancellationToken cancellationToken = default)
    {
        StorageClient.DeleteObjectAsync(BucketName, fullFileName, cancellationToken: cancellationToken);
        return Task.CompletedTask;
    }

    protected sealed override IAsyncEnumerable<StorageObject> ListObjectsOperationAsync(string? prefix = null, object? options = null)
    {
        return AsyncEnumerable.Empty<StorageObject>();
    }
}