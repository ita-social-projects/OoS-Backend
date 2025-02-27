using Google.Apis.Util;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using OutOfSchool.ExternalFileStore.Exceptions;
using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore.S3;

public abstract class S3FilesStorageBase<TFile>(IStorageContext<IMinioClient> storageContext)
    : FilesStorageBase<TFile, IMinioClient>(storageContext)
    where TFile : FileModel, new()
{
    protected sealed override async Task<TFile> GetByIdOperationAsync(string fileId, MemoryStream fileStream, CancellationToken cancellationToken = default)
    {
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

            return new TFile { ContentStream = fileStream, ContentType = fileObject.ContentType };
        }
        catch (MinioException ex)
        {
            await fileStream.DisposeAsync();
            throw new FileStorageException(ex);
        }
    }

    protected sealed override async Task<string> UploadOperationAsync(TFile file, string fullFileName, string cacheControl = "",
        IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(cacheControl))
        {
            metadata["Cache-Control"] = cacheControl;
        }

        file.ContentStream.Position = 0;
        var args = new PutObjectArgs()
            .WithBucket(BucketName)
            .WithObject(fullFileName)
            .WithStreamData(file.ContentStream)
            .WithObjectSize(file.ContentStream.Length)
            .WithContentType(file.ContentType)
            .WithHeaders(metadata);
        var dataObject = await StorageClient.PutObjectAsync(
                args,
                cancellationToken);

        return dataObject.ObjectName;
    }

    protected sealed override async Task DeleteOperationAsync(string fileId, CancellationToken cancellationToken = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(BucketName)
            .WithObject(fileId);

        await StorageClient.RemoveObjectAsync(args, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    protected sealed override IAsyncEnumerable<StorageObject> ListObjectsOperationAsync(string? prefix = null, object? options = null)
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
}