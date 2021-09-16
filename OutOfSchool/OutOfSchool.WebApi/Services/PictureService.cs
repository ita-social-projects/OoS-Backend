using System;
using System.Linq;
using System.Threading.Tasks;
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

            var workshop = teacherRepository.GetById(teacherId).Result;

            var pictureMetadata = GetPictureMetadata(pictureId, workshop);

            return new PictureStorageModel
            {
                ContentStream = await pictureStorage.GetPictureByIdAsync(pictureMetadata.StorageId).ConfigureAwait(false),
                ContentType = pictureMetadata.ContentType,
            };
        }

        private PictureMetadata GetPictureMetadata(Guid pictureId, Workshop workshop)
        {
            var picture = workshop.WorkshopPictures.FirstOrDefault(x => x.Picture.Id == pictureId)?.Picture;

            if (picture == null)
            {
                logger.Error($"Picture {pictureId} was not found");
                throw new PictureNotFoundException(pictureId.ToString());
            }

            return picture;
        }

        private PictureMetadata GetPictureMetadata(Guid pictureId, Provider provider)
        {
            var picture = provider.ProviderPictures.FirstOrDefault(x => x.Picture.Id == pictureId)?.Picture;

            if (picture == null)
            {
                logger.Error($"Picture {pictureId} was not found");
                throw new PictureNotFoundException(pictureId.ToString());
            }

            return picture;
        }

        private PictureMetadata GetPictureMetadata(Guid pictureId, Teacher teacher)
        {
            var picture = teacher.TeacherPictures.FirstOrDefault(x => x.Picture.Id == pictureId)?.Picture;

            if (picture == null)
            {
                logger.Error($"Picture {pictureId} was not found");
                throw new PictureNotFoundException(pictureId.ToString());
            }

            return picture;
        }
    }
}
