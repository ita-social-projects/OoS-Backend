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
    /// <summary>
    /// Represents a class for operations with <see cref="Workshop"/> images.
    /// </summary>
    public sealed class WorkshopImagesInteractionService :
        ChangeableImagesInteractionService<IWorkshopRepository, Workshop, Guid>,
        IWorkshopImagesInteractionService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopImagesInteractionService"/> class.
        /// </summary>
        /// <param name="imageService">Service for interacting with an image storage.</param>
        /// <param name="repository">Workshop repository.</param>
        /// <param name="limits">Describes limits of images for <see cref="Workshop"/>.</param>
        /// <param name="logger">Logger.</param>
        public WorkshopImagesInteractionService(IImageService imageService, IWorkshopRepository repository, ILogger<ImageInteractionBaseService<IWorkshopRepository, Workshop, Guid>> logger, IOptions<ImagesLimits<Workshop>> limits)
            : base(imageService, repository, logger, limits.Value)
        {
        }

        /// <inheritdoc/>
        protected override EntitySearchFilter<Workshop> GetFilterForSearchingEntityByIdWithIncludedImages(Guid entityId)
        {
            return new EntitySearchFilter<Workshop>(x => x.Id == entityId, nameof(Workshop.WorkshopImages));
        }

        /// <inheritdoc/>
        protected override List<Image<Workshop>> GetEntityImages(Workshop entity) => entity.WorkshopImages;
    }
}
