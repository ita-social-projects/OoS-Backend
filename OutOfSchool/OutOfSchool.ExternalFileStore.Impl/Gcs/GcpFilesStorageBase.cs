using Google;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using OutOfSchool.ExternalFileStore.Models;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace OutOfSchool.ExternalFileStore.Gcs;

/// <summary>
/// Represents a base file storage for GCP.
/// </summary>
/// <typeparam name="TFile">File model.</typeparam>
public abstract class GcpFilesStorageBase<TFile>(IStorageContext<StorageClient> storageContext)
    : FilesStorageBase<TFile, StorageClient>(storageContext)
    where TFile : FileModel, new()
{
    /// <inheritdoc/>
    protected sealed override IAsyncEnumerable<StorageObject> ListObjectsOperationAsync(string? prefix = null, object? options = null)
    {
        if (options is ListObjectsOptions opts)
        {
            return StorageClient
                .ListObjectsAsync(BucketName, prefix: prefix, options: opts)
                .AsRawResponses()
                .SelectMany<Objects, Object>(o => o.Items.ToAsyncEnumerable())
                .Select(i => new StorageObject
                {
                    Name = i.Name,
                    ContentType = i.ContentType,
                    Size = i.Size ?? 0,
                    CreatedAt = i.TimeCreatedDateTimeOffset,
                    LastModified = i.UpdatedDateTimeOffset
                });
        }

        throw new ArgumentException($"Argument is not of required type {typeof(ListObjectsOptions)}", nameof(options));
    }

    /// <inheritdoc/>
    protected sealed override async Task<TFile> GetByIdOperationAsync(string fileId, MemoryStream fileStream, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileObject = await StorageClient.GetObjectAsync(
                BucketName,
                fileId,
                cancellationToken: cancellationToken);

            await StorageClient.DownloadObjectAsync(
                fileObject,
                fileStream,
                cancellationToken: cancellationToken);

            fileStream.Position = 0;
            return new TFile { ContentStream = fileStream, ContentType = fileObject.ContentType };
        }
        catch (GoogleApiException)
        {
            await fileStream.DisposeAsync();
            return null;
        }
        catch
        {
            await fileStream.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc/>
    protected sealed override async Task<string> UploadOperationAsync(TFile file, string fullFileName, string cacheControl = "", 
        IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        var storageObject = new Object
        {
            Bucket = BucketName,
            Name = fullFileName,
            ContentType = file.ContentType,
        };

        if (!string.IsNullOrEmpty(cacheControl))
        {
            storageObject.CacheControl = cacheControl;
        }

        if (metadata?.Count > 0)
        {
            storageObject.Metadata = metadata;
        }

        file.ContentStream.Position = 0;
        var dataObject = await StorageClient.UploadObjectAsync(
                storageObject,
                file.ContentStream,
                cancellationToken: cancellationToken);

        return dataObject.Name;
    }

    /// <inheritdoc/>
    protected sealed override async Task DeleteOperationAsync(string fileId, CancellationToken cancellationToken = default)
    {
        await StorageClient.DeleteObjectAsync(BucketName, fileId, cancellationToken: cancellationToken);
    }
}