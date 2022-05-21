using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Config.Images;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    /// <summary>
    /// Represents a class for operations with images.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    public class ChangeableImagesInteractionMediator<TEntity> :
        ImageInteractionBaseMediator<TEntity>,
        IChangeableImagesInteractionMediator<TEntity>
        where TEntity : class, IKeyedEntity, IImageDependentEntity<TEntity>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeableImagesInteractionMediator{TEntity}"/> class.
        /// </summary>
        /// <param name="imageService">Service for interacting with an image storage.</param>
        /// <param name="limits">Describes limits of images for entities.</param>
        /// <param name="logger">Logger.</param>
        public ChangeableImagesInteractionMediator(
            IImageService imageService,
            ImagesLimits<TEntity> limits,
            ILogger<ChangeableImagesInteractionMediator<TEntity>> logger)
            : base(imageService, limits, logger)
        {
        }

        /// <inheritdoc/>
        public async Task<MultipleImageChangingResult> ChangeImagesAsync(TEntity entity, IList<string> oldImageIds, IList<IFormFile> newImages)
        {
            _ = entity ?? throw new ArgumentNullException(nameof(entity));
            _ = entity.Images ?? throw new NullReferenceException($"Entity {nameof(entity.Images)} cannot be null collection");
            oldImageIds ??= new List<string>();

            Logger.LogTrace("Changing images for the entity was started");

            var result = new MultipleImageChangingResult();

            var shouldRemove = !new HashSet<string>(oldImageIds).SetEquals(entity.Images.Select(x => x.ExternalStorageId));

            if (shouldRemove)
            {
                var removingList = entity.Images.Select(x => x.ExternalStorageId).Except(oldImageIds).ToList();
                if (removingList.Any())
                {
                    result.RemovedMultipleResult = await RemoveManyImagesProcessAsync(entity, removingList).ConfigureAwait(false);
                }
            }

            if (newImages?.Count > 0)
            {
                result.UploadedMultipleResult = await UploadManyImagesProcessAsync(entity, newImages).ConfigureAwait(false);
            }

            Logger.LogTrace("Changing images for the entity was finished");
            return result;
        }

        public async Task<ImageChangingResult> ChangeImageAsync(TEntity entity, string oldImageId, IFormFile newImage)
        {
            Logger.LogTrace("Updating an image the for entity was started");

            var result = new ImageChangingResult();

            if (!string.IsNullOrEmpty(oldImageId))
            {
                result.RemovingResult = await RemoveImageProcessAsync(entity, oldImageId).ConfigureAwait(false);
                if (!result.RemovingResult.Succeeded)
                {
                    return result;
                }
            }

            if (newImage != null)
            {
                result.UploadingResult = await UploadImageProcessAsync(entity, newImage).ConfigureAwait(false);
            }

            Logger.LogTrace("Updating an image for the entity was finished");
            return result;
        }
    }
}
