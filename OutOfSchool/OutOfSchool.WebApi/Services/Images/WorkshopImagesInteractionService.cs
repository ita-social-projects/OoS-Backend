using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Common.SearchFilters;
using OutOfSchool.WebApi.Config.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    public sealed class WorkshopImagesInteractionService :
        ChangeableImagesInteractionService<IWorkshopRepository, Workshop, Guid>,
        IWorkshopImagesInteractionService
    {
        public WorkshopImagesInteractionService(IImageService imageService, IWorkshopRepository repository, ILogger<ImageInteractionBaseService<IWorkshopRepository, Workshop, Guid>> logger, IOptions<ImagesLimits<Workshop>> limits)
            : base(imageService, repository, logger, limits.Value)
        {
        }

        protected override EntitySearchFilter<Workshop> GetFilterForSearchingEntityByIdWithIncludedImages(Guid entityId)
        {
            return new EntitySearchFilter<Workshop>(x => x.Id == entityId, nameof(Workshop.WorkshopImages));
        }

        protected override List<Image<Workshop>> GetEntityImages(Workshop entity) => entity.WorkshopImages;
    }
}
