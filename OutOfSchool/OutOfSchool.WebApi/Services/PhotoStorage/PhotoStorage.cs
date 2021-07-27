using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services.PhotoStorage
{
    public class PhotoStorage : IPhotoStorage
    {
        private readonly IEntityRepository<Photo> repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhotoStorage"/> class.
        /// </summary>
        /// <param name="repository">Repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public PhotoStorage(IEntityRepository<Photo> repository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.repository = repository;
            this.logger = logger;
            this.localizer = localizer;
        }

        public static string FilePath { get; set; }

        /// <inheritdoc/>
        public async Task<PhotoDto> AddFile(PhotoDto photo, string fileName)
        {
            if (photo is null)
            {
                throw new ArgumentNullException(localizer["Photo can not be null!."]);
            }

            if (fileName is null)
            {
                throw new ArgumentNullException(localizer["File name can not be null!."]);
            }

            try
            {
                logger.Information("Process of creating photo started.");

                var dirPath = Path.GetDirectoryName(FilePath);

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var filePath = $"{FilePath}\\{fileName.TrimEnd()}";

                using (var fileStream = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    await fileStream.WriteAsync(photo.Photo, 0, photo.Photo.Length).ConfigureAwait(false);
                }

                photo.Path = filePath;

                var photoInfo = await repository.Create(photo.ToDomain()).ConfigureAwait(false);

                logger.Information($"Photo with Id = {photoInfo?.Id} created successfully.");

                return photoInfo.ToModel();
            }
            catch (ArgumentException ex)
            {
                logger.Error($"Process of creating photo failed. {ex}");
                throw;
            }
            catch (IOException ex)
            {
                logger.Error($"Process of creating photo failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<PhotoDto>> AddFiles(List<PhotoDto> photos)
        {
            if (photos is null)
            {
                throw new ArgumentNullException(localizer["Photos can not be null!."]);
            }

            try
            {
                logger.Information("Process of creating photos started.");

                var dirPath = Path.GetDirectoryName(FilePath);

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var createdPhotos = new List<PhotoDto>();

                for (int i = 0; i < photos.Count; i++)
                {
                    var fileName = $"{photos[i].EntityId}_{photos[i].EntityType}_{i}.{photos[i].PhotoExtension.ToString().ToLower()}";

                    var filePath = $"{FilePath}\\{fileName.TrimEnd()}";

                    photos[i].Path = filePath;

                    using (var fileStream = File.Open(filePath, FileMode.OpenOrCreate))
                    {
                        await fileStream.WriteAsync(photos[i].Photo, 0, photos[i].Photo.Length).ConfigureAwait(false);
                    }

                    var photoInfo = await repository.Create(photos[i].ToDomain()).ConfigureAwait(false);

                    logger.Information($"Photos created successfully.");

                    createdPhotos.Add(photoInfo.ToModel());
                }

                return createdPhotos;
            }
            catch (ArgumentException ex)
            {
                logger.Error($"Process of creating photos failed. {ex}");
                throw;
            }
            catch (IOException ex)
            {
                logger.Error($"Process of creating photos failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteFile(string filePath)
        {
            if (filePath is null)
            {
                throw new ArgumentNullException(localizer["File path can not be null!"]);
            }

            try
            {
                logger.Information($"Process of deleting photo started.");

                File.Delete(filePath);

                Expression<Func<Photo, bool>> filter = p => p.Path == filePath;

                var photo = await repository.GetByFilter(filter).ConfigureAwait(false);

                await repository.Delete(photo.FirstOrDefault()).ConfigureAwait(false);

                logger.Information($"Photo deleted photo successfully.");
            }
            catch (ArgumentException ex)
            {
                logger.Error($"Process of deleting photo failed. {ex}");
                throw;
            }
            catch (IOException ex)
            {
                logger.Error($"Process of deleting photo failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteFiles(List<string> filesPaths)
        {
            if (filesPaths is null)
            {
                throw new ArgumentNullException(localizer["File paths can not be null!"]);
            }

            try
            {
                logger.Information($"Process of deleting photos started.");

                foreach (var path in filesPaths)
                {
                    File.Delete(path);

                    Expression<Func<Photo, bool>> filter = p => p.Path == path;

                    var photos = await repository.GetByFilter(filter).ConfigureAwait(false);

                    await repository.Delete(photos.FirstOrDefault()).ConfigureAwait(false);
                }

                logger.Information($"Photo deleted photo successfully.");
            }
            catch (ArgumentException ex)
            {
                logger.Error($"Process of deleting photos failed. {ex}");
                throw;
            }
            catch (IOException ex)
            {
                logger.Error($"Process of deleting photos failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<byte[]>> GetFiles(long entityId, EntityType entityType)
        {
            try
            {
                logger.Information($"Process of getting photos started.");

                var photos = new List<byte[]>();

                Expression<Func<Photo, bool>> filter = p => p.EntityId == entityId && p.EntityType == entityType;

                var images = await repository.GetByFilter(filter).ConfigureAwait(false);

                foreach (var image in images)
                {
                    using (var stream = File.Open(image.Path, FileMode.Open))
                    {
                        byte[] fileContent;

                        using (var ms = new MemoryStream())
                        {
                            await stream.CopyToAsync(ms).ConfigureAwait(false);

                            fileContent = ms.ToArray();

                            photos.Add(fileContent);
                        }
                    }
                }

                logger.Information($"Successfully got photos.");

                return photos;
            }
            catch (ArgumentException ex)
            {
                logger.Error($"Process of getting photos failed. {ex}");
                throw;
            }
            catch (IOException ex)
            {
                logger.Error($"Process of getting photos failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetFilesPaths(long entityId, EntityType entityType)
        {
            try
            {
                logger.Information($"Process of getting paths of the photos started.");

                var paths = new List<string>();

                Expression<Func<Photo, bool>> filter = p => p.EntityId == entityId && p.EntityType == entityType;

                var photos = await repository.GetByFilter(filter).ConfigureAwait(false);

                foreach (var photo in photos)
                {
                    paths.Add(photo.Path);
                }

                logger.Information($"Successfully got paths of the photos.");

                return paths;
            }
            catch (ArgumentException ex)
            {
                logger.Error($"Process of getting paths of the photos failed. {ex}");
                throw;
            }
            catch (IOException ex)
            {
                logger.Error($"Process of getting paths of the photos failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetFilePath(long entityId, EntityType entityType)
        {
            try
            {
                logger.Information($"Process of getting path of the photo started.");

                Expression<Func<Photo, bool>> filter = p => p.EntityId == entityId && p.EntityType == entityType;

                var photos = await repository.GetByFilter(filter).ConfigureAwait(false);

                logger.Information($"Successfully got path of the photo.");

                return photos.FirstOrDefault().Path;
            }
            catch (ArgumentException ex)
            {
                logger.Error($"Process of getting path of the photo failed. {ex}");
                throw;
            }
            catch (IOException ex)
            {
                logger.Error($"Process of getting path of the photo failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PhotoDto> UpdateFile(PhotoDto photo)
        {
            if (photo is null)
            {
                throw new ArgumentNullException(localizer["Photo can not be null!"]);
            }

            try
            {
                logger.Information($"Process of updating the photo started.");

                using (var fileStream = File.Open(photo.Path, FileMode.Create))
                {
                    await fileStream.WriteAsync(photo.Photo, 0, photo.Photo.Length).ConfigureAwait(false);
                }

                logger.Information($"Successfully update the photo.");

                return photo;
            }
            catch (ArgumentException ex)
            {
                logger.Error($"Process of updating the photo failed. {ex}");
                throw;
            }
        }
    }
}
