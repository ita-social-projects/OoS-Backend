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
        private readonly ILogger logger;

        public PictureService(IPictureStorage pictureStorage, ILogger logger)
        {
            this.pictureStorage = pictureStorage ?? throw new ArgumentNullException(nameof(pictureStorage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PictureStorageModel> GetPictureWorkshop(long workshopId, Guid pictureId)
        {
            logger.Debug($"Getting picture {pictureId} for workshop {workshopId}");

            var workshop = workshopRepository.GetById(workshopId).Result;

            var pictureMetadata = GetWorkshopPictureMetadata(pictureId, workshop);

            return new PictureStorageModel
            {
                ContentStream = await pictureStorage.GetPictureByIdAsync(pictureMetadata.StorageId).ConfigureAwait(false),
                ContentType = pictureMetadata.ContentType,
            };
        }

        private PictureMetadata GetWorkshopPictureMetadata(Guid pictureId, Workshop workshop)
        {
            var picture = workshop.WorkshopPictures.FirstOrDefault(x => x.Picture.Id == pictureId)?.Picture;

            if (picture == null)
            {
                logger.Error($"Picture {pictureId} was not found");
                throw new PictureNotFoundException(pictureId.ToString());
            }

            return picture;
        }
    }
}
