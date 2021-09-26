using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common.Exceptions;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    public class PictureService : IPictureService
    {
        private readonly IPictureStorage pictureStorage;
        private readonly IWorkshopRepository workshopRepository;
        private readonly IProviderRepository providerRepository;
        private readonly IEntityRepository<Teacher> teacherRepository;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureService"/> class.
        /// </summary>
        /// <param name="pictureStorage">Repository to work with picture storage.</param>
        /// <param name="workshopRepository">Repository to work with Workshop Entity.</param>
        /// <param name="providerRepository">Repository to work with Provider Entity.</param>
        /// <param name="teacherRepository">Repository to work with Teacher Entity.</param>
        /// <param name="logger">Logger.</param>
        public PictureService(IPictureStorage pictureStorage, IWorkshopRepository workshopRepository, IProviderRepository providerRepository, IEntityRepository<Teacher> teacherRepository, ILogger logger)
        {
            this.workshopRepository = workshopRepository ?? throw new ArgumentNullException(nameof(workshopRepository));
            this.providerRepository = providerRepository ?? throw new ArgumentNullException(nameof(providerRepository));
            this.teacherRepository = teacherRepository ?? throw new ArgumentNullException(nameof(teacherRepository));
            this.pictureStorage = pictureStorage ?? throw new ArgumentNullException(nameof(pictureStorage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<PictureStorageModel> GetPictureWorkshop(long workshopId, Guid pictureId)
        {
            logger.Debug($"Getting picture {pictureId} for workshop {workshopId}");

            var workshop = workshopRepository.GetById(workshopId).Result;

            if (workshop == null)
            {
                logger.Error($"Workshop {workshopId} was not found");
                throw new EntityNotFoundException(nameof(workshopId));
            }

            var pictureMetadata = GetPictureMetadata(pictureId, workshop);

            return new PictureStorageModel
            {
                ContentStream = await pictureStorage.GetPictureByIdAsync(pictureMetadata.StorageId).ConfigureAwait(false),
                ContentType = pictureMetadata.ContentType,
            };
        }

        /// <inheritdoc/>
        public async Task<PictureStorageModel> GetPictureProvider(long providerId, Guid pictureId)
        {
            logger.Debug($"Getting picture {pictureId} for provider {providerId}");

            var provider = providerRepository.GetById(providerId).Result;

            if (provider == null)
            {
                logger.Error($"Provider {providerId} was not found");
                throw new EntityNotFoundException(nameof(providerId));
            }

            var pictureMetadata = GetPictureMetadata(pictureId, provider);

            return new PictureStorageModel
            {
                ContentStream = await pictureStorage.GetPictureByIdAsync(pictureMetadata.StorageId).ConfigureAwait(false),
                ContentType = pictureMetadata.ContentType,
            };
        }

        /// <inheritdoc/>
        public async Task<PictureStorageModel> GetPictureTeacher(long teacherId, Guid pictureId)
        {
            logger.Debug($"Getting picture {pictureId} for workshop {teacherId}");

            var teacher = teacherRepository.GetById(teacherId).Result;

            if (teacher == null)
            {
                logger.Error($"Teacher {teacherId} was not found");
                throw new EntityNotFoundException(nameof(teacherId));
            }

            var pictureMetadata = GetPictureMetadata(pictureId, teacher);

            return new PictureStorageModel
            {
                ContentStream = await pictureStorage.GetPictureByIdAsync(pictureMetadata.StorageId).ConfigureAwait(false),
                ContentType = pictureMetadata.ContentType,
            };
        }

        /// <inheritdoc/>
        public async Task UploadPictureTeacher(long teacherId, Stream file, string contentType)
        {
            // TODO: maybe find the better solution how to handle if error occurs after picture was created.
            ObjectId pictureObjectId = default;

            try
            {
                logger.Debug($"Uploading picture for teacher {teacherId}");

                var teacher = teacherRepository.GetById(teacherId).Result;

                var pictureId = await pictureStorage.UploadPicture(file, CancellationToken.None).ConfigureAwait(false);

                pictureObjectId = new ObjectId(pictureId);

                var teacherPicture = new TeacherPicture
                {
                    Picture = new PictureMetadata
                    {
                        ContentType = contentType,
                        StorageId = pictureId,
                    },
                };

                teacher.TeacherPictures.Add(teacherPicture);

                await teacherRepository.Update(teacher).ConfigureAwait(false);
            }
            catch (PictureStorageException ex)
            {
                logger.Error($"An error occurred while uploading the picture {ex}");
                throw;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Error($"An error occurred while uploading the picture {ex}");
                await pictureStorage.DeletePicture(pictureObjectId, CancellationToken.None).ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while uploading the picture {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UploadPictureWorkshop(long workshopId, Stream file, string contentType)
        {
            // TODO: maybe find the better solution how to handle if error occurs after picture was created.
            ObjectId pictureObjectId = default;

            try
            {
                logger.Debug($"Uploading picture for workshop {workshopId}");

                var workshop = workshopRepository.GetById(workshopId).Result;

                var pictureId = await pictureStorage.UploadPicture(file, CancellationToken.None).ConfigureAwait(false);

                pictureObjectId = new ObjectId(pictureId);

                var workshopPicture = new WorkshopPicture
                {
                    Picture = new PictureMetadata
                    {
                        ContentType = contentType,
                        StorageId = pictureId,
                    },
                };

                workshop.WorkshopPictures.Add(workshopPicture);

                await workshopRepository.Update(workshop).ConfigureAwait(false);
            }
            catch (PictureStorageException ex)
            {
                logger.Error($"An error occurred while uploading the picture {ex}");
                throw;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Error($"An error occurred while uploading the picture {ex}");
                await pictureStorage.DeletePicture(pictureObjectId, CancellationToken.None).ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while uploading the picture {ex}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UploadPictureProvider(long providerId, Stream file, string contentType)
        {
            // TODO: maybe find the better solution how to handle if error occurs after picture was created.
            ObjectId pictureObjectId = default;

            try
            {
                logger.Debug($"Uploading picture for provider {providerId}");

                var provider = providerRepository.GetById(providerId).Result;

                var pictureId = await pictureStorage.UploadPicture(file, CancellationToken.None).ConfigureAwait(false);

                pictureObjectId = new ObjectId(pictureId);

                var providerPicture = new ProviderPicture
                {
                    Picture = new PictureMetadata
                    {
                        ContentType = contentType,
                        StorageId = pictureId,
                    },
                };

                provider.ProviderPictures.Add(providerPicture);

                await providerRepository.Update(provider).ConfigureAwait(false);
            }
            catch (PictureStorageException ex)
            {
                logger.Error($"An error occurred while uploading the picture {ex}");
                throw;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                logger.Error($"An error occurred while uploading the picture {ex}");
                await pictureStorage.DeletePicture(pictureObjectId, CancellationToken.None).ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while uploading the picture {ex}");
                throw;
            }
        }

        private PictureMetadata GetPictureMetadata(Guid pictureId, Workshop workshop)
        {
            var picture = workshop.WorkshopPictures.FirstOrDefault(x => x.Picture.Id == pictureId)?.Picture;

            if (picture == null)
            {
                logger.Error($"Picture {pictureId} was not found");
                throw new FileNotFoundException(pictureId.ToString());
            }

            return picture;
        }

        private PictureMetadata GetPictureMetadata(Guid pictureId, Provider provider)
        {
            var picture = provider.ProviderPictures.FirstOrDefault(x => x.Picture.Id == pictureId)?.Picture;

            if (picture == null)
            {
                logger.Error($"Picture {pictureId} was not found");
                throw new FileNotFoundException(pictureId.ToString());
            }

            return picture;
        }

        private PictureMetadata GetPictureMetadata(Guid pictureId, Teacher teacher)
        {
            var picture = teacher.TeacherPictures.FirstOrDefault(x => x.Picture.Id == pictureId)?.Picture;

            if (picture == null)
            {
                logger.Error($"Picture {pictureId} was not found");
                throw new FileNotFoundException(pictureId.ToString());
            }

            return picture;
        }

        
    }
}
