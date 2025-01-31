using OutOfSchool.ExternalFileStore.Models;

namespace OutOfSchool.ExternalFileStore;

public interface IFilesStorage<TFile, TIdentifier>
    where TFile : FileModel
{
    /// <summary>
    /// Asynchronously gets a file by its id.
    /// </summary>
    /// <param name="fileId">File id.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a file of type <see cref="TFile"/> or null if it was not found.
    /// </returns>
    Task<TFile> GetByIdAsync(TIdentifier fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously uploads a file into the storage.
    /// </summary>
    /// <param name="file">File.</param>
    /// <param name="cacheControl">Value of Cache-Control header/metadata for objects. Uses provider default if empty.</param>
    /// <param name="metadata">Additional object metadata.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains an identifier of the file if it's uploaded.
    /// </returns>
    Task<TIdentifier> UploadAsync(TFile file, string cacheControl = "", IDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes a file from the storage.
    /// </summary>
    /// <param name="fileId">File id.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task DeleteAsync(TIdentifier fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// This method generates a unique value that is used as file identifier.
    /// </summary>
    /// <returns>
    /// The result contains a string value of the file if it's uploaded.
    /// </returns>
    TIdentifier GenerateFileId();
}