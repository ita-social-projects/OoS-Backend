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
        /// <param name="fileName">File name.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<PhotoDto> AddFile(PhotoDto photo, string fileName);

        /// <summary>
        /// Adds files to data storage.
        /// </summary>
        /// <param name="photos">Photos.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<List<PhotoDto>> AddFiles(List<PhotoDto> photos);

        /// <summary>
        /// Delete file from data storage.
        /// </summary>
        /// <param name="filePath">File path.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteFile(string filePath);

        /// <summary>
        /// Delete files from data storage.
        /// </summary>
        /// <param name="filePaths">File paths.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteFiles(List<string> filePaths);

        /// <summary>
        /// Gets files from data storage.
        /// </summary>
        /// <param name="entityId">Entity Id.</param>
        /// <param name="entityType">Entity type.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<List<byte[]>> GetFiles(long entityId, EntityType entityType);

        /// <summary>
        /// Gets paths of the files from data storage.
        /// </summary>
        /// <param name="entityId">Entity Id.</param>
        /// <param name="entityType">Entity type.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<List<string>> GetFilesPaths(long entityId, EntityType entityType);

        /// <summary>
        /// Gets path of the file from data storage.
        /// </summary>
        /// <param name="entityId">Entity Id.</param>
        /// <param name="entityType">Entity type.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<string> GetFilePath(long entityId, EntityType entityType);

        /// <summary>
        /// Update file from data storage.
        /// </summary>
        /// <param name="photo">Photo entity.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<PhotoDto> UpdateFile(PhotoDto photo);
    }
}
