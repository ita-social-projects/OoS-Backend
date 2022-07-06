using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Api.Gax;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Repository.Files;

public interface IFilesStorage<TFile, TIdentifier>
    where TFile : FileModel
{
    /// <summary>
    /// Returns the sequence of raw API responses, each of which contributes a page of
    /// files to this sequence.
    /// </summary>
    /// <param name="prefix">Files prefix to fetch.</param>
    /// <param name="options">Files options to fetch.</param>
    /// <returns>An asynchronous sequence of raw API responses, each containing a page of files.</returns>
    IAsyncEnumerable<Objects> GetBulkListsOfObjectsAsync(string prefix = null, ListObjectsOptions options = null);

    /// <summary>
    /// Asynchronously gets a file by its id.
    /// </summary>
    /// <param name="fileId">File id.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a file of type <see cref="TFile"/>.
    /// </returns>
    Task<TFile> GetByIdAsync(TIdentifier fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously uploads a file into the storage.
    /// </summary>
    /// <param name="file">File.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains an identifier of the file if it's uploaded.
    /// </returns>
    Task<TIdentifier> UploadAsync(TFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes a file from the storage.
    /// </summary>
    /// <param name="fileId">File id.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task DeleteAsync(TIdentifier fileId, CancellationToken cancellationToken = default);
}