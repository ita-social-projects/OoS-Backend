using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Config.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    public class WorkshopImagesInteractionService :
        ChangeableImagesInteractionService<IWorkshopRepository, Workshop, Guid>,
        IWorkshopImagesInteractionService
    {
        public WorkshopImagesInteractionService(IImageService imageService, IWorkshopRepository repository, ILogger<ImageInteractionBaseService<IWorkshopRepository, Workshop, Guid>> logger, ImagesLimits<Workshop> limits) : base(imageService, repository, logger, limits)
        {
        }

        protected override async Task<Workshop> GetEntityWithIncludedImages(Guid entityId)
        {
            return (await Repository.GetByFilter(x => x.Id == entityId, nameof(Workshop.WorkshopImages)).ConfigureAwait(false)).FirstOrDefault();
        }

        protected override List<Image<Workshop>> GetEntityImages(Workshop entity) => entity.WorkshopImages;
    }
}
