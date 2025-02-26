using OutOfSchool.ExternalFileStore.Exceptions;
using OutOfSchool.ExternalFileStore.Models;
using System.Text;

namespace OutOfSchool.ExternalFileStore;

/// <summary>
/// Represents an abstract base file storage.
/// </summary>
/// <typeparam name="TFile">File model.</typeparam>
public abstract class FilesStorageBase<TFile, TStorageClient>(IStorageContext<TStorageClient> storageContext)
     : IObjectStorage<TFile, string>
     where TFile : FileModel, new()
{
    protected TStorageClient StorageClient { get; } = storageContext.StorageClient;

    protected string BucketName { get; } = storageContext.BucketName;

    /// <inheritdoc/>
    public async Task DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        _ = fileId ?? throw new ArgumentNullException(nameof(fileId));
        var fullFileName = CreateFullPathFromFileId(fileId);

        try
        {
            await DeleteOperationAsync(fullFileName, cancellationToken);
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

    /// <inheritdoc/>
    public async Task<TFile> GetByIdAsync(string fileId, CancellationToken cancellationToken = default)
    {
        _ = fileId ?? throw new ArgumentNullException(nameof(fileId));
        using var fileStream = new MemoryStream();
        var fullFileName = CreateFullPathFromFileId(fileId);

        try
        {
            return await GetByIdOperationAsync(fullFileName, fileStream, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new FileStorageException(ex);
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<StorageObject> ListObjectsAsync(string? prefix = null, object? options = null)
    {
        return ListObjectsOperationAsync(prefix, options);
    }

    /// <inheritdoc/>
    public async Task<string> UploadAsync(TFile file, string cacheControl = "", IDictionary<string, string>? metadata = null, 
        CancellationToken cancellationToken = default)
    {
        _ = file ?? throw new ArgumentNullException(nameof(file));
        metadata ??= new Dictionary<string, string>(StringComparer.Ordinal);
        var fileId = this.GenerateFileId();
        var fullFileName = CreateFullPathFromFileId(fileId);

        try
        {
            await UploadOperationAsync(file, fullFileName, cacheControl, metadata, cancellationToken);
            return fileId;
        }
        catch (Exception ex)
        {
            throw new FileStorageException(ex);
        }
    }

    protected abstract Task DeleteOperationAsync(string fullFileName, CancellationToken cancellationToken = default);
    protected abstract Task<TFile> GetByIdOperationAsync(string fullFileName, MemoryStream fileStream, CancellationToken cancellationToken = default);
    protected abstract IAsyncEnumerable<StorageObject> ListObjectsOperationAsync(string? prefix = null, object? options = null);
    protected abstract Task UploadOperationAsync(TFile file, string fullFileName, string cacheControl = "", 
        IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    private static int GetHashCodeString(string s)
    {
        int h = 0;
        if (!string.IsNullOrEmpty(s))
        {
            for (int i = 0; i < s.Length; i++)
            {
                h = (h << 5) - h + s[i];
            }
        }
        return h;
    }

    private static string CreateFullPathFromFileId(string fileId)
    {
        int hash = GetHashCodeString(fileId);
        int mask = 255;
        int firstDir = hash & mask;
        int secondDir = (hash >> 8) & mask;

        // Build the path using the directory separator and formatting each byte as two-digit hexadecimal.
        return new StringBuilder()
            .Append(Path.DirectorySeparatorChar)
            .Append(firstDir.ToString("x2"))
            .Append(Path.DirectorySeparatorChar)
            .Append(secondDir.ToString("x2"))
            .Append(Path.DirectorySeparatorChar)
            .Append(fileId)
            .ToString();
    }
}
