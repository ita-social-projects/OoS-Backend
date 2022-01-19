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
using OutOfSchool.WebApi.Common.SearchFilters;
using OutOfSchool.WebApi.Config.Images;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    /// <summary>
    /// Represents a class for operations with images.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TKey">The type of entity Id.</typeparam>
    public class ChangeableImagesInteractionService<TEntity, TKey> :
        ImageInteractionBaseService<TEntity, TKey>,
        IChangeableImagesInteractionService<TKey>
        where TEntity : class, IKeyedEntity<TKey>, IImageDependentEntity<TEntity>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeableImagesInteractionService{TEntity, TKey}"/> class.
        /// </summary>
        /// <param name="imageService">Service for interacting with an image storage.</param>
        /// <param name="repository">Repository with images.</param>
        /// <param name="limits">Describes limits of images for entities.</param>
        /// <param name="logger">Logger.</param>
        public ChangeableImagesInteractionService(
            IImageService imageService,
            IEntityRepositoryBase<TKey, TEntity> repository,
            ILogger<ChangeableImagesInteractionService<TEntity, TKey>> logger,
            ImagesLimits<TEntity> limits)
            : base(imageService, repository, limits, logger)
        {
        }

        /// <inheritdoc/>
        public async Task<ImageChangingResult> ChangeImagesAsync(TKey entityId, IList<string> oldImageIds, IList<IFormFile> newImages)
        {
            _ = oldImageIds ?? throw new ArgumentNullException(nameof(oldImageIds));

            Logger.LogDebug($"Changing images for entity [Id = {entityId}] was started.");
            var entity = await GetRequiredEntityWithIncludedImages(entityId).ConfigureAwait(false);

            var result = new ImageChangingResult();

            var shouldRemove = !new HashSet<string>(oldImageIds).SetEquals(entity.Images.Select(x => x.ExternalStorageId));

            if (shouldRemove)
            {
                var removingList = entity.Images.Select(x => x.ExternalStorageId).Except(oldImageIds).ToList();
                result.RemovedMultipleResult = await RemoveManyImagesProcessAsync(entity, removingList).ConfigureAwait(false);
            }

            if (result.RemovedMultipleResult is { HasResults: true })
            {
                entity = await GetRequiredEntityWithIncludedImages(entityId).ConfigureAwait(false);
            }

            if (newImages?.Count > 0)
            {
                result.UploadedMultipleResult = await UploadManyImagesProcessAsync(entity, newImages).ConfigureAwait(false);
            }

            Logger.LogDebug($"Changing images for entity [Id = {entityId}] was finished.");
            return result;
        }
    }
}
