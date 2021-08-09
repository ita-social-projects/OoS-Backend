using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        private readonly IPhotoRepository repository;
        private readonly IEntityRepository<Photo> repositoryDB;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhotoStorage"/> class.
        /// </summary>
        /// <param name="repository">Repository to work with Photo storage.</param>
        /// <param name="repositoryDB">Repository to work with Photo Entity.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public PhotoStorage(IPhotoRepository repository, IEntityRepository<Photo> repositoryDB, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.repository = repository;
            this.repositoryDB = repositoryDB;
            this.logger = logger;
            this.localizer = localizer;
        }

        /// <inheritdoc/>
        public async Task<PhotoDto> AddFile(PhotoDto photo)
        {
            try
            {
                logger.Information("Process of creating photo started.");

                if (photo is null)
                {
                    throw new ArgumentNullException(localizer["Photo can not be null!."]);
                }

                photo.FileName = $"{photo.EntityId}_{photo.EntityType}{Path.GetExtension(photo.File.FileName)}";

                var requiredSize = GetSizeByEntity(photo.EntityType);

                await CreateUpdatePhoto(photo.File, Path.Combine(photo.EntityType.ToString(), photo.FileName), requiredSize).ConfigureAwait(false);

                var createdPhoto = await repositoryDB.Create(photo.ToDomain()).ConfigureAwait(false);

                logger.Information($"Photo with Id = {photo?.Id} created successfully.");

                return createdPhoto.ToModel();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                logger.Error($"Process of creating photo failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<PhotoDto>> AddFiles(PhotosDto photos)
        {
            try
            {
                logger.Information("Process of creating photos started.");

                if (photos is null)
                {
                    throw new ArgumentNullException(localizer["Photos can not be null!."]);
                }

                var createdPhotos = new List<PhotoDto>();

                var imgSize = GetSizeByEntity(photos.EntityType);

                for (int i = 0; i < photos.Files.Count; i++)
                {
                    photos.FileName = $"{photos.EntityId}_{photos.EntityType}_{i}{Path.GetExtension(photos.Files[i].FileName)}";

                    await CreateUpdatePhoto(photos.Files[i], Path.Combine(photos.EntityType.ToString(), photos.FileName), imgSize).ConfigureAwait(false);

                    var cratedPhotoInfo = await repositoryDB.Create(photos.ToDomain()).ConfigureAwait(false);

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
        public async Task DeleteFile(string fileName)
        {
            try
            {
                logger.Information($"Process of deleting photo started.");

                if (fileName is null)
                {
                    throw new ArgumentNullException(localizer["File name can not be null!"]);
                }

                var photos = await GetFilesByName(fileName).ConfigureAwait(false);

                var photo = photos.FirstOrDefault();

                repository.DeletePhoto(Path.Combine(photo.EntityType.ToString(), photo.FileName));

                await repositoryDB.Delete(photo).ConfigureAwait(false);

                logger.Information($"Photo deleted photo successfully.");
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                logger.Error($"Process of deleting photo failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteFiles(List<string> filesNames)
        {
            try
            {
                logger.Information($"Process of deleting photos started.");

                if (filesNames is null)
                {
                    throw new ArgumentNullException(localizer["Files names can not be null!"]);
                }

                foreach (var name in filesNames)
                {
                    var photos = await GetFilesByName(name).ConfigureAwait(false);

                    var photo = photos.FirstOrDefault();

                    repository.DeletePhoto(Path.Combine(photo.EntityType.ToString(), photo.FileName));

                    await repositoryDB.Delete(photo).ConfigureAwait(false);
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
        public async Task<byte[]> GetFile(string fileName)
        {
            try
            {
                logger.Information($"Process of getting photo started.");

                var photosInfo = await GetFilesByName(fileName).ConfigureAwait(false);

                var photoInfo = photosInfo.FirstOrDefault();

                MimeTypeMap.GetMimeType(Path.GetExtension(photoInfo.FileName));

                var file = await repository.GetPhoto(Path.Combine(photoInfo.EntityType.ToString(), photoInfo.FileName)).ConfigureAwait(false);

                logger.Information($"Successfully got photo.");

                return file;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                logger.Error($"Process of getting photo failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetFilesNames(long entityId, EntityType entityType)
        {
            try
            {
                logger.Information($"Process of getting names of the photos started.");

                var paths = new List<string>();

                var photos = await GetFilesByEntity(entityId, entityType).ConfigureAwait(false);

                foreach (var photo in photos)
                {
                    paths.Add(photo.FileName);
                }

                logger.Information($"Successfully got names of the photos.");

                return paths;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                logger.Error($"Process of getting names of the photos failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetFileName(long entityId, EntityType entityType)
        {
            try
            {
                logger.Information($"Process of getting name of the photo started.");

                var photos = await GetFilesByEntity(entityId, entityType).ConfigureAwait(false);

                logger.Information($"Successfully got name of the photo.");

                return photos.FirstOrDefault().FileName;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException)
            {
                logger.Error($"Process of getting name of the photo failed. {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateFile(PhotoDto photo)
        {
            try
            {
                logger.Information($"Process of updating the photo started.");

                if (photo is null)
                {
                    throw new ArgumentNullException(localizer["Photo can not be null!"]);
                }

                var requiredSize = GetSizeByEntity(photo.EntityType);

                var fileName = Path.Combine(photo.EntityType.ToString(), photo.FileName);

                await CreateUpdatePhoto(photo.File, fileName, requiredSize).ConfigureAwait(false);

                logger.Information($"Successfully update the photo.");
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

            return await repositoryDB.GetByFilter(filter).ConfigureAwait(false);
        }

        private async Task<IEnumerable<Photo>> GetFilesByName(string fileName)
        {
            Expression<Func<Photo, bool>> filter = p => p.FileName == fileName;

            return await repositoryDB.GetByFilter(filter).ConfigureAwait(false);
        }

        private byte[] ImageToByte(Bitmap image)
        {
            using (var stream = new MemoryStream())
            {
                long quality = 75;
                var qualityParamId = Encoder.Quality;
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(qualityParamId, quality);
                var codec = ImageCodecInfo.GetImageDecoders()
                    .FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);

                image.Save(stream, codec, encoderParameters);

                return stream.ToArray();
            }
        }

        private Size GetSizeByEntity(EntityType entityType)
        {
            if (entityType is EntityType.Teacher)
            {
                return new Size(512, 512);
            }
            else
            {
                return new Size(1280, 720);
            }
        }

        private async Task CreateUpdatePhoto(IFormFile photo, string fileName, Size requiredSize)
        {
            using (var memoryStream = new MemoryStream())
            {
                await photo.CopyToAsync(memoryStream).ConfigureAwait(false);

                using (var img = Image.FromStream(memoryStream))
                {
                    var scale = new Scale(new Size(img.Width, img.Height), requiredSize);

                    var bitmap = Scale.ResizeImage(img, scale.TargetRect.Width, scale.TargetRect.Height);

                    await repository.CreateUpdatePhoto(ImageToByte(bitmap), fileName).ConfigureAwait(false);
                }
            }
        }
    }
}
