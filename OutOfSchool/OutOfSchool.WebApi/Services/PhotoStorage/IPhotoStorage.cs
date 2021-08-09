using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services.PhotoStorage
{
    /// <summary>
    /// Represents contract that must implement all photo storage managers.
    /// </summary>
    public interface IPhotoStorage
    {
        /// <summary>
        /// Adds file to data storage.
        /// </summary>
        /// <param name="photo">Photo data.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<PhotoDto> AddFile(PhotoDto photo);

        /// <summary>
        /// Adds files to data storage.
        /// </summary>
        /// <param name="photos">Photos data.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<List<PhotoDto>> AddFiles(PhotosDto photos);

        /// <summary>
        /// Delete file from data storage.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteFile(string fileName);

        /// <summary>
        /// Delete files from data storage.
        /// </summary>
        /// <param name="fileNames">Files names.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteFiles(List<string> fileNames);

        /// <summary>
        /// Gets file from data storage.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<byte[]> GetFile(string fileName);

        /// <summary>
        /// Gets names of the files from data storage.
        /// </summary>
        /// <param name="entityId">Entity Id.</param>
        /// <param name="entityType">Entity type.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<List<string>> GetFilesNames(long entityId, EntityType entityType);

        /// <summary>
        /// Gets name of the file from data storage.
        /// </summary>
        /// <param name="entityId">Entity Id.</param>
        /// <param name="entityType">Entity type.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<string> GetFileName(long entityId, EntityType entityType);

        /// <summary>
        /// Update file from data storage.
        /// </summary>
        /// <param name="photo">Photo data.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateFile(PhotoDto photo);
    }
}
