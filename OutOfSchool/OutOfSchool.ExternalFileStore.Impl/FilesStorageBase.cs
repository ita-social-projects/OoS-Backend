using System.Text;
using OutOfSchool.ExternalFileStore.Exceptions;
using OutOfSchool.ExternalFileStore.Models;

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

        try
        {
            await DeleteOperationAsync(fileId, cancellationToken);
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
        var fileStream = new MemoryStream();

        try
        {
            return await GetByIdOperationAsync(fileId, fileStream, cancellationToken);
        }
        catch (Exception ex)
        {
            await fileStream.DisposeAsync();
            throw new FileStorageException(ex);
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<StorageObject> ListObjectsAsync(string? prefix = null, object? options = null)
    {
        return ListObjectsOperationAsync(prefix, options);
    }

    /// <inheritdoc/>
    public async Task<string> UploadAsync(TFile file, string? main_subfolder = null, string cacheControl = "", IDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        _ = file ?? throw new ArgumentNullException(nameof(file));
        metadata ??= new Dictionary<string, string>(StringComparer.Ordinal);
        var fileId = this.GenerateFileId();
        var fullFileName = CreateFullPathFromFileId(fileId, main_subfolder);
        var customFileName = String.Empty;

        try
        {
            if (metadata.TryGetValue("customFileName", out customFileName))
            {
                metadata.Remove("customFileName");
            }
            else
            {
                customFileName = fullFileName;
            }
            return await UploadOperationAsync(file, fullFileName: customFileName, cacheControl, metadata, cancellationToken);

        }
        catch (Exception ex)
        {
            throw new FileStorageException(ex);
        }
    }

    // These protected abstract methods must be overridden for each store that will be used in the application.
    protected abstract Task DeleteOperationAsync(string fileId, CancellationToken cancellationToken = default);
    protected abstract Task<TFile> GetByIdOperationAsync(string fileId, MemoryStream fileStream, CancellationToken cancellationToken = default);
    protected abstract IAsyncEnumerable<StorageObject> ListObjectsOperationAsync(string? prefix = null, object? options = null);
    protected abstract Task<string> UploadOperationAsync(TFile file, string fullFileName, string cacheControl = "",
        IDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    // Custom hash implementation used for consistent directory structure across platforms.
    // This approach avoids the platform-dependent behavior of string.GetHashCode().
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

    private static string CreateFullPathFromFileId(string fileId, string? prefix = null)
    {
        int hash = GetHashCodeString(fileId);
        int mask = 255;
        int firstDir = hash & mask;
        int secondDir = (hash >> 8) & mask;

        // Build the path using the directory separator and formatting each byte as two-digit hexadecimal.
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(prefix))
        {
            sb.Append(prefix.ToLower())
              .Append('/');
        }

        sb.Append(firstDir.ToString("x2"))
          .Append('/')
          .Append(secondDir.ToString("x2"))
          .Append('/')
          .Append(fileId);

        return sb.ToString();
    }
}
