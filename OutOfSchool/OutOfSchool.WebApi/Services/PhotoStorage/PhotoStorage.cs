using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
        /// <param name="config">Config.</param>
        public PhotoStorage(IEntityRepository<Photo> repository, ILogger logger, IStringLocalizer<SharedResource> localizer, IConfiguration config)
        {
            this.repository = repository;
            this.logger = logger;
            this.localizer = localizer;
            FilePath = config.GetValue<string>("BasePhotoPath");
        }

        private static string FilePath { get; set; }

        /// <inheritdoc/>
        public async Task<PhotoDto> AddFile(IFormFile photo, PhotoDto photoInfo, string fileName)
        {
            try
            {
                logger.Information("Process of creating photo started.");

                if (photo is null)
                {
                    throw new ArgumentNullException(localizer["Photo can not be null!."]);
                }

                if (fileName is null)
                {
                    throw new ArgumentNullException(localizer["File name can not be null!."]);
                }

                var dirPath = Path.GetDirectoryName(FilePath);

                Directory.CreateDirectory(dirPath);

                var filePath = Path.Combine(FilePath, photoInfo.EntityType.ToString(), fileName.TrimEnd());

                photoInfo.FileName = filePath;

                using (var fileStream = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    await photo.CopyToAsync(fileStream).ConfigureAwait(false);
                }

                var createdPhoto = await repository.Create(photoInfo.ToDomain()).ConfigureAwait(false);

                logger.Information($"Photo with Id = {photoInfo?.Id} created successfully.");

                return createdPhoto.ToModel();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                logger.Error($"Process of creating photo failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<PhotoDto>> AddFiles(IFormFileCollection photos, PhotoDto photoInfo)
        {
            try
            {
                logger.Information("Process of creating photos started.");

                if (photos is null)
                {
                    throw new ArgumentNullException(localizer["Photos can not be null!."]);
                }

                var dirPath = Path.GetDirectoryName(FilePath);

                Directory.CreateDirectory(dirPath);

                var createdPhotos = new List<PhotoDto>();

                for (int i = 0; i < photos.Count; i++)
                {
                    var fileName = $"{photoInfo.EntityId}_{photoInfo.EntityType}_{i}{Path.GetExtension(photos[i].FileName)}";

                    var filePath = Path.Combine(FilePath, photoInfo.EntityType.ToString(), fileName.TrimEnd());

                    photoInfo.FileName = filePath;

                    using (var fileStream = File.Open(filePath, FileMode.OpenOrCreate))
                    {
                        await photos[i].CopyToAsync(fileStream).ConfigureAwait(false);
                    }

                    var cratedPhotoInfo = await repository.Create(photoInfo.ToDomain()).ConfigureAwait(false);

                    logger.Information($"Photos created successfully.");

                    createdPhotos.Add(cratedPhotoInfo.ToModel());
                }

                return createdPhotos;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                logger.Error($"Process of creating photos failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteFile(string filePath)
        {
            try
            {
                logger.Information($"Process of deleting photo started.");

                if (filePath is null)
                {
                    throw new ArgumentNullException(localizer["File path can not be null!"]);
                }

                File.Delete(filePath);

                var photo = await GetFilesByPath(filePath).ConfigureAwait(false);

                await repository.Delete(photo.FirstOrDefault()).ConfigureAwait(false);

                logger.Information($"Photo deleted photo successfully.");
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                logger.Error($"Process of deleting photo failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteFiles(List<string> filesPaths)
        {
            try
            {
                logger.Information($"Process of deleting photos started.");

                if (filesPaths is null)
                {
                    throw new ArgumentNullException(localizer["File paths can not be null!"]);
                }

                foreach (var path in filesPaths)
                {
                    File.Delete(path);

                    var photos = await GetFilesByPath(path).ConfigureAwait(false);

                    await repository.Delete(photos.FirstOrDefault()).ConfigureAwait(false);
                }

                logger.Information($"Photo deleted photo successfully.");
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                logger.Error($"Process of deleting photos failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetFile(long entityId, EntityType entityType)
        {
            try
            {
                logger.Information($"Process of getting photos started.");

                var photosInfo = await GetFilesByEntity(entityId, entityType).ConfigureAwait(false);

                var photoInfo = photosInfo.FirstOrDefault();

                MimeTypeMap.GetMimeType(Path.GetExtension(photoInfo.FileName));

                var file = await File.ReadAllBytesAsync(photoInfo.FileName).ConfigureAwait(false);

                logger.Information($"Successfully got photos.");

                return file;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
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

                var photos = await GetFilesByEntity(entityId, entityType).ConfigureAwait(false);

                foreach (var photo in photos)
                {
                    paths.Add(photo.FileName);
                }

                logger.Information($"Successfully got paths of the photos.");

                return paths;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
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

                var photos = await GetFilesByEntity(entityId, entityType).ConfigureAwait(false);

                logger.Information($"Successfully got path of the photo.");

                return photos.FirstOrDefault().FileName;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                logger.Error($"Process of getting path of the photo failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PhotoDto> UpdateFile(PhotoDto photoInfo, IFormFile photo)
        {
            try
            {
                logger.Information($"Process of updating the photo started.");

                if (photo is null)
                {
                    throw new ArgumentNullException(localizer["Photo can not be null!"]);
                }

                using (var fileStream = File.Open(photoInfo.FileName, FileMode.Create))
                {
                    await photo.CopyToAsync(fileStream).ConfigureAwait(false);
                }

                logger.Information($"Successfully update the photo.");

                return photoInfo;
            }
            catch (ArgumentException ex)
            {
                logger.Error($"Process of updating the photo failed. {ex}");
                throw;
            }
        }

        private async Task<IEnumerable<Photo>> GetFilesByEntity(long entityId, EntityType entityType)
        {
            Expression<Func<Photo, bool>> filter = p => p.EntityId == entityId && p.EntityType == entityType;

            return await repository.GetByFilter(filter).ConfigureAwait(false);
        }

        private async Task<IEnumerable<Photo>> GetFilesByPath(string filePath)
        {
            Expression<Func<Photo, bool>> filter = p => p.FileName == filePath;

            return await repository.GetByFilter(filter).ConfigureAwait(false);
        }
    }
}
