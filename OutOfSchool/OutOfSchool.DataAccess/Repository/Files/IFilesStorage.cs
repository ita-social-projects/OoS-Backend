using System.Threading;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Repository.Files
{
    public interface IFilesStorage<TFile, TIdentifier>
        where TFile : FileModel
    {
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
}