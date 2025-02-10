using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore;

/// <summary>
/// Represents additional operations available in object storage systems.
/// </summary>
public interface IObjectStorage<TFile, TIdentifier>: IFilesStorage<TFile, TIdentifier>
    where TFile : FileModel
{
    /// <summary>
    /// Lists objects in the storage with the specified prefix.
    /// </summary>
    /// <param name="prefix">Optional prefix to filter objects.</param>
    /// <param name="options">Optional listing options specific to the storage provider.</param>
    /// <returns>An asynchronous sequence of storage objects.</returns>
    IAsyncEnumerable<StorageObject> ListObjectsAsync(string? prefix = null, object? options = null);
} 