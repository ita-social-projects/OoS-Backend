using Google;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using OutOfSchool.ExternalFileStore.Exceptions;
using OutOfSchool.ExternalFileStore.Models;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace OutOfSchool.ExternalFileStore.Gcs;

/// <summary>
/// Represents a base file storage.
/// </summary>
/// <typeparam name="TFile">File model.</typeparam>
public abstract class GcpFilesStorageBase<TFile>(IStorageContext<StorageClient> storageContext)
    : IObjectStorage<TFile, string>
    where TFile : FileModel, new()
{
    private protected StorageClient StorageClient { get; } = storageContext.StorageClient;

    private protected string BucketName { get; } = storageContext.BucketName;

    /// <inheritdoc/>
    public IAsyncEnumerable<StorageObject> ListObjectsAsync(string? prefix = null, object? options = null)
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
    public virtual async Task<TFile> GetByIdAsync(string fileId, CancellationToken cancellationToken = default)
    {
        _ = fileId ?? throw new ArgumentNullException(nameof(fileId));
        var fileStream = new MemoryStream();
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
            return new TFile {ContentStream = fileStream, ContentType = fileObject.ContentType};
        }
        catch (GoogleApiException)
        {
            await fileStream.DisposeAsync();
            return null;
        }
        catch (Exception ex)
        {
            await fileStream.DisposeAsync();
            throw new FileStorageException(ex);
        }
    }

    /// <inheritdoc/>
    public virtual async Task<string> UploadAsync(TFile file, string cacheControl,
        IDictionary<string, string>? metadata, CancellationToken cancellationToken = default)
    {
        _ = file ?? throw new ArgumentNullException(nameof(file));
        metadata ??= new Dictionary<string, string>(StringComparer.Ordinal);

        try
        {
            var fileId = this.GenerateFileId();
            var storageObject = new Object
            {
                Bucket = BucketName,
                Name = fileId,
                ContentType = file.ContentType,
            };
            if (!string.IsNullOrEmpty(cacheControl))
            {
                storageObject.CacheControl = cacheControl;
            }

            if (metadata.Count > 0)
            {
                storageObject.Metadata = metadata;
            }
            var dataObject = await StorageClient.UploadObjectAsync(
                storageObject,
                file.ContentStream,
                cancellationToken: cancellationToken);
            return dataObject.Name;
        }
        catch (Exception ex)
        {
            throw new FileStorageException(ex);
        }
    }

    /// <inheritdoc/>
    public virtual async Task DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        _ = fileId ?? throw new ArgumentNullException(nameof(fileId));
        try
        {
            await StorageClient.DeleteObjectAsync(BucketName, fileId, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new FileStorageException(ex);
        }
    }

    /// <inheritdoc/>
    public virtual string GenerateFileId()
    {
        return Guid.NewGuid().ToString();
    }
}