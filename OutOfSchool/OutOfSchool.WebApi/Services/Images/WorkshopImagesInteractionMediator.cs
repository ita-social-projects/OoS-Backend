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
using OutOfSchool.WebApi.Config.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    /// <summary>
    /// Represents a class for operations with <see cref="Workshop"/> images.
    /// </summary>
    public sealed class WorkshopImagesInteractionMediator :
        ChangeableImagesInteractionMediator<Workshop>,
        IWorkshopImagesInteractionMediator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopImagesInteractionMediator"/> class.
        /// </summary>
        /// <param name="imageService">Service for interacting with an image storage.</param>
        /// <param name="limits">Describes limits of images for <see cref="Workshop"/>.</param>
        /// <param name="logger">Logger.</param>
        public WorkshopImagesInteractionMediator(IImageService imageService, ILogger<WorkshopImagesInteractionMediator> logger, IOptions<ImagesLimits<Workshop>> limits)
            : base(imageService, limits.Value, logger)
        {
        }
    }
}
