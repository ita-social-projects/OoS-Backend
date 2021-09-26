using System;
using System.IO;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Represents contract that must implement all picture service methods.
    /// </summary>
    public interface IPictureService
    {
        /// <summary>
        /// Gets picture from data storage.
        /// </summary>
        /// <param name="workshopId">Id of the Workshop entity.</param>
        /// <param name="pictureId">Id of the picture.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<PictureStorageModel> GetPictureWorkshop(long workshopId, Guid pictureId);

        /// <summary>
        /// Gets picture from data storage.
        /// </summary>
        /// <param name="providerId">Id of the Provider entity.</param>
        /// <param name="pictureId">Id of the picture.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<PictureStorageModel> GetPictureProvider(long providerId, Guid pictureId);

        /// <summary>
        /// Gets picture from data storage.
        /// </summary>
        /// <param name="teacherId">Id of the Teacher entity.</param>
        /// <param name="pictureId">Id of the picture.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task<PictureStorageModel> GetPictureTeacher(long teacherId, Guid pictureId);

        /// <summary>
        /// Upload picture to data storage.
        /// </summary>
        /// <param name="teacherId">Id of the Teacher entity.</param>
        /// <param name="file">Picture.</param>
        /// <param name="contentType">Content type of the picture.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task UploadPictureTeacher(long teacherId, Stream file, string contentType);

        /// <summary>
        /// Upload picture to data storage.
        /// </summary>
        /// <param name="workshopId">Id of the Workshop entity.</param>
        /// <param name="file">Picture.</param>
        /// <param name="contentType">Content type of the picture.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task UploadPictureWorkshop(long workshopId, Stream file, string contentType);

        /// <summary>
        /// Upload picture to data storage.
        /// </summary>
        /// <param name="providerId">Id of the Provider entity.</param>
        /// <param name="file">Picture.</param>
        /// <param name="contentType">Content type of the picture.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        Task UploadPictureProvider(long providerId, Stream file, string contentType);
    }
}
