using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.Services.Contexts;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository.Files;

/// <summary>
/// Represents a base file storage.
/// </summary>
/// <typeparam name="TFile">File model.</typeparam>
public abstract class GcpFilesStorageBase<TFile> : IFilesStorage<TFile, string>
    where TFile : FileModel, new()
{
    protected GcpFilesStorageBase(IGcpStorageContext storageContext)
    {
        StorageClient = storageContext.StorageClient;
        BucketName = storageContext.BucketName;
    }

    private protected StorageClient StorageClient { get; }

    private protected string BucketName { get; }

    /// <inheritdoc/>
    public IAsyncEnumerable<Objects> GetBulkListsOfObjectsAsync(string prefix = null, ListObjectsOptions options = null)
        => StorageClient.ListObjectsAsync(BucketName, prefix: prefix, options: options).AsRawResponses();

    /// <inheritdoc/>
    public virtual async Task<TFile> GetByIdAsync(string fileId, CancellationToken cancellationToken = default)
    {
        _ = fileId ?? throw new ArgumentNullException(nameof(fileId));
        try
        {
            var fileObject = await StorageClient.GetObjectAsync(
                BucketName,
                fileId,
                cancellationToken: cancellationToken);

            var fileStream = new MemoryStream();
            await StorageClient.DownloadObjectAsync(
                fileObject,
                fileStream,
                cancellationToken: cancellationToken);

            fileStream.Position = 0;
            return new TFile { ContentStream = fileStream, ContentType = fileObject.ContentType };
        }
        catch (GoogleApiException)
        {
            return null;
        }
        catch (Exception ex)
        {
            throw new FileStorageException(ex);
        }
    }

    /// <inheritdoc/>
    public virtual async Task<string> UploadAsync(TFile file, CancellationToken cancellationToken = default)
    {
        _ = file ?? throw new ArgumentNullException(nameof(file));

        try
        {
            var fileId = GenerateFileId();
            var dataObject = await StorageClient.UploadObjectAsync(
                BucketName,
                fileId,
                file.ContentType,
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

    /// <summary>
    /// This method generates a unique value that is used as file identifier.
    /// </summary>
    /// <returns>
    /// The result contains a string value of the file if it's uploaded.
    /// </returns>
    protected virtual string GenerateFileId()
    {
        return Guid.NewGuid().ToString();
    }
}