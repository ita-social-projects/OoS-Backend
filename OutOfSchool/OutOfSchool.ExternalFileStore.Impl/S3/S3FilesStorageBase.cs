using Google.Apis.Util;
using Minio;
using Minio.DataModel.Args;
using OutOfSchool.ExternalFileStore.Exceptions;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.S3;

public abstract class S3FilesStorageBase<TFile>(IStorageContext<IMinioClient> storageContext)
    : IObjectStorage<TFile, string>
    where TFile : FileModel, new()
{
    private protected IMinioClient StorageClient { get; } = storageContext.StorageClient;

    private protected string BucketName { get; } = storageContext.BucketName;


    public async Task<TFile> GetByIdAsync(string fileId, CancellationToken cancellationToken = default)
    {
        _ = fileId ?? throw new ArgumentNullException(nameof(fileId));
        var fileStream = new MemoryStream();
        try
        {
            var args = new GetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(fileId)
                .WithCallbackStream(stream => stream.CopyTo(fileStream));
            var fileObject = await StorageClient.GetObjectAsync(
                args,
                cancellationToken);

            fileStream.Position = 0;
            return new TFile {ContentStream = fileStream, ContentType = fileObject.ContentType};
        }
        catch (Exception ex)
        {
            await fileStream.DisposeAsync();
            throw new FileStorageException(ex);
        }
    }

    public async Task<string> UploadAsync(TFile file, string cacheControl,
        IDictionary<string, string>? metadata, CancellationToken cancellationToken = default)
    {
        _ = file ?? throw new ArgumentNullException(nameof(file));
        metadata ??= new Dictionary<string, string>(StringComparer.Ordinal);

        if (!string.IsNullOrEmpty(cacheControl))
        {
            metadata["Cache-Control"] = cacheControl;
        }

        try
        {
            var fileId = this.GenerateFileId();
            file.ContentStream.Position = 0;
            var args = new PutObjectArgs()
                .WithBucket(BucketName)
                .WithObject(fileId)
                .WithStreamData(file.ContentStream)
                .WithObjectSize(file.ContentStream.Length)
                .WithContentType(file.ContentType)
                .WithHeaders(metadata);
            var dataObject = await StorageClient.PutObjectAsync(
                args,
                cancellationToken);
            return dataObject.ObjectName;
        }
        catch (Exception ex)
        {
            throw new FileStorageException(ex);
        }
    }

    public async Task DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        _ = fileId ?? throw new ArgumentNullException(nameof(fileId));
        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(BucketName)
                .WithObject(fileId);
            await StorageClient.RemoveObjectAsync(args, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new FileStorageException(ex);
        }
    }

    public IAsyncEnumerable<StorageObject> ListObjectsAsync(string? prefix = null, object? options = null)
    {
        if (options is ListObjectsArgs args)
        {
            args = args.WithPrefix(prefix).WithBucket(BucketName);
            return StorageClient.ListObjectsEnumAsync(args).Select(o => new StorageObject
            {
                Name = o.Key,
                ContentType = o.ContentType,
                Size = o.Size,
                // Dependency on Google static method, but it allows to unify implementation
                CreatedAt = DiscoveryFormat.ParseDateTimeToDateTimeOffset(o.LastModified),
                LastModified = DiscoveryFormat.ParseDateTimeToDateTimeOffset(o.LastModified)
            });
        }

        throw new ArgumentException($"Argument is not of required type {typeof(ListObjectsArgs)}", nameof(options));
    }

    /// <inheritdoc/>
    public virtual string GenerateFileId()
    {
        return Guid.NewGuid().ToString();
    }
}